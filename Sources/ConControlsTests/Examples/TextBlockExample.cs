﻿/*
 * (C) René Vogt
 *
 * Published under MIT license as described in the LICENSE.md file.
 *
 */

#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading.Tasks;
using ConControls.Controls;
using ConControls.Logging;

// ReSharper disable AssignmentIsFullyDiscarded

namespace ConControlsTests.Examples
{
    [ExcludeFromCodeCoverage]
    class TextBlockExample : Example
    {
        public override DebugContext DebugContext => DebugContext.Text;
        public override async Task RunAsync()
        {
            const string text = @"sample text with some longer
and
some shorter
lines. empty line:

a
b
c

end.";
            using var window = new ConsoleWindow
            {
                Title = "ConControls: TextBlock example"
            };
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            using(window.DeferDrawing())
            {
                var panel = new Panel(window)
                {
                    BackgroundColor = ConsoleColor.DarkBlue,
                    Area = new Rectangle(1, 1, 50, 20),
                    BorderStyle = BorderStyle.Bold
                };
                window.Controls.Add(panel);

                var button = new Button(window)
                    {
                        Area = new Rectangle(8, 16, 9, 3),
                        Text = "Close"
                    };
                button.Click += (sender, e) => tcs.SetResult(0);
                panel.Controls.AddRange(
                    new TextBlock(window)
                    {
                        Area = new Rectangle(2, 2, 10, 6),
                        BorderStyle = BorderStyle.None,
                        BackgroundColor = ConsoleColor.Blue,
                        ForegroundColor = ConsoleColor.White,
                        Text = text
                    },
                    new TextBlock(window)
                    {
                        Area = new Rectangle(16, 2, 10, 6),
                        BorderStyle = BorderStyle.None,
                        BackgroundColor = ConsoleColor.Blue,
                        ForegroundColor = ConsoleColor.White,
                        Wrap = true,
                        Text = text
                    },
                    new TextBlock(window)
                    {
                        Area = new Rectangle(2, 10, 10, 6),
                        BorderStyle = BorderStyle.SingleLined,
                        BackgroundColor = ConsoleColor.Blue,
                        ForegroundColor = ConsoleColor.White,
                        Text = text
                    },
                    new TextBlock(window)
                    {
                        Area = new Rectangle(14, 10, 10, 6),
                        BorderStyle = BorderStyle.SingleLined,
                        Wrap = true,
                        BackgroundColor = ConsoleColor.Blue,
                        ForegroundColor = ConsoleColor.White,
                        Text = text,
                        CursorVisible = false
                    },
                    button
                );
                window.FocusedControl = panel.Controls[0];
            }

            await tcs.Task;
        }
    }
}
