﻿/*
 * (C) René Vogt
 *
 * Published under MIT license as described in the LICENSE.md file.
 *
 */

#nullable enable

// ReSharper disable AccessToDisposedClosure

using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConControlsTests.UnitTests.ConsoleApi.ConsoleController
{
    public partial class ConsoleControllerTests
    {
        [TestMethod]
        public async Task ThreadManagement_ThreadStartedAndStoppedCorrectly()
        {
            TaskCompletionSource<int> startTaskSource = new TaskCompletionSource<int>();
            TaskCompletionSource<int> endTaskSource = new TaskCompletionSource<int>();
            bool threadStartLogged = false, threadEndLogged = false;

            using var logger = new TestLogger(CheckLog);

            using var api = new StubbedNativeCalls();
            var sut = new ConControls.ConsoleApi.ConsoleController(api);
            (await Task.WhenAny(startTaskSource.Task, Task.Delay(2000)))
                .Should()
                .Be(startTaskSource.Task, "Thread should be started in less than 2 seconds!");
            threadStartLogged.Should().BeTrue();
            sut.Dispose();
            (await Task.WhenAny(endTaskSource.Task, Task.Delay(2000)))
                .Should()
                .Be(endTaskSource.Task, "Thread should be stopped in less than 2 seconds!");
            threadEndLogged.Should().BeTrue();
            sut.Dispose(); // should not fail

            void CheckLog(string msg)
            {
                if (msg.Contains("Starting thread."))
                {
                    threadStartLogged = true;
                    startTaskSource.SetResult(0);
                }

                if (msg.Contains("Stopping thread"))
                {
                    threadEndLogged = true;
                    endTaskSource.SetResult(0);
                }
            }
        }
    }
}
