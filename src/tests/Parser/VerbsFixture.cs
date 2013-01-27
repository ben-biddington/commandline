﻿#region License
//
// Command Line Library: VerbsFixture.cs
//
// Author:
//   Giacomo Stelluti Scala (gsscoder@gmail.com)
//
// Copyright (C) 2005 - 2013 Giacomo Stelluti Scala
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
#endregion
#region Using Directives
using System;
using System.IO;
using Xunit;
using FluentAssertions;
using CommandLine.Tests.Mocks;
#endregion

namespace CommandLine.Tests
{
    
    public class VerbsFixture : CommandLineParserBaseFixture
    {
        [Fact]
        public void ParseVerbsCreateInstance()
        {
            var options = new OptionsWithVerbs();
            options.AddVerb.Should().BeNull();

            var parser = new CommandLineParser();
            var result = parser.ParseArguments(new string[] {"add", "-p", "untracked.bin"} , options);

            result.Should().BeTrue();

            parser.WasVerbOptionInvoked("add").Should().BeTrue();
            parser.WasVerbOptionInvoked("commit").Should().BeFalse();
            parser.WasVerbOptionInvoked("clone").Should().BeFalse();

            // Parser has built instance for us
            options.AddVerb.Should().NotBeNull();
            options.AddVerb.CreationProof.Should().NotHaveValue();
            options.AddVerb.Patch.Should().BeTrue();
            options.AddVerb.FileName[0].Should().Be("untracked.bin");
        }

        [Fact]
        public void ParseVerbsUsingInstance()
        {
            var proof = new Random().Next(int.MaxValue);
            var options = new OptionsWithVerbs();
            options.CommitVerb.Should().NotBeNull();
            options.CommitVerb.CreationProof = proof;

            var parser = new CommandLineParser();
            var result = parser.ParseArguments(new string[] { "commit", "--amend" }, options);

            result.Should().BeTrue();

            parser.WasVerbOptionInvoked("add").Should().BeFalse();
            parser.WasVerbOptionInvoked("commit").Should().BeTrue();
            parser.WasVerbOptionInvoked("clone").Should().BeFalse();

            // Check if the instance is the one provider by us (not by the parser)
            options.CommitVerb.CreationProof.Should().Be(proof);
            options.CommitVerb.Amend.Should().BeTrue();
        }

        [Fact]
        public void FailedParsingPrintsHelpIndex()
        {
            var options = new OptionsWithVerbs();
            var testWriter = new StringWriter();

            var parser = new CommandLineParser();
            var result = parser.ParseArguments(new string[] {}, options, testWriter);

            result.Should().BeFalse();

            parser.WasVerbOptionInvoked("add").Should().BeFalse();
            parser.WasVerbOptionInvoked("commit").Should().BeFalse();
            parser.WasVerbOptionInvoked("clone").Should().BeFalse();

            var helpText = testWriter.ToString();
            helpText.Should().Be("verbs help index");
        }

        [Fact]
        public void FailedVerbParsingPrintsParticularHelpScreen()
        {
            var options = new OptionsWithVerbs();
            var testWriter = new StringWriter();

            var parser = new CommandLineParser();
            var result = parser.ParseArguments(new string[] {"clone", "--no_hardlinks"}, options, testWriter);

            result.Should().BeFalse();

            parser.WasVerbOptionInvoked("add").Should().BeFalse();
            parser.WasVerbOptionInvoked("commit").Should().BeFalse();
            // The following returns true because also if the parser fail 'clone' was invoked.
            parser.WasVerbOptionInvoked("clone").Should().BeTrue();

            var helpText = testWriter.ToString();
            helpText.Should().Be("help for: clone");
        }

        [Fact]
        public void WasVerbOptionInvokedReturnsFalseWithEmptyArguments()
        {
            var options = new OptionsWithVerbs();

            var parser = new CommandLineParser();
            var result = parser.ParseArguments(new string[] {}, options);

            result.Should().BeFalse();

            parser.WasVerbOptionInvoked("add").Should().BeFalse();
            parser.WasVerbOptionInvoked("commit").Should().BeFalse();
            parser.WasVerbOptionInvoked("clone").Should().BeFalse();
        }

        [Fact]
        public void WasVerbOptionInvokedReturnsFalseWithNullOrEmptyVerb()
        {
            var options = new OptionsWithVerbs();

            var parser = new CommandLineParser();
            var result = parser.ParseArguments(new string[] {"commit", "--amend"}, options);

            result.Should().BeTrue();

            parser.WasVerbOptionInvoked(null).Should().BeFalse();
            parser.WasVerbOptionInvoked("").Should().BeFalse();
        }

        [Fact]
        public void WasVerbOptionInvokedReturnsFalseWithOrdinaryOptions()
        {
            var options = new OptionsWithVerbs();

            var parser = new CommandLineParser();
            var result = parser.ParseArguments(new string[] {"commit", "--amend"}, options);

            result.Should().BeTrue();

            parser.WasVerbOptionInvoked("--commit").Should().BeFalse();
            parser.WasVerbOptionInvoked("-commit").Should().BeFalse(); // <- pure fantasy
            parser.WasVerbOptionInvoked("-c").Should().BeFalse();
            parser.WasVerbOptionInvoked("---commit").Should().BeFalse(); // <- pure fantasy
            parser.WasVerbOptionInvoked("--amend").Should().BeFalse();
            parser.WasVerbOptionInvoked("-a").Should().BeFalse();
        }
    }
}