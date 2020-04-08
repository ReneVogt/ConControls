﻿namespace ConControls.Controls
{
    /// <summary>
    /// The base class for all custom console controls.
    /// </summary>
    public class ConsolePanel : ConsoleControl
    {
        /// <inheritdoc />
        public ConsolePanel(IConsoleWindow window)
            : base(window) { }

    }
}
