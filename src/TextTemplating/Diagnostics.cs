using System;
using System.Linq;
using System.Reflection;

namespace AethonsTools.TextTemplating
{
    public static class Descriptors
    {
        public static readonly BasicDiagnosticDescriptor CannotFindDefaultTemplateMethod =
            new BasicDiagnosticDescriptor(Severity.Information, "TT201", "Cannot Find Default Template Method");

        public static readonly BasicDiagnosticDescriptor AmbiguousDefaultTemplateMethod =
            new BasicDiagnosticDescriptor(Severity.Error, "TT002", "Ambiguous Default Template Method");

        public static readonly BasicDiagnosticDescriptor TemplatingException =
            new BasicDiagnosticDescriptor(Severity.Error, "TT003", "Templating Exception");

        public static readonly BasicDiagnosticDescriptor TemplateMethodMustReturnTemplateResult =
            new BasicDiagnosticDescriptor(Severity.Error, "TT005", "Template Method Must Return TemplateResult");
    }



    internal static class Diagnostics
    {
        public static BasicDiagnostic CannotFindDefaultTemplateMethod(Type modelType, TextTemplate template) =>
            new BasicDiagnostic(Descriptors.CannotFindDefaultTemplateMethod,
                $"Cannot find method '{template?.GetType()?.Name}.For' that can accept model type {modelType?.Name}");

        public static BasicDiagnostic AmbiguousDefaultTemplateMethod(Type modelType, TextTemplate template) =>
            new BasicDiagnostic(Descriptors.AmbiguousDefaultTemplateMethod,
                $"Multiple matches found attempting to find '{template?.GetType()?.Name}.For' that can accept model type '{modelType?.Name}'");

        public static BasicDiagnostic TemplatingException(Exception exception) =>
            new BasicDiagnostic(Descriptors.TemplatingException,
                $"An exception was caught while templating:\n{FormatException(exception)}");

        public static BasicDiagnostic TemplateMethodMustReturnTemplateResult(MethodInfo method) =>
                new BasicDiagnostic(Descriptors.TemplateMethodMustReturnTemplateResult,
                    $"Template method {method?.DeclaringType?.Name}.{method?.Name}({method?.GetParameters().FirstOrDefault()?.ParameterType.Name}) should return TemplateResult");

        private static string FormatException(Exception x)
        {
            bool examine = true;
            while (examine)
            {
                examine = false;
                if (x is TargetInvocationException)
                {
                    examine = true;
                    x = x.InnerException;
                }
            }
            return x.ToString();
        }
    }
}
