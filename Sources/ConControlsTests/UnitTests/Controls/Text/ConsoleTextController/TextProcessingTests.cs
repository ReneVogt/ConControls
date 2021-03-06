﻿/*
 * (C) René Vogt
 *
 * Published under MIT license as described in the LICENSE.md file.
 *
 */

#nullable enable

using System.Drawing;
using ConControls.Controls.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConControlsTests.UnitTests.Controls.Text.ConsoleTextController
{
    public partial class ConsoleTextControllerTests
    {
        [TestMethod]
        public void TextProcessing_TestCase_001()
        {
            const string text = "0123456789\n0123456789";
            var sut = new ConControls.Controls.Text.ConsoleTextController
            {
                Width = 5,
                WrapMode = WrapMode.SimpleWrap,
                Text = text
            };

            sut.BufferLineCount.Should().Be(6);
            sut.MaxLineLength.Should().Be(5);
            sut.Text.Should().Be(text);
            sut.Width.Should().Be(5);
            sut.WrapMode.Should().Be(WrapMode.SimpleWrap);
            sut.GetLineLength(-1).Should().Be(0);
            sut.GetLineLength(0).Should().Be(5);
            sut.GetLineLength(1).Should().Be(5);
            sut.GetLineLength(2).Should().Be(0);
            sut.GetLineLength(3).Should().Be(5);
            sut.GetLineLength(4).Should().Be(5);
            sut.GetLineLength(5).Should().Be(0);

            sut.GetCharacters(new Rectangle(Point.Empty, new Size(5, 6)))
               .Should()
               .Equal(
                   '0', '1', '2', '3', '4',
                   '5', '6', '7', '8', '9',
                   '\0', '\0', '\0', '\0', '\0',
                   '0', '1', '2', '3', '4',
                   '5', '6', '7', '8', '9',
                   '\0', '\0', '\0', '\0', '\0');
            sut.GetCharacters(new Rectangle(2, 2, 7, 2))
               .Should()
               .Equal(
                   '\0', '\0', '\0', '\0', '\0', '\0', '\0',
                   '2', '3', '4', '\0', '\0', '\0', '\0');

            sut.WrapMode = WrapMode.NoWrap;
            sut.Width = 4;

            sut.BufferLineCount.Should().Be(2);
            sut.MaxLineLength.Should().Be(10);
            sut.Text.Should().Be(text);
            sut.Width.Should().Be(4);
            sut.WrapMode.Should().Be(WrapMode.NoWrap);
            sut.GetLineLength(-1).Should().Be(0);
            sut.GetLineLength(0).Should().Be(10);
            sut.GetLineLength(1).Should().Be(10);
            sut.GetLineLength(2).Should().Be(0);

            sut.GetCharacters(new Rectangle(3, 1, 10, 2))
               .Should()
               .Equal(
                   '3', '4', '5', '6', '7', '8', '9', '\0', '\0', '\0',
                   '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0');
        }
        [TestMethod]
        public void TextProcessing_TestCase_002()
        {
            const string text = "01234\r\n01234";
            var sut = new ConControls.Controls.Text.ConsoleTextController
            {
                Width = 5,
                WrapMode = WrapMode.SimpleWrap,
                Text = text
            };
            sut.GetCharacters(new Rectangle(Point.Empty, new Size(6, 4)))
               .Should()
               .Equal(
                   '0', '1', '2', '3', '4', '\0',
                   '\0', '\0', '\0', '\0', '\0', '\0',
                   '0', '1', '2', '3', '4', '\0',
                   '\0', '\0', '\0', '\0', '\0', '\0');
        }
        [TestMethod]
        public void TextProcessing_RequestingAreaWithNegativeSize_Empty()
        {
            const string text = "01234\r\n01234";
            var sut = new ConControls.Controls.Text.ConsoleTextController
            {
                Width = 5,
                WrapMode = WrapMode.SimpleWrap,
                Text = text
            };
            sut.GetCharacters(new Rectangle(Point.Empty, new Size(-1, 1)))
               .Should()
               .BeEmpty();
            sut.GetCharacters(new Rectangle(Point.Empty, new Size(1, -1)))
               .Should()
               .BeEmpty();
            sut.GetCharacters(new Rectangle(Point.Empty, new Size(-1, -1)))
               .Should()
               .BeEmpty();
        }
    }
}
