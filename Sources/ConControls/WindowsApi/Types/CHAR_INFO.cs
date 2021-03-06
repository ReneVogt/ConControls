﻿/*
 * (C) René Vogt
 *
 * Published under MIT license as described in the LICENSE.md file.
 *
 */

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace ConControls.WindowsApi.Types
{
    [ExcludeFromCodeCoverage]
    [StructLayout(LayoutKind.Sequential)]
    struct CHAR_INFO
    {
        [MarshalAs(UnmanagedType.U2)]
        public char Char;
        public ConCharAttributes Attributes;

        public CHAR_INFO(char c, ConCharAttributes attributes)
        {
            Char = c;
            Attributes = attributes;
        }
    }
}
