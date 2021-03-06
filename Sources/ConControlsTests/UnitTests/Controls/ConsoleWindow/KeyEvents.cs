﻿/*
 * (C) René Vogt
 *
 * Published under MIT license as described in the LICENSE.md file.
 *
 */

#nullable enable

using ConControls.ConsoleApi;
using ConControls.WindowsApi.Types;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable AccessToDisposedClosure

namespace ConControlsTests.UnitTests.Controls.ConsoleWindow
{
    public partial class ConsoleWindowTests
    {
        [TestMethod]
        public void KeyEvent_Handled_Ignored()
        {
            using var consoleController = new StubbedConsoleController();
            using var api = new StubbedNativeCalls();
            var graphicsProvider = new StubbedGraphicsProvider();
            using var sut = new ConControls.Controls.ConsoleWindow(api, consoleController, graphicsProvider);
            bool raised = false;
            sut.KeyEvent += (sender, e) =>
            {
                sender.Should().Be(sut);
                raised = true;
                e.Handled = true;
            };

            _ = new StubbedTextControl(sut);
            consoleController.KeyEventEvent(consoleController,
                                            new ConsoleKeyEventArgs(new KEY_EVENT_RECORD
                                            {
                                                ControlKeys = ControlKeyStates.None, KeyDown = 1, VirtualKeyCode = VirtualKey.Tab
                                            }));
            raised.Should().BeTrue();
            sut.FocusedControl.Should().BeNull();
        }
        [TestMethod]
        public void KeyEvent_Tab_FocusChanged()
        {
            using var consoleController = new StubbedConsoleController();
            using var api = new StubbedNativeCalls();
            var graphicsProvider = new StubbedGraphicsProvider();
            using var sut = new ConControls.Controls.ConsoleWindow(api, consoleController, graphicsProvider);

            var t1 = new ConControls.Controls.TextBlock(sut) { Parent = sut };
            var t2 = new ConControls.Controls.TextBlock(sut) { Parent = sut };
            var eventArgs = new ConsoleKeyEventArgs(new KEY_EVENT_RECORD
            {
                ControlKeys = ControlKeyStates.NUMLOCK_ON,
                KeyDown = 1,
                VirtualKeyCode = VirtualKey.Tab
            });
            consoleController.KeyEventEvent(consoleController,
                                            eventArgs);
            sut.FocusedControl.Should().Be(t1);
            consoleController.KeyEventEvent(consoleController,
                                            eventArgs);
            sut.FocusedControl.Should().Be(t2);
            consoleController.KeyEventEvent(consoleController,
                                            eventArgs);
            sut.FocusedControl.Should().Be(t1);
            consoleController.KeyEventEvent(consoleController,
                                            eventArgs);
            sut.FocusedControl.Should().Be(t2);
        }
        [TestMethod]
        public void KeyEvent_ShiftTab_FocusChanged()
        {
            using var consoleController = new StubbedConsoleController();
            using var api = new StubbedNativeCalls();
            var graphicsProvider = new StubbedGraphicsProvider();
            using var sut = new ConControls.Controls.ConsoleWindow(api, consoleController, graphicsProvider);

            var t1 = new ConControls.Controls.TextBlock(sut) { Parent = sut };
            var t2 = new ConControls.Controls.TextBlock(sut) { Parent = sut };
            var eventArgs = new ConsoleKeyEventArgs(new KEY_EVENT_RECORD
            {
                ControlKeys = ControlKeyStates.SCROLLLOCK_ON | ControlKeyStates.SHIFT_PRESSED,
                KeyDown = 1,
                VirtualKeyCode = VirtualKey.Tab
            });
            consoleController.KeyEventEvent(consoleController,
                                            eventArgs);
            sut.FocusedControl.Should().Be(t2);
            consoleController.KeyEventEvent(consoleController,
                                            eventArgs);
            sut.FocusedControl.Should().Be(t1);
            consoleController.KeyEventEvent(consoleController,
                                            eventArgs);
            sut.FocusedControl.Should().Be(t2);
            consoleController.KeyEventEvent(consoleController,
                                            eventArgs);
            sut.FocusedControl.Should().Be(t1);
        }
        [TestMethod]
        public void KeyEvent_UnhandledKey_Ignored()
        {
            using var consoleController = new StubbedConsoleController();
            using var api = new StubbedNativeCalls();
            var graphicsProvider = new StubbedGraphicsProvider();
            using var sut = new ConControls.Controls.ConsoleWindow(api, consoleController, graphicsProvider);

            _ = new StubbedTextControl(sut);
            consoleController.KeyEventEvent(consoleController,
                                            new ConsoleKeyEventArgs(new KEY_EVENT_RECORD
                                            {
                                                ControlKeys = ControlKeyStates.None,
                                                KeyDown = 1,
                                                VirtualKeyCode = VirtualKey.A
                                            }));
            sut.FocusedControl.Should().BeNull();
        }
        [TestMethod]
        public void KeyEvent_UnhandledKeyUp_Ignored()
        {
            using var consoleController = new StubbedConsoleController();
            using var api = new StubbedNativeCalls();
            var graphicsProvider = new StubbedGraphicsProvider();
            using var sut = new ConControls.Controls.ConsoleWindow(api, consoleController, graphicsProvider);

            _ = new StubbedTextControl(sut);
            consoleController.KeyEventEvent(consoleController,
                                            new ConsoleKeyEventArgs(new KEY_EVENT_RECORD
                                            {
                                                ControlKeys = ControlKeyStates.None,
                                                KeyDown = 0,
                                                VirtualKeyCode = VirtualKey.Tab
                                            }));
            sut.FocusedControl.Should().BeNull();
        }
        [TestMethod]
        public void KeyEvent_SwitchBuffersKey_BuffersSwitched()
        {
            bool active = true;
            using var consoleController = new StubbedConsoleController
            {
                ActiveScreenGet = () => active,
                ActiveScreenSetBoolean = b => active = b
            };
            using var api = new StubbedNativeCalls();
            var graphicsProvider = new StubbedGraphicsProvider();
            using var sut = new ConControls.Controls.ConsoleWindow(api, consoleController, graphicsProvider)
            {
                SwitchConsoleBuffersKey = ConControls.Controls.KeyCombination.F11
            };

            var e = new ConsoleKeyEventArgs(new KEY_EVENT_RECORD
            {
                KeyDown = 1,
                VirtualKeyCode = VirtualKey.F11
            });
            consoleController.KeyEventEvent(consoleController, e);
            active.Should().BeFalse();
            consoleController.KeyEventEvent(consoleController, e);
            active.Should().BeTrue();
            consoleController.KeyEventEvent(consoleController, e);
            active.Should().BeFalse();
        }
        [TestMethod]
        public void KeyEvent_CloseWindowKey_WindowClosed()
        {
            using var consoleController = new StubbedConsoleController();
            using var api = new StubbedNativeCalls();
            var graphicsProvider = new StubbedGraphicsProvider();
            using var sut = new ConControls.Controls.ConsoleWindow(api, consoleController, graphicsProvider)
            {
                CloseWindowKey = ConControls.Controls.KeyCombination.AltF4
            };
            consoleController.KeyEventEvent(consoleController,
                                            new ConsoleKeyEventArgs(new KEY_EVENT_RECORD
                                            {
                                                ControlKeys = ControlKeyStates.LEFT_ALT_PRESSED,
                                                KeyDown = 1,
                                                VirtualKeyCode = VirtualKey.F4
                                            }));

            sut.IsDisposed.Should().BeTrue();
        }
    }
}
