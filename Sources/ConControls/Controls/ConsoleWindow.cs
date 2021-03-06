﻿/*
 * (C) René Vogt
 *
 * Published under MIT license as described in the LICENSE.md file.
 *
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConControls.ConsoleApi;
using ConControls.Controls.Drawing;
using ConControls.Helpers;
using ConControls.Logging;
using ConControls.WindowsApi;
using ConControls.WindowsApi.Types;

namespace ConControls.Controls
{
    /// <summary>
    /// The window for a console UI session.
    /// </summary>
    /// <remarks>This window takes control over the console input buffer and
    /// the current screen buffer to provide UI functionality. Only one window
    /// may be instantiated at a time. Make sure to dispose of any previously
    /// instantiated contexts before creating a new one.
    /// </remarks>
    /// <threadsafety>
    /// All public properties and methods are sychronized using the window's <see cref="SynchronizationLock"/>.
    /// </threadsafety>
    public sealed class ConsoleWindow : IConsoleWindow, IControlContainer, IDisposable
    {
        static int instancesCreated;
        
        readonly INativeCalls api;
        readonly IConsoleController consoleController;
        readonly IProvideConsoleGraphics graphicsProvider;
        readonly bool originalCursorVisible;
        readonly int originalCursorSize;
#pragma warning disable CA2213
        readonly DisposableBlock drawingInhibiter;
#pragma warning restore CA2213
        readonly TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();

        int isDisposed;
        int inhibitDrawing;
        FrameCharSets frameCharSets = new FrameCharSets();
        ConsoleControl? focusedControl;

        ConsoleColor defaultForegroundColor = ConsoleColor.Gray;
        ConsoleColor defaultBackgroundColor = ConsoleColor.Black;
        ConsoleColor defaultBorderColor = ConsoleColor.Yellow;
        int defaultCursorSize;

        /// <inheritdoc />
        public event EventHandler? AreaChanged;
        /// <inheritdoc />
        public event EventHandler<KeyEventArgs>? KeyEvent;
        /// <inheritdoc />
        public event EventHandler<MouseEventArgs>? MouseEvent;
        /// <inheritdoc />
        public event EventHandler? Disposed;

        /// <inheritdoc />
        public IConsoleWindow Window => this;
        /// <inheritdoc />
        public string Title
        {
            get => api.GetConsoleTitle();
            set => api.SetConsoleTitle(value ?? string.Empty);
        }
        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public Point Location { get; } = Point.Empty;
        /// <inheritdoc />
        public Size Size
        {
            get
            {
                var info = api.GetConsoleScreenBufferInfo(consoleController.OutputHandle);
                return new Size(info.BufferSize.X, info.BufferSize.Y);
            }
            set
            {
                lock (SynchronizationLock)
                {
                    // temporarily set the window size to the smaller buffer size to avoid errors
                    var info = api.GetConsoleScreenBufferInfo(consoleController.OutputHandle);
                    var size = new Size(Math.Min(Math.Min(value.Width, info.BufferSize.X), info.MaximumWindowSize.X),
                                        Math.Min(Math.Min(value.Height, info.BufferSize.Y), info.MaximumWindowSize.Y));
                    api.SetConsoleWindowSize(consoleController.OutputHandle, size);
                    api.SetConsoleScreenBufferSize(consoleController.OutputHandle, value);
                    info = api.GetConsoleScreenBufferInfo(consoleController.OutputHandle);
                    size = new Size(Math.Max(0, Math.Min(value.Width, info.MaximumWindowSize.X)), Math.Max(0, Math.Min(value.Height, info.MaximumWindowSize.Y)));
                    api.SetConsoleWindowSize(consoleController.OutputHandle, size);
                    Invalidate();
                }
            }
        }
        /// <inheritdoc />
        public Rectangle Area => new Rectangle(Point.Empty, Size);

        /// <inheritdoc />
        public int ExitCode { get; private set; }

        /// <inheritdoc />
        public ConsoleColor DefaultForegroundColor
        {
            get => defaultForegroundColor;
            set
            {
                lock (SynchronizationLock)
                {
                    if (value == defaultForegroundColor) return;
                    defaultForegroundColor = value;
                    Invalidate();
                }
            }
        }
        /// <inheritdoc />
        public ConsoleColor DefaultBackgroundColor
        {
            get => defaultBackgroundColor;
            set
            {
                lock (SynchronizationLock)
                {
                    if (value == defaultBackgroundColor) return;
                    defaultBackgroundColor = value;
                    Invalidate();
                }
            }
        }
        /// <inheritdoc />
        public ConsoleColor DefaultBorderColor
        {
            get => defaultBorderColor;
            set
            {
                lock (SynchronizationLock)
                {
                    if (value == defaultBorderColor) return;
                    defaultBorderColor = value;
                    Invalidate();
                }
            }
        }
        /// <inheritdoc />
        public int DefaultCursorSize
        {
            get => defaultCursorSize;
            set
            {
                lock (SynchronizationLock)
                {
                    if (value == defaultCursorSize) return;
                    defaultCursorSize = value;
                    Invalidate();
                }
            }
        }
        /// <inheritdoc />
        public ControlCollection Controls { get; }
        /// <inheritdoc />
        public ConsoleControl? FocusedControl 
        {
            get
            {
                lock (SynchronizationLock) return focusedControl;
            }
            set
            {
                lock (SynchronizationLock)
                {
                    if (value == focusedControl) return;
                    if (value?.CanFocus == false) throw Exceptions.CannotFocusUnFocusableControl(value.GetType().Name);
                    var oldFocused = focusedControl;
                    focusedControl = null;
                    if (oldFocused != null)
                    {
                        oldFocused.CursorPositionChanged -= OnFocusedControlCursorChanged;
                        oldFocused.CursorSizeChanged -= OnFocusedControlCursorChanged;
                        oldFocused.CursorVisibleChanged -= OnFocusedControlCursorChanged;
                        oldFocused.Focused = false;
                    }
                    focusedControl = value;
                    UpdateCursor();
                    if (focusedControl == null) return;
                    focusedControl.Focused = true;
//                    api.SetCursorInfo(consoleController.OutputHandle, focusedControl.CursorVisible, GetCursorSizeForControl(focusedControl), focusedControl.PointToConsole(focusedControl.CursorPosition));
                    focusedControl.CursorPositionChanged += OnFocusedControlCursorChanged;
                    focusedControl.CursorSizeChanged += OnFocusedControlCursorChanged;
                    focusedControl.CursorVisibleChanged += OnFocusedControlCursorChanged;
                }

            }
        }
        /// <inheritdoc />
        public FrameCharSets FrameCharSets
        {
            get
            {
                lock (SynchronizationLock) return frameCharSets;
            }
            set
            {
                lock (SynchronizationLock)
                {
                    if (frameCharSets == value) return;
                    frameCharSets = value ?? throw new ArgumentNullException(nameof(FrameCharSets));
                    OnFrameCharSetsChanged();
                }
            }
        }
        /// <inheritdoc />
        public KeyCombination? SwitchConsoleBuffersKey { get; set; }
        /// <inheritdoc />
        public KeyCombination? CloseWindowKey { get; set; }
        /// <inheritdoc />
        public bool ActiveScreen
        {
            get
            {
                lock (SynchronizationLock) return consoleController.ActiveScreen;
            }
            set
            {
                lock (SynchronizationLock) consoleController.ActiveScreen = value;
            }
        }

        /// <inheritdoc />
        public bool DrawingInhibited => !Visible || inhibitDrawing > 0;
        /// <inheritdoc />
        public bool Enabled => !IsDisposed;
        /// <inheritdoc />
        public bool Visible => !IsDisposed;
        /// <inheritdoc />
        public bool IsDisposed => isDisposed != 0;
        /// <inheritdoc />
        public object SynchronizationLock { get; } = new object();

        /// <summary>
        /// Opens a new <see cref="ConsoleWindow"/>. Only one instance can exist at a time.
        /// Be sure to dispose of any previously instantiated contexts.
        /// </summary>
        /// <exception cref="InvalidOperationException">A previously instantiated <see cref="ConsoleWindow"/> has not yet been disposed of. Only a single window can exist at a time.</exception>
        [ExcludeFromCodeCoverage]
        public ConsoleWindow()
            : this(null, null, null) { }
        internal ConsoleWindow(INativeCalls? api, IConsoleController? consoleController, IProvideConsoleGraphics? graphicsProvider)
        {
            if (Interlocked.CompareExchange(ref instancesCreated, 1, 0) != 0)
                throw Exceptions.CanOnlyUseSingleContext();

            drawingInhibiter = new DisposableBlock(EndDeferDrawing);
            this.api = api ?? new NativeCalls();
            this.consoleController = consoleController ?? new ConsoleController(this.api);
            this.graphicsProvider = graphicsProvider ?? new ConsoleGraphicsProvider();

            this.consoleController.FocusEvent += OnConsoleControllerFocusReceived;
            this.consoleController.KeyEvent += OnConsoleControllerKeyReceived;
            this.consoleController.MenuEvent += OnConsoleControllerMenuReceived;
            this.consoleController.MouseEvent += OnConsoleControllerMouseReceived;
            this.consoleController.SizeEvent += OnConsoleControllerSizeReceived;

            Controls = new ControlCollection(this);
            Controls.ControlCollectionChanged += OnControlCollectionChanged;

            (originalCursorVisible, originalCursorSize, _) = this.api.GetCursorInfo(this.consoleController.OutputHandle);
            defaultCursorSize = originalCursorSize;
            this.api.SetCursorInfo(this.consoleController.OutputHandle, false, DefaultCursorSize, Point.Empty);

            Invalidate();
        }
        /// <summary>
        /// Cleans up native resources.
        /// </summary>
        [ExcludeFromCodeCoverage]
        ~ConsoleWindow()
        {
            Dispose(false);
        }
        /// <summary>
        /// Releases any resources used by this <see cref="ConsoleWindow"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="ConsoleWindow.Dispose()"/> method releases any resources used by this instance.
        /// It allows to create a new instance (only one <see cref="ConsoleWindow"/> instance can be alive
        /// at a time) and tries to reset the console to the state before the creation of this instacne.</remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        void Dispose(bool disposing)
        {
            if (Interlocked.CompareExchange(ref isDisposed, 1, 0) != 0) return;
            if (disposing)
            {
                taskCompletionSource.SetResult(ExitCode);
                api.SetCursorInfo(consoleController.OutputHandle, originalCursorVisible, originalCursorSize, Point.Empty);
                consoleController.FocusEvent -= OnConsoleControllerFocusReceived;
                consoleController.KeyEvent -= OnConsoleControllerKeyReceived;
                consoleController.MenuEvent -= OnConsoleControllerMenuReceived;
                consoleController.MouseEvent -= OnConsoleControllerMouseReceived;
                consoleController.SizeEvent -= OnConsoleControllerSizeReceived;
                consoleController.Dispose();
            }
            Interlocked.Decrement(ref instancesCreated);
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public void Close(int exitCode = 0)
        {
            lock (SynchronizationLock)
            {
                ExitCode = exitCode;
                Dispose();
            }
        }
        /// <inheritdoc />
        public async Task<int> WaitForCloseAsync() => await taskCompletionSource.Task.ConfigureAwait(false);

        /// <inheritdoc />
        public Point PointToClient(Point consolePoint) => consolePoint;
        /// <inheritdoc />
        public Point PointToConsole(Point clientPoint) => clientPoint;

        /// <inheritdoc />
        public IConsoleGraphics GetGraphics() => graphicsProvider.Provide(consoleController.OutputHandle, api, Size, frameCharSets);

        void Draw()
        {
            Logger.Log(DebugContext.Window | DebugContext.Drawing, "called.");
            lock (SynchronizationLock)
            {
                if (DrawingInhibited)
                {
                    Logger.Log(DebugContext.Window | DebugContext.Drawing, "drawing inhibited.");
                    return;
                }
                var graphics = GetGraphics();
                var rect = new Rectangle(Point.Empty, Size);
                Logger.Log(DebugContext.Window | DebugContext.Drawing, $"drawing background at {rect}.");
                graphics.DrawBackground(DefaultBackgroundColor, rect);
                Logger.Log(DebugContext.Window | DebugContext.Drawing, "drawing controls.");
                foreach(var control in Controls)
                    control.Draw(graphics);
                Logger.Log(DebugContext.Window | DebugContext.Drawing, "flushing.");
                graphics.Flush();
                UpdateCursor();
            }
        }
        /// <summary>
        /// Invalidates the complete window to trigger a complete redrawing of
        /// the controls.
        /// </summary>
        public void Invalidate()
        {
            Draw();
        }
        /// <inheritdoc />
        public IDisposable DeferDrawing()
        {
            Interlocked.Increment(ref inhibitDrawing);
            return drawingInhibiter;
        }
        void EndDeferDrawing()
        {
            if (Interlocked.Decrement(ref inhibitDrawing) <= 0)
                Invalidate();
        }
        void OnFrameCharSetsChanged()
        {
            Invalidate();
        }
        void OnControlCollectionChanged(object sender, ControlCollectionChangedEventArgs e)
        {
            lock (SynchronizationLock)
            {
                using(DeferDrawing())
                {
                    foreach (var control in e.AddedControls)
                        control.AreaChanged += OnControlAreaChanged;

                    foreach (var control in e.RemovedControls)
                        control.AreaChanged -= OnControlAreaChanged;
                }
            }
        }
        void OnControlAreaChanged(object sender, EventArgs e) => Invalidate();
        void OnFocusedControlCursorChanged(object sender, EventArgs e)
        {
            UpdateCursor();
        }
        void UpdateCursor()
        {
            var control = focusedControl;
            api.SetCursorInfo(consoleController.OutputHandle, control?.CursorVisible ?? false, GetCursorSizeForControl(control),
                              control?.PointToConsole(control.CursorPosition) ?? Point.Empty);

        }
        /// <inheritdoc />
        public ConsoleControl? FocusNext()
        {
            var focusableControls = GetTabFocusableControls(Controls).ToList();
            if (focusedControl == null)
            {
                FocusedControl = focusableControls.FirstOrDefault();
                return FocusedControl;
            }

            FocusedControl = focusableControls.Count > 0
                                 ? focusableControls[(focusableControls.IndexOf(focusedControl) + 1) % focusableControls.Count]
                                 : null;
            return FocusedControl;
        }
        /// <inheritdoc />
        public ConsoleControl? FocusPrevious()
        {
            var focusableControls = GetTabFocusableControls(Controls).ToList();
            if (focusedControl == null)
            {
                FocusedControl = focusableControls.LastOrDefault();
                return FocusedControl;
            }

            FocusedControl = focusableControls.Count > 0
                                 ? focusableControls[(focusableControls.IndexOf(focusedControl) + focusableControls.Count - 1) % focusableControls.Count]
                                 : null;
            return FocusedControl;
        }
        static IEnumerable<ConsoleControl> GetTabFocusableControls(ControlCollection controls)
        {
            var orderedControls = controls
                                  .Select((control, index) => (control, index))
                                  .OrderBy(x => x.control.TabOrder)
                                  .ThenBy(x => x.index)
                                  .Select(x => x.control);

            foreach (var control in orderedControls)
            {
                if (control.Enabled && control.CanFocus && control.TabStop)
                    yield return control;
                foreach (var focusableChild in GetTabFocusableControls(control.Controls))
                    yield return focusableChild;
            }
        }
        [ExcludeFromCodeCoverage]
        void OnConsoleControllerFocusReceived(object sender, ConsoleFocusEventArgs e)
        {
            lock (SynchronizationLock)
            {
                Logger.Log(DebugContext.Window, $"Received focus event: SetFocus: {e.SetFocus}");
            }
        }
        void OnConsoleControllerKeyReceived(object sender, ConsoleKeyEventArgs e)
        {
            lock (SynchronizationLock)
            {
                Logger.Log(DebugContext.Window | DebugContext.Keys,
                           $"Received key event: VK {e.VirtualKeyCode} UC '{e.UnicodeChar}' Down: {e.KeyDown} CK {e.ControlKeys} RC {e.RepeatCount}");

                var eventArgs = new KeyEventArgs(e);
                KeyEvent?.Invoke(this, eventArgs);
                if (eventArgs.Handled || !eventArgs.KeyDown) return;

                if (eventArgs.VirtualKey == VirtualKey.Tab)
                {
                    var controlKeys = eventArgs.ControlKeys.WithoutSwitches();
                    if (controlKeys == ControlKeyStates.SHIFT_PRESSED)
                        FocusPrevious();
                    else if (controlKeys == ControlKeyStates.None)
                        FocusNext();
                }

                if (e.Matches(CloseWindowKey))
                    Close();
                if (e.Matches(SwitchConsoleBuffersKey))
                    ActiveScreen = !ActiveScreen;
            }
        }
        [ExcludeFromCodeCoverage]
        void OnConsoleControllerMenuReceived(object sender, ConsoleMenuEventArgs e)
        {
            lock (SynchronizationLock)
            {
                Logger.Log(DebugContext.Window, $"Received menu event: command: {e.CommandId}");
            }
        }
        void OnConsoleControllerMouseReceived(object sender, ConsoleMouseEventArgs e)
        {
            lock (SynchronizationLock)
            {
                Logger.Log(DebugContext.Window | DebugContext.Mouse,
                           $"Received mouse event: [{e.EventFlags}] at {e.MousePosition} button '{e.ButtonState}' CK {e.ControlKeys} Scroll: {e.Scroll}");
                MouseEvent?.Invoke(this, new MouseEventArgs(e));
            }
        }
        void OnConsoleControllerSizeReceived(object sender, ConsoleSizeEventArgs e)
        {
            lock (SynchronizationLock)
            {
                Logger.Log(DebugContext.Window,
                           $"Received size event: Window = {e.WindowArea} Buffer = {e.BufferSize}");
                using(DeferDrawing())
                    AreaChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        int GetCursorSizeForControl(ConsoleControl? control)
        {
            int? size = control?.CursorSize;
            if (size.HasValue) return size.Value;
            var parent = control?.Parent;
            while (parent != null && size == null)
            {
                size = parent.CursorSize;
                parent = parent.Parent;
            }

            return size ?? DefaultCursorSize;
        }

        IControlContainer? IControlContainer.Parent => null;
        ConsoleColor? IControlContainer.ForegroundColor
        {
            get => DefaultForegroundColor;
            set => DefaultForegroundColor = value ?? DefaultForegroundColor;
        }
        ConsoleColor? IControlContainer.BackgroundColor
        {
            get => DefaultBackgroundColor;
            set => DefaultBackgroundColor = value ?? DefaultBackgroundColor;
        }
        ConsoleColor? IControlContainer.BorderColor
        {
            get => DefaultBorderColor;
            set => DefaultBorderColor = value ?? DefaultBorderColor;
        }
        int? IControlContainer.CursorSize
        {
            get => DefaultCursorSize;
            set => DefaultCursorSize = value ?? DefaultCursorSize;
        }
    }
}
