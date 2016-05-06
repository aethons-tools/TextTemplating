using AethonsTools;
using AethonsTools.TextTemplating;
using FluentAssertions;
using FluentAssertions.Collections;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace Tests
{

    public interface IOne { }
    public interface ITwo { }

    public class OneModel : IOne { }
    public class TwoModel : ITwo { }
    public class OneAndTwoModel : IOne, ITwo { }

    public class BaseModel
    {
        public string Name { get; }
    }

    public sealed class DerivedModel : BaseModel
    {
    }

    public sealed class OtherDerivedModel : BaseModel
    {
        public BaseModel Sibling { get; }
    }

    public sealed class UnknownModel
    { }

    public sealed class SelectionTestTemplate : TextTemplate
    {
        public TemplateResult For(BaseModel model) => Template($"Base");
        public TemplateResult For(DerivedModel model) => Template($"Derived");
        public TemplateResult For(IOne x) => Template($"IOne");
        public TemplateResult For(ITwo x) => Template($"ITwo");
        public TemplateResult For(TwoModel x) => Template($"TwoModel");
    }

    public sealed class BadTemplate : TextTemplate
    {
        public TemplateResult For(object model) { throw new Exception("Because I hate you, Alex!"); }

        public string For(OtherDerivedModel model) => "OtherDerived";

        //       public TemplateResult For(OtherDerivedModel model) => Template($"Derived; Sibling={model.Sibling}");

        //       public TemplateResult For(BaseModel model) => Template($"{model.Name}");
    }

    [TestFixture]
    public class TextTemplateTests
    {
        private static readonly BaseModel BaseModel = new BaseModel();
        private static readonly DerivedModel DerivedModel = new DerivedModel();
        private static readonly OtherDerivedModel OtherDerivedModel = new OtherDerivedModel();
        private static readonly OneModel OneModel = new OneModel();
        private static readonly TwoModel TwoModel = new TwoModel();
        private static readonly OneAndTwoModel OneAndTwoModel = new OneAndTwoModel();
        private static readonly UnknownModel UnknownModel = new UnknownModel();

        private static readonly SelectionTestTemplate SelectionTestTemplate = new SelectionTestTemplate();
        private static readonly BadTemplate BadTemplate = new BadTemplate();

        [Test]
        public void Execute_WithExactModelType_ExecutesProperDefault()
        {
            SelectionTestTemplate.Apply(BaseModel)
                .Dump()
                .ShouldBeEquivalentTo(new TemplateResult("Base"));
        }

        [Test]
        public void Execute_WithUnknownDerivedModelType_ExecutesProperDefault()
        {
            SelectionTestTemplate.Apply(OtherDerivedModel)
                .Dump()
                .ShouldBeEquivalentTo(new TemplateResult("Base"));
        }

        [Test]
        public void Execute_WithKnownDerivedModelType_ExecutesProperDefault()
        {
            SelectionTestTemplate.Apply(DerivedModel)
                .Dump()
                .ShouldBeEquivalentTo(new TemplateResult("Derived"));
        }

        [Test]
        public void Execute_WithImplementedInterface_ExecutesProperDefault()
        {
            SelectionTestTemplate.Apply(OneModel)
                .Dump()
                .ShouldBeEquivalentTo(new TemplateResult("IOne"));
        }

        [Test]
        public void Execute_WithImplementedInterfaceAndExactModel_ExecutesModelDefault()
        {
            SelectionTestTemplate.Apply(TwoModel)
                .Dump()
                .ShouldBeEquivalentTo(new TemplateResult("TwoModel"));
        }

        [Test]
        public void Execute_WithUnkownModelType_ReturnsCorrectError()
        {
            SelectionTestTemplate.Apply(UnknownModel)
                .Dump()
                .Diagnostics
                    .Should().Contain(Descriptors.CannotFindDefaultTemplateMethod);
        }

        [Test]
        public void Execute_WithAmbiguousModelType_ReturnsErrors()
        {
            SelectionTestTemplate.Apply(OneAndTwoModel)
                .Dump()
                .Diagnostics
                    .Should().Contain(Descriptors.AmbiguousDefaultTemplateMethod);
        }

        [Test]
        public void Execute_WithAThrownException_ReturnsCorrectError()
        {
            BadTemplate.Apply(new object())
                .Dump()
                .Diagnostics
                    .Should().Contain(Descriptors.TemplatingException);
        }

        [Test]
        public void Execute_WhenTheSelectedTemplateMethodReturnsNonTemplateResult_ReturnsCorrectError()
        {
            BadTemplate.Apply(OtherDerivedModel)
                .Dump()
                .Diagnostics
                    .Should().Contain(Descriptors.TemplateMethodMustReturnTemplateResult);
        }
    }

    public sealed class Person
    {
        public Person(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public string FirstName { get; }
        public string LastName { get; }
        public IEnumerable<Role> Roles { get; } = new Role[0];

        public void AddRole(Role role)
        {
            
        }
    }

    public abstract class Role
    {
        public abstract string Name { get; }
    }

    public sealed class Author : Role
    {
        public override string Name => "Author";

        public Person Person { get; }
        public IEnumerable<Publication> Publications { get; }
    }

    public abstract class Publication
    {
        public abstract string Name { get; }
        public abstract string Type { get; }
    }

    public sealed class Book : Publication
    {
        public override string Type => "Book";
        public override string Name { get; }
        public Author Author { get; }
    }


    public sealed class Model
    {
        public Model(object value)
        {
            Value = value;
        }

        public object Value { get; }
    }

    [TestFixture]
    public class RecursionTemplate : TextTemplate
    {
        public TemplateResult For(Model model) => Template($"M({model.Value})");

        [Test]
        public void Test()
        {
            new RecursionTemplate().Apply(new Model(10))
                .Dump()
                .Filter(Severity.Error)
                    .ShouldBeEquivalentTo(new TemplateResult("M(10)"));
        }
        [Test]
        public void Test2()
        {
            new RecursionTemplate().Apply(new Model(new Model(10)))
                .Dump()
                .Filter(Severity.Error)
                    .ShouldBeEquivalentTo(new TemplateResult("M(M(10))"));
        }
        [Test]
        public void Test3()
        {
            new RecursionTemplate().Apply(Enumerable.Range(1,9))
                .Dump()
                .Filter(Severity.Error)
                    .ShouldBeEquivalentTo(new TemplateResult("123456789"));
        }
    }

    [TestFixture]
    public class Template : TextTemplate
    {
        public TemplateResult For(int model) => Template($"int: {(float)model}");

        public TemplateResult For(float model) => GeneralDiagnostics.Error("Don't like it");

        [Test]
        public void DeepErrorIsPropagated()
        {
            Apply(42)
                .Dump();
        }
    }

    public static class TemplateResultTestingExtensions
    {
        public static TemplateResult Filter(this TemplateResult @this, Severity level)
        {
            return new TemplateResult(@this.Output, @this.Diagnostics.Where(d => d.Descriptor.Severity <= level));
        }

        public static TemplateResult Dump(this TemplateResult @this)
        {
            if (!string.IsNullOrEmpty(@this.Output))
            {
                Console.WriteLine("===OUTPUT========");
                Console.WriteLine(@this.Output);
                Console.WriteLine("=================");
                Console.WriteLine();
            }

            var diagnostics = @this.Diagnostics.ToArray();
            if (diagnostics.Length > 0)
            {
                Console.WriteLine("===DIAGNOSTICS===");
                for (var i = 0; i < diagnostics.Length; i++)
                    Console.WriteLine($"{i + 1}. {diagnostics[i]}");
                Console.WriteLine("=================");
                Console.WriteLine();
            }
            return @this;
        }
    }

    public static class AssertionExtensions
    {
        public static object Contain(this GenericCollectionAssertions<BasicDiagnostic> @this, BasicDiagnosticDescriptor descriptor)
        {
            if (!@this.Subject.Any(d => d.Descriptor.Equals(descriptor)))
                throw new AssertionException($"Diagnostics did not contain '{descriptor}'");
            return new AndConstraint<GenericCollectionAssertions<BasicDiagnostic>>(@this);
        }
    }
}
