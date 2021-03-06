﻿/*
 * (C) René Vogt
 *
 * Published under MIT license as described in the LICENSE.md file.
 *
 */

#nullable enable

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConControlsTests.UnitTests.Controls.Text.ConsoleTextController
{
    public partial class ConsoleTextControllerTests
    {
        [TestMethod]
        public void Text_Changed()
        {
            const string text = "hello world!\ngood bye!";
            var sut = new ConControls.Controls.Text.ConsoleTextController();
            sut.Text.Should().BeEmpty();
            sut.Text = text;
            sut.Text.Should().Be(text);
            sut.BufferLineCount.Should().Be(2);
            sut.Text = text;
            sut.Text.Should().Be(text);
            sut.BufferLineCount.Should().Be(2);
        }
    }
}
