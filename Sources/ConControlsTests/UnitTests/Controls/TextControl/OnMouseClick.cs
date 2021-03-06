﻿/*
 * (C) René Vogt
 *
 * Published under MIT license as described in the LICENSE.md file.
 *
 */

#nullable enable

using System;
using System.Drawing;
using ConControls.ConsoleApi;
using ConControls.Controls;
using ConControls.Extensions;
using ConControls.WindowsApi.Types;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConControlsTests.UnitTests.Controls.TextControl
{
    public partial class TextControlTests
    {
        [TestMethod]
        public void OnMouseClick_Null_ArgumentNullException()
        {
            using var stubbedWindow = new StubbedWindow();
            var stubbedController = new StubbedConsoleTextController();
            using var sut = new StubbedTextControl(stubbedWindow, stubbedController);
            sut.Invoking(s => s.CallOnMouseClick(null!)).Should().Throw<ArgumentNullException>();
        }
        [TestMethod]
        public void OnMouseClick_LeftClickedOutsideArea_Notthing()
        {
            using var stubbedWindow = new StubbedWindow();
            var stubbedController = new StubbedConsoleTextController
            {
                BufferLineCountGet = () => 20,
                MaxLineLengthGet = () => 20,
                WidthGet = () => 20,
                GetLineLengthInt32 = l => 20
            };
            using var sut = new StubbedTextControl(stubbedWindow, stubbedController)
            {
                Area = (5, 5, 10, 10).Rect(),
                Parent = stubbedWindow,
                Scroll = (3, 3).Pt()
            };
            var e = new MouseEventArgs(new ConsoleMouseEventArgs(new MOUSE_EVENT_RECORD
            {
                MousePosition = new COORD(4, 4),
                ButtonState = MouseButtonStates.LeftButtonPressed
            }));
            stubbedWindow.MouseEventEvent(stubbedWindow, e);
            sut.Caret.Should().Be(Point.Empty);
            sut.Focused.Should().BeFalse();
            e.Handled.Should().BeFalse();
        }
        [TestMethod]
        public void OnMouseClick_LeftClickedHandled_Notthing()
        {
            using var stubbedWindow = new StubbedWindow();
            var stubbedController = new StubbedConsoleTextController
            {
                BufferLineCountGet = () => 20,
                MaxLineLengthGet = () => 20,
                WidthGet = () => 20,
                GetLineLengthInt32 = l => 20
            };
            using var sut = new StubbedTextControl(stubbedWindow, stubbedController)
            {
                Area = (5, 5, 10, 10).Rect(),
                Parent = stubbedWindow,
                Scroll = (3, 3).Pt()
            };
            var e = new MouseEventArgs(new ConsoleMouseEventArgs(new MOUSE_EVENT_RECORD
            {
                MousePosition = new COORD(10, 10),
                ButtonState = MouseButtonStates.LeftButtonPressed
            })) {Handled = true};
            sut.CallOnMouseClick(e);
            sut.Caret.Should().Be(Point.Empty);
            sut.Focused.Should().BeFalse();
        }
        [TestMethod]
        public void OnMouseClick_LeftClickedDisabled_Nothing()
        {
            using var stubbedWindow = new StubbedWindow();
            var stubbedController = new StubbedConsoleTextController
            {
                BufferLineCountGet = () => 20,
                MaxLineLengthGet = () => 20,
                WidthGet = () => 20,
                GetLineLengthInt32 = l => 20
            };
            using var sut = new StubbedTextControl(stubbedWindow, stubbedController)
            {
                Area = (5, 5, 10, 10).Rect(),
                Parent = stubbedWindow,
                Scroll = (3, 3).Pt(),
                Enabled = false
            };
            var e = new MouseEventArgs(new ConsoleMouseEventArgs(new MOUSE_EVENT_RECORD
            {
                MousePosition = new COORD(10, 10),
                ButtonState = MouseButtonStates.LeftButtonPressed
            }));
            sut.CallOnMouseClick(e);
            sut.Caret.Should().Be(Point.Empty);
            sut.Focused.Should().BeFalse();
            e.Handled.Should().BeFalse();
        }
        [TestMethod]
        public void OnMouseClick_LeftClickedInvisible_Nothing()
        {
            using var stubbedWindow = new StubbedWindow();
            var stubbedController = new StubbedConsoleTextController
            {
                BufferLineCountGet = () => 20,
                MaxLineLengthGet = () => 20,
                WidthGet = () => 20,
                GetLineLengthInt32 = l => 20
            };
            using var sut = new StubbedTextControl(stubbedWindow, stubbedController)
            {
                Area = (5, 5, 10, 10).Rect(),
                Parent = stubbedWindow,
                Scroll = (3, 3).Pt(),
                Visible = false
            };
            var e = new MouseEventArgs(new ConsoleMouseEventArgs(new MOUSE_EVENT_RECORD
            {
                MousePosition = new COORD(10, 10),
                ButtonState = MouseButtonStates.LeftButtonPressed
            }));
            sut.CallOnMouseClick(e);
            sut.Caret.Should().Be(Point.Empty);
            sut.Focused.Should().BeFalse();
            e.Handled.Should().BeFalse();
        }
        [TestMethod]
        public void OnMouseClick_RightClicked_Nothing()
        {
            using var stubbedWindow = new StubbedWindow();
            var stubbedController = new StubbedConsoleTextController
            {
                BufferLineCountGet = () => 20,
                MaxLineLengthGet = () => 20,
                WidthGet = () => 20,
                GetLineLengthInt32 = l => 20
            };
            using var sut = new StubbedTextControl(stubbedWindow, stubbedController)
            {
                Area = (5, 5, 10, 10).Rect(),
                Parent = stubbedWindow,
                Scroll = (3, 3).Pt()
            };
            var e = new MouseEventArgs(new ConsoleMouseEventArgs(new MOUSE_EVENT_RECORD
            {
                MousePosition = new COORD(10, 10),
                ButtonState = MouseButtonStates.RightButtonPressed
            }));
            sut.CallOnMouseClick(e);
            sut.Caret.Should().Be(Point.Empty);
            sut.Focused.Should().BeFalse();
            e.Handled.Should().BeFalse();
        }
        [TestMethod]
        public void OnMouseClick_LeftClicked_FocusedAndCaretSet()
        {
            ConControls.Controls.ConsoleControl? focused = null;
            using var stubbedWindow = new StubbedWindow
            {
                FocusedControlGet = () => focused,
                FocusedControlSetConsoleControl = c => focused = c
            };
            var stubbedController = new StubbedConsoleTextController
            {
                BufferLineCountGet = () => 20,
                MaxLineLengthGet = () => 20,
                WidthGet = () => 20,
                GetLineLengthInt32 = l => 20
            };
            using var sut = new ConControls.Controls.TextBlock(stubbedWindow, stubbedController)
            {
                Area = (5, 5, 10, 10).Rect(),
                Parent = stubbedWindow,
                Scroll = (3, 2).Pt()
            };
            var e = new MouseEventArgs(new ConsoleMouseEventArgs(new MOUSE_EVENT_RECORD
            {
                MousePosition = new COORD(10, 10),
                ButtonState = MouseButtonStates.LeftButtonPressed
            }));
            sut.Focused.Should().BeFalse();
            focused.Should().BeNull();
            stubbedWindow.MouseEventEvent(stubbedWindow, e);
            sut.Caret.Should().Be(new Point(8, 7));
            sut.Focused.Should().BeTrue();
            focused.Should().Be(sut);
            e.Handled.Should().BeTrue();
        }
    }
}
