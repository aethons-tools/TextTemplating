using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AethonsTools.TextTemplating
{
    public abstract class TextTemplate
    {
        public IFormatProvider FormatProvider { get; }
        public string Newline { get; } = "\n";

        public delegate object TemplateResolver(object model, Action<BasicDiagnostic> reportDiagnostic);
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, TemplateResolver>> GeneratorTypeTables =
            new ConcurrentDictionary<Type, ConcurrentDictionary<Type, TemplateResolver>>();

        protected TextTemplate()
        { }

        protected TextTemplate(IFormatProvider formatProvider)
        {
            FormatProvider = formatProvider;
        }

        public TemplateResult Apply<T>(T model) => Template($"{model}");

        protected virtual TemplateResult Template(FormattableString template)
        {
            try
            {
                var diagnostics = new List<BasicDiagnostic>();
                var resolvedArgs = template.GetArguments()
                    .Select(arg => Resolve(arg, diagnostics.Add))
                    .ToArray();

                var output = string.Format(FormatProvider, template.Format, resolvedArgs);

                return new TemplateResult(output, diagnostics);
            }
            catch (Exception x)
            {
                return new TemplateResult(Diagnostics.TemplatingException(x));
            }
        }


        protected virtual object Resolve<T>(T model, Action<BasicDiagnostic> reportDiagnostic)
        {
            var modelType = model == null
                ? typeof(T)
                : model.GetType();

            object resolved = model;

            if (!(resolved is TemplateResult))
            {
                var resolver = GetResolver(modelType, reportDiagnostic);
                resolved = resolver == null
                    ? resolved
                    : resolver(model, reportDiagnostic);
            }

            var templateResult = resolved as TemplateResult;
            if (templateResult != null)
            {
                foreach (var d in templateResult.Diagnostics)
                    reportDiagnostic(d);
                resolved = templateResult.Output;
            }
            return resolved;
        }

        protected virtual TemplateResolver GetResolver(Type modelType, Action<BasicDiagnostic> reportDiagnostic)
        {
            if (modelType == null)
                return null;
            var generatorType = GetType();
            var table = GeneratorTypeTables.GetOrAdd(generatorType, _ => new ConcurrentDictionary<Type, TemplateResolver>());

            return table.GetOrAdd(modelType, t =>
            {
                try
                {
                    var methodInfo = generatorType.GetTypeInfo().GetMethod("For", new[] { modelType });
                    if (methodInfo == null)
                    {
                        reportDiagnostic(Diagnostics.CannotFindDefaultTemplateMethod(modelType, this)); // TODO: this should be information
                        return null;
                    }
                    if (methodInfo.ReturnType != typeof(TemplateResult))
                    {
                        var error = Diagnostics.TemplateMethodMustReturnTemplateResult(methodInfo);
                        reportDiagnostic(error);
                        var result = TextForError(modelType, error);
                        return (model, reporter) => result;
                    }
                    return (model, reporter) => methodInfo.Invoke(this, new[] { model }); // TODO: compile via linq expression
                }
                catch (AmbiguousMatchException)
                {
                    var error = Diagnostics.AmbiguousDefaultTemplateMethod(modelType, this);
                    reportDiagnostic(error);
                    var result = TextForError(modelType, error);
                    return (model, reporter) => result;
                }
            });
        }

        protected virtual string TextForError(Type modelType, BasicDiagnostic error) =>
            $"##Cannot template type {modelType?.Name}: {error?.Message}##";

        protected static string genif(bool condition, string trueValue, string falseValue) =>
            (condition ? trueValue : falseValue) ?? string.Empty;
        protected static string genif(bool condition, Func<string> trueValue, Func<string> falseValue) =>
            (condition ? trueValue() : falseValue?.Invoke()) ?? string.Empty;
        protected static string genif(bool condition, string trueValue, Func<string> falseValue) =>
            (condition ? trueValue : falseValue?.Invoke()) ?? string.Empty;
        protected static string genif(bool condition, Func<string> trueValue, string falseValue) =>
            (condition ? trueValue() : falseValue) ?? string.Empty;

        protected static string genif(bool condition, string trueValue) =>
            genif(condition, trueValue, string.Empty);
        protected static string genif(bool condition, Func<string> trueValue) =>
            genif(condition, trueValue, string.Empty);

        public virtual TemplateResult For(IEnumerable members)
        {
            var diagnostics = new List<BasicDiagnostic>();
            var resolved = members.Cast<object>().Select(member => Resolve(member, diagnostics.Add)?.ToString());
            var output = string.Join(string.Empty, resolved);
            return new TemplateResult(output, diagnostics);
        }

        //  protected static string _<T>(IEnumerable<T> members, Func<T, string> selector) =>
        //     _(members.Select(selector));

        protected static string list(IEnumerable<string> arguments, string separator) =>
            string.Join(separator, arguments.Where(s => !string.IsNullOrEmpty(s)));
        protected static string list<T>(IEnumerable<T> arguments, string separator, Func<T, string> selector) =>
            list(arguments.Select(selector), separator);
    }
}
