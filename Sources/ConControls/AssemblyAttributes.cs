﻿/*
 * (C) René Vogt
 *
 * Published under MIT license as described in the LICENSE.md file.
 *
 */

using System;
using System.Runtime.CompilerServices;

[assembly: CLSCompliant(true)]

#if RELEASE
[assembly: InternalsVisibleTo("ConControlsTests, PublicKey=002400000480000094000000060200000024000052534131000400000100010015df19e42fdada070de3aa62f547a9b16a8d13216d5fd6adc27abf90f66c3bc3de6f4240759b347cf09486df31f019ab477d66e5241e18ab8c8a19ec7bbc01d04ab658aa0675b7d1777d2c3ab81fe6587c9a58d85021fe087f2d2b78434219a6e5d2870abc667bb74053f35f1fc80966d3bbdaf2fee5312d20483b2c34c5fcbf")]
[assembly: InternalsVisibleTo("ConControls.Fakes, PublicKey=0024000004800000940000000602000000240000525341310004000001000100e92decb949446f688ab9f6973436c535bf50acd1fd580495aae3f875aa4e4f663ca77908c63b7f0996977cb98fcfdb35e05aa2c842002703cad835473caac5ef14107e3a7fae01120a96558785f48319f66daabc862872b2c53f5ac11fa335c0165e202b4c011334c7bc8f4c4e570cf255190f4e3e2cbc9137ca57cb687947bc")]
#else
[assembly: InternalsVisibleTo("ConControlsTests")]
[assembly: InternalsVisibleTo("ConControls.Fakes")]
#endif
