﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Saltarelle.Compiler.Tests.MethodCompilationTests.ExpressionTests {
	[TestFixture]
	public class ThisTests : MethodCompilerTestBase {
		[Test]
		public void ThisWorksInsideNormalMethod() {
			AssertCorrect(
@"int x;
public void M() {
	// BEGIN
	int i = x;
	// END
}",
@"	var $i = this.$x;
");
		}

		[Test]
		public void ThisWorksInsideAnonymousMethodInsideNormalMethod() {
			// It works like this because the anonymous method conversion will perform the correct bind.
			AssertCorrect(
@"int x;
public void M() {
	Action a = () => {
		// BEGIN
		int i = x;
		// END
	};
}",
@"		var $i = this.$x;
");
		}

		[Test]
		public void ThisWorksInsideAnonymousMethodThatUsesByRefVariablesInsideNormalMethod() {
			// It works like this because the anonymous method conversion will perform the correct bind.
			AssertCorrect(
@"void F(ref int a) {}
int x;
public void M() {
	int y = 0;
	F(ref y);
	Action a = () => {
		// BEGIN
		int i = x;
		// END
		int j = y;
	};
}",
@"		var $i = this.$this.$x;
");
		}

		[Test]
		public void ThisWorksInsideStaticMethodWithThisAsFirstArgument() {
			AssertCorrect(
@"int x;
public void M() {
	// BEGIN
	int i = x;
	// END
}",
@"	var $i = $this.$x;
", namingConvention: new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.StaticMethodWithThisAsFirstArgument("$" + m.Name) });
		}

		[Test]
		public void ThisWorksInsideAnonymousMethodInsideStaticMethodWithThisAsFirstArgument() {
			// It works like this because the anonymous method conversion will perform the correct bind.
			AssertCorrect(
@"int x;
public void M() {
	Action a = () => {
		// BEGIN
		int i = x;
		// END
	};
}",
@"		var $i = $this.$x;
", namingConvention: new MockNamingConventionResolver { GetMethodImplementation = m => MethodImplOptions.StaticMethodWithThisAsFirstArgument("$" + m.Name) });
		}
	}
}
