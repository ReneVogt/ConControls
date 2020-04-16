﻿/*
 * (C) René Vogt
 *
 * Published under MIT license as described in the LICENSE.md file.
 *
 */

#nullable enable

using System.Drawing;
using ConControls.Controls;
using ConControls.Controls.Drawing.Fakes;
using ConControls.Controls.Fakes;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConControlsTests.UnitTests.Controls.ConsoleControl
{
    public partial class ConsoleControlTests
    {
        [TestMethod]
        public void GetClientArea_BorderStyleNone_FullArea()
        {
            object syncLock = new object();
            var stubbedWindow = new StubIConsoleWindow
            {
                SynchronizationLockGet = () => syncLock,
                GetGraphics = () => new StubIConsoleGraphics()
            };
            stubbedWindow.WindowGet = () => stubbedWindow;
            var controlsCollection = new ConControls.Controls.ControlCollection(stubbedWindow);
            stubbedWindow.ControlsGet = () => controlsCollection;

            Rectangle area = new Rectangle(5, 5, 10, 10);
            Rectangle expectedClientArea = area;
            var sut = new TestControl(stubbedWindow)
            {
                Area = area,
                BorderStyle = BorderStyle.None
            };
            sut.ClientArea.Should().Be(expectedClientArea);
        }
        [TestMethod]
        public void GetClientArea_BorderStyleSingleLined_ClientArea()
        {
            object syncLock = new object();
            var stubbedWindow = new StubIConsoleWindow
            {
                SynchronizationLockGet = () => syncLock,
                GetGraphics = () => new StubIConsoleGraphics()
            };
            stubbedWindow.WindowGet = () => stubbedWindow;
            var controlsCollection = new ConControls.Controls.ControlCollection(stubbedWindow);
            stubbedWindow.ControlsGet = () => controlsCollection;

            Rectangle area = new Rectangle(5, 5, 10, 10);
            Rectangle expectedClientArea = new Rectangle(6, 6, 8, 8);
            var sut = new TestControl(stubbedWindow)
            {
                Area = area,
                BorderStyle = BorderStyle.SingleLined
            };
            sut.ClientArea.Should().Be(expectedClientArea);
        }
        [TestMethod]
        public void GetClientArea_BorderStyleDoubleLined_ClientArea()
        {
            object syncLock = new object();
            var stubbedWindow = new StubIConsoleWindow
            {
                SynchronizationLockGet = () => syncLock,
                GetGraphics = () => new StubIConsoleGraphics()
            };
            stubbedWindow.WindowGet = () => stubbedWindow;
            var controlsCollection = new ConControls.Controls.ControlCollection(stubbedWindow);
            stubbedWindow.ControlsGet = () => controlsCollection;

            Rectangle area = new Rectangle(5, 5, 10, 10);
            Rectangle expectedClientArea = new Rectangle(6, 6, 8, 8);
            var sut = new TestControl(stubbedWindow)
            {
                Area = area,
                BorderStyle = BorderStyle.DoubleLined
            };
            sut.ClientArea.Should().Be(expectedClientArea);
        }
        [TestMethod]
        public void GetClientArea_BorderStyleBold_ClientArea()
        {
            object syncLock = new object();
            var stubbedWindow = new StubIConsoleWindow
            {
                SynchronizationLockGet = () => syncLock,
                GetGraphics = () => new StubIConsoleGraphics()
            };
            stubbedWindow.WindowGet = () => stubbedWindow;
            var controlsCollection = new ConControls.Controls.ControlCollection(stubbedWindow);
            stubbedWindow.ControlsGet = () => controlsCollection;

            Rectangle area = new Rectangle(5, 5, 10, 10);
            Rectangle expectedClientArea = new Rectangle(6, 6, 8, 8);
            var sut = new TestControl(stubbedWindow)
            {
                Area = area,
                BorderStyle = BorderStyle.Bold
            };
            sut.ClientArea.Should().Be(expectedClientArea);
        }
    }
}
