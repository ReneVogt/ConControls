﻿/*
 * (C) René Vogt
 *
 * Published under MIT license as described in the LICENSE.md file.
 *
 */

using System;
using System.Drawing;

namespace ConControls.Controls.Drawing
{
    /// <summary>
    /// Provides methods to draw on the console.
    /// </summary>
    /// <remarks>
    /// This interface represents an abstraction layer for <see cref="ConsoleControl"/> instances
    /// to draw on the console screen buffer. It is implemented by the internal class <c>ConsoleGraphics</c>.<br/>
    /// An instance of this interface must be provided by <see cref="IConsoleWindow.GetGraphics">IConsoleWindow.GetGraphics</see>
    /// to enable controls to draw themselves.
    /// </remarks>
    public interface IConsoleGraphics
    {
        /// <summary>
        /// Sets the background <paramref name="color"/> in the specified <paramref name="area"/>.
        /// The changes are only written to the screen buffer when <see cref="Flush"/> is called.
        /// </summary>
        /// <param name="color">The <see cref="ConsoleColor"/> to use for the background.</param>
        /// <param name="area">The area (in screen buffer coordinates) to fill.</param>
        void DrawBackground(ConsoleColor color, Rectangle area);
        /// <summary>
        /// Draws a border around (onto the edge of) the specified <paramref name="area"/>.
        /// </summary>
        /// <param name="background">The <see cref="ConsoleColor"/> to use for the background.</param>
        /// <param name="foreground">The <see cref="ConsoleColor"/> to use for the border foreground.</param>
        /// <param name="style">The <see cref="BorderStyle"/> to use for the border.</param>
        /// <param name="area">The area (in screen buffer coordinates) to fill.</param>
        void DrawBorder(ConsoleColor background, ConsoleColor foreground, BorderStyle style, Rectangle area);
        /// <summary>
        /// Fills the specified <paramref name="area"/> with the given colors and character.
        /// </summary>
        /// <param name="background">The background color to use.</param>
        /// <param name="foreColor">The foreground color to use.</param>
        /// <param name="c">The character to use.</param>
        /// <param name="area">The area to fill.</param>
        /// <remarks>
        /// <para>
        /// Fills the specified rectangle of the cached buffer
        /// with the given characters and colors. Character cells outside
        /// of the boundaries of the cached buffer will be ignored.
        /// </para>
        /// <para>
        /// The changed area will be written to the console screen buffer only when
        /// <see cref="Flush"/> is called.
        /// </para></remarks>
        void FillArea(ConsoleColor background, ConsoleColor foreColor, char c, Rectangle area);
        /// <summary>
        /// Copies an array of characters into the buffer and applies the given background and
        /// foreground colors.
        /// </summary>
        /// <remarks>
        /// The method tries to copy the characters at the position given by <paramref name="topLeft"/>.
        /// It only copies characters that fit into the boundaries of the target buffer.
        /// </remarks>
        /// <param name="background">The <see cref="ConsoleColor"/> to use for the background.</param>
        /// <param name="foreColor">The <see cref="ConsoleColor"/> to use for the foreground.</param>
        /// <param name="topLeft">The top left <see cref="Point"/> of the target area.</param>
        /// <param name="characters">An array of characters to copy to the buffer.</param>
        /// <param name="size">The size of the <paramref name="characters"/> array.</param>
        void CopyCharacters(ConsoleColor background, ConsoleColor foreColor, Point topLeft, char[] characters, Size size);
        /// <summary>
        /// Flushes the internal buffer to the console screen buffer.
        /// </summary>
        void Flush();
    }
}
