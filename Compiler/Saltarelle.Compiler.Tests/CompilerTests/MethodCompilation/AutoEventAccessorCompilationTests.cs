﻿using System;
using NUnit.Framework;
using Saltarelle.Compiler.JSModel;
using Saltarelle.Compiler.ScriptSemantics;

namespace Saltarelle.Compiler.Tests.CompilerTests.MethodCompilation {
	[TestFixture]
	public class AutoEventAccessorCompilationTests : CompilerTestBase {
		[Test]
		public void InstanceAutoEventAccessorsImplementedAsInstanceMethodsAreCorrectlyCompiled() {
			Compile(new[] { "using System; class C { public event System.EventHandler MyEvent; }" });

			var adder   = FindInstanceMethod("C.add_MyEvent");
			var remover = FindInstanceMethod("C.remove_MyEvent");

			AssertCorrect(SourceLocationsInserter.Process(adder.Definition),
@"function($value) {
	// @(1, 58) - (1, 65)
	this.$MyEvent = {sm_Delegate}.Combine(this.$MyEvent, $value);
}");

			AssertCorrect(SourceLocationsInserter.Process(remover.Definition),
@"function($value) {
	// @(1, 58) - (1, 65)
	this.$MyEvent = {sm_Delegate}.Remove(this.$MyEvent, $value);
}");

			AssertCorrect(FindClass("C").UnnamedConstructor,
@"function() {
	$Init(this, '$MyEvent', null);
	{sm_Object}.call(this);
}");
		}

		[Test]
		public void InstanceAutoEventAccessorsImplementedAsStaticMethodsAreCorrectlyCompiled() {
			Compile(new[] { "using System; class C { public event System.EventHandler MyEvent; }" }, metadataImporter: new MockMetadataImporter { GetEventSemantics = e => EventScriptSemantics.AddAndRemoveMethods(MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("add_" + e.Name), MethodScriptSemantics.StaticMethodWithThisAsFirstArgument("remove_" + e.Name)) });

			var adder   = FindStaticMethod("C.add_MyEvent");
			var remover = FindStaticMethod("C.remove_MyEvent");

			AssertCorrect(SourceLocationsInserter.Process(adder.Definition),
@"function($this, $value) {
	// @(1, 58) - (1, 65)
	$this.$MyEvent = {sm_Delegate}.Combine($this.$MyEvent, $value);
}");

			AssertCorrect(SourceLocationsInserter.Process(remover.Definition),
@"function($this, $value) {
	// @(1, 58) - (1, 65)
	$this.$MyEvent = {sm_Delegate}.Remove($this.$MyEvent, $value);
}");

			AssertCorrect(FindClass("C").UnnamedConstructor,
@"function() {
	$Init(this, '$MyEvent', null);
	{sm_Object}.call(this);
}");
		}

		[Test]
		public void StaticAutoEventAccessorsAreCorrectlyCompiled() {
			Compile(new[] { "using System; class C { public static event System.EventHandler MyEvent; }" });

			var adder   = FindStaticMethod("C.add_MyEvent");
			var remover = FindStaticMethod("C.remove_MyEvent");

			AssertCorrect(SourceLocationsInserter.Process(adder.Definition),
@"function($value) {
	// @(1, 65) - (1, 72)
	{sm_C}.$MyEvent = {sm_Delegate}.Combine({sm_C}.$MyEvent, $value);
}");

			AssertCorrect(SourceLocationsInserter.Process(remover.Definition),
@"function($value) {
	// @(1, 65) - (1, 72)
	{sm_C}.$MyEvent = {sm_Delegate}.Remove({sm_C}.$MyEvent, $value);
}");

			var c = FindClass("C");
			Assert.That(c.StaticInitStatements, Has.Count.EqualTo(1));
			Assert.That(OutputFormatter.Format(c.StaticInitStatements[0], allowIntermediates: true), Is.EqualTo("$Init({sm_C}, '$MyEvent', null);" + Environment.NewLine));
		}

		[Test]
		public void StaticAutoEventAccessorsAreCorrectlyCompiledForGenericClasses() {
			Compile(new[] { "using System; class C<T> { public static event System.EventHandler MyEvent; }" });

			var adder   = FindStaticMethod("C.add_MyEvent");
			var remover = FindStaticMethod("C.remove_MyEvent");

			AssertCorrect(SourceLocationsInserter.Process(adder.Definition),
@"function($value) {
	// @(1, 68) - (1, 75)
	sm_$InstantiateGenericType({C}, $T).$MyEvent = {sm_Delegate}.Combine(sm_$InstantiateGenericType({C}, $T).$MyEvent, $value);
}");

			AssertCorrect(SourceLocationsInserter.Process(remover.Definition),
@"function($value) {
	// @(1, 68) - (1, 75)
	sm_$InstantiateGenericType({C}, $T).$MyEvent = {sm_Delegate}.Remove(sm_$InstantiateGenericType({C}, $T).$MyEvent, $value);
}");

			var c = FindClass("C");
			Assert.That(c.StaticInitStatements, Has.Count.EqualTo(1));
			Assert.That(OutputFormatter.Format(c.StaticInitStatements[0], allowIntermediates: true), Is.EqualTo("$Init(sm_$InstantiateGenericType({C}, $T), '$MyEvent', null);" + Environment.NewLine));
		}
	}
}
