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
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable AccessToDisposedClosure

namespace ConControlsTests.UnitTests.Controls.ConsoleWindow
{
    public partial class ConsoleWindowTests
    {
        [TestMethod]
        public void SizeEvents_AreaChangedRaised()
        {
            var api = new StubbedNativeCalls();
            using var controller = new StubbedConsoleController();
            var graphicsProvider = new StubbedGraphicsProvider();

            using var sut = new ConControls.Controls.ConsoleWindow(api, controller, graphicsProvider);
            bool raised = false;
            sut.AreaChanged += OnAreaChanged;
            controller.SizeEventEvent(controller, new ConsoleSizeEventArgs(Rectangle.Empty, Size.Empty));
            raised.Should().BeTrue();
            raised = false;
            sut.AreaChanged -= OnAreaChanged;
            controller.SizeEventEvent(controller, new ConsoleSizeEventArgs(Rectangle.Empty, Size.Empty));
            raised.Should().BeFalse();

            void OnAreaChanged(object sender, EventArgs e)
            {
                sender.Should().Be(sut);
                raised = true;
            }
        }
    }
}
