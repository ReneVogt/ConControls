﻿/*
 * (C) René Vogt
 *
 * Published under MIT license as described in the LICENSE.md file.
 *
 */

namespace ConControls.Controls 
{
    /// <summary>
    /// A console panel control. A plain container
    /// for other controls.
    /// </summary>
    public sealed class ConsolePanel : ConsoleControl
    {
        /// <inheritdoc />
        public ConsolePanel(IConsoleWindow window) : base(window) { }
        /// <inheritdoc />
        public ConsolePanel(IConsoleWindow window, ConsoleControl parent) : base(window, parent) { }
    }
}
