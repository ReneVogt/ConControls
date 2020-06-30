﻿using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Verify = Microsoft.CodeAnalysis.CSharp.CodeFix.Testing.MSTest.CodeFixVerifier<
    ConControls.CodeAnalysis.PublicEntryPoint.PublicEntryPointAnalyzer,
    ConControls.CodeAnalysis.PublicEntryPoint.PublicEntryPointFixProvider>;

namespace ConControlsAnalyzerTests.PublicEntryPoint
{
    [TestClass]
    public class UnitTest
    {
        //No diagnostics expected to show up
        [TestMethod]
        public async Task TestMethod1()
        {
            var test = @"";

            await Verify.VerifyCSharpDiagnosticAsync(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public async Task TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
        }
    }";

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TYPENAME
        {   
        }
    }";

            var expected = Verify.Diagnostic("ConControlsAnalyzer").WithLocation(11, 15).WithArguments("TypeName");
            await Verify.VerifyCSharpFixAsync(test, expected, fixtest);
        }
    }
}
