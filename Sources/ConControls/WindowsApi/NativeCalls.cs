﻿/*
 * (C) René Vogt
 *
 * Published under MIT license as described in the LICENSE.md file.
 *
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ConControls.WindowsApi.Types;

namespace ConControls.WindowsApi
{
    [ExcludeFromCodeCoverage]
    sealed class NativeCalls : INativeCalls
    {
        public ConsoleOutputHandle CreateConsoleScreenBuffer() => new ConsoleOutputHandle(NativeMethods.CreateConsoleScreenBuffer(
                                                                                              AccessRights.GenericRead | AccessRights.GenericWrite,
                                                                                              FileShare.Read | FileShare.Write, IntPtr.Zero,
                                                                                              ConsoleBufferMode.Text, IntPtr.Zero));
        public ConsoleInputModes GetConsoleMode(ConsoleInputHandle consoleInputHandle) =>
            NativeMethods.GetConsoleMode(consoleInputHandle.DangerousGetHandle(), out ConsoleInputModes mode) ? mode : throw Exceptions.Win32();
        public ConsoleOutputModes GetConsoleMode(ConsoleOutputHandle consoleOutputHandle) =>
            NativeMethods.GetConsoleMode(consoleOutputHandle.DangerousGetHandle(), out ConsoleOutputModes mode) ? mode : throw Exceptions.Win32();
        public CONSOLE_SCREEN_BUFFER_INFOEX GetConsoleScreenBufferInfo(ConsoleOutputHandle consoleOutputHandle)
        {
            CONSOLE_SCREEN_BUFFER_INFOEX info = new CONSOLE_SCREEN_BUFFER_INFOEX
            {
                Size = Marshal.SizeOf<CONSOLE_SCREEN_BUFFER_INFOEX>()
            };
            if (!NativeMethods.GetConsoleScreenBufferInfoEx(consoleOutputHandle, ref info))
                throw Exceptions.Win32();
            return info;
        }
        public string GetConsoleTitle()
        {
            StringBuilder titleBuilder = new StringBuilder(1024);
            if (NativeMethods.GetConsoleTitle(titleBuilder, 1024) <= 0)
                throw Exceptions.Win32();
            return titleBuilder.ToString();
        }
        public (bool visible, int size, Point position) GetCursorInfo(ConsoleOutputHandle consoleOutputHandle)
        {
            if (!NativeMethods.GetConsoleCursorInfo(consoleOutputHandle, out var info)) 
                throw Exceptions.Win32();
            var infoEx = new CONSOLE_SCREEN_BUFFER_INFOEX
            {
                Size = Marshal.SizeOf<CONSOLE_SCREEN_BUFFER_INFOEX>()
            };
            if (!NativeMethods.GetConsoleScreenBufferInfoEx(consoleOutputHandle, ref infoEx))
                throw Exceptions.Win32();

            return (info.Visible, info.Size, new Point(infoEx.CursorPosition.X, infoEx.CursorPosition.Y));
        }

        public ConsoleInputHandle GetInputHandle() =>
            new ConsoleInputHandle(NativeMethods.GetStdHandle(NativeMethods.STDIN));
        public ConsoleOutputHandle GetOutputHandle() =>
            new ConsoleOutputHandle(NativeMethods.GetStdHandle(NativeMethods.STDOUT));

        public INPUT_RECORD[] ReadConsoleInput(ConsoleInputHandle consoleInputHandle, int maxElements = 1028)
        {
            INPUT_RECORD[] result = new INPUT_RECORD[maxElements];
            if (!NativeMethods.ReadConsoleInput(consoleInputHandle, result, maxElements, out var read))
                throw Exceptions.Win32();
            return result.Take(read).ToArray();
        }
        public CHAR_INFO[] ReadConsoleOutput(ConsoleOutputHandle consoleOutputHandle, Rectangle region)
        {
            SMALL_RECT rect = new SMALL_RECT(region);
            CHAR_INFO[] buffer = new CHAR_INFO[region.Width * region.Height];
            if (!NativeMethods.ReadConsoleOutput(consoleOutputHandle, buffer, new COORD(region), default, ref rect))
                throw Exceptions.Win32();
            return buffer;
        }
        public bool SetActiveConsoleScreenBuffer(ConsoleOutputHandle handle) => NativeMethods.SetConsoleActiveScreenBuffer(handle);
        public void SetConsoleMode(ConsoleInputHandle consoleInputHandle, ConsoleInputModes inputMode)
        {
            if (!NativeMethods.SetConsoleMode(consoleInputHandle, inputMode))
                throw Exceptions.Win32();
        }
        public void SetConsoleMode(ConsoleOutputHandle consoleOutputHandle, ConsoleOutputModes outputMode)
        {
            if (!NativeMethods.SetConsoleMode(consoleOutputHandle, outputMode))
                throw Exceptions.Win32();
        }
        public void SetConsoleScreenBufferSize(ConsoleOutputHandle consoleOutputHandle, Size size)
        {
            if (!NativeMethods.SetConsoleScreenBufferSize(consoleOutputHandle, new COORD(size)))
                throw Exceptions.Win32();
        }

        public void SetConsoleWindowSize(ConsoleOutputHandle consoleOutputHandle, Size size)
        {
            SMALL_RECT rect = new SMALL_RECT(size);
            if (!NativeMethods.SetConsoleWindowInfo(consoleOutputHandle, true, ref rect))
                throw Exceptions.Win32();
        }

        public void SetConsoleTitle(string title)
        {
            if (!NativeMethods.SetConsoleTitle(title))
                throw Exceptions.Win32();
        }
        public void SetCursorInfo(ConsoleOutputHandle consoleOutputHandle, bool visible, int size, Point position)
        {
            if (!NativeMethods.SetConsoleCursorPosition(consoleOutputHandle, new COORD(position)))
                throw Exceptions.Win32();
            CONSOLE_CURSOR_INFO info = new CONSOLE_CURSOR_INFO
            {
                Size = size,
                Visible = visible
            };
            if (!NativeMethods.SetConsoleCursorInfo(consoleOutputHandle, ref info))
                throw Exceptions.Win32();
        }
        public void SetInputHandle(ConsoleInputHandle inputHandle)
        {
            if (!NativeMethods.SetStdHandle(NativeMethods.STDIN, inputHandle.DangerousGetHandle()))
                throw Exceptions.Win32();
        }
        public void SetOutputHandle(ConsoleOutputHandle outputHandle)
        {
            if (!NativeMethods.SetStdHandle(NativeMethods.STDOUT, outputHandle.DangerousGetHandle()))
                throw Exceptions.Win32();
        }
        public void WriteConsoleOutput(ConsoleOutputHandle consoleOutputHandle, CHAR_INFO[] buffer, Rectangle region)
        {
            SMALL_RECT rect = new SMALL_RECT(region);
            if (!NativeMethods.WriteConsoleOutput(consoleOutputHandle, buffer, new COORD(region), default, ref rect))
                throw Exceptions.Win32();
        }
    }
}
