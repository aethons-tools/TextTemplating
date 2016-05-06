using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AethonsTools.TextTemplating.CSharp
{
    /// <summary>
    /// A code generator with additional convenience methods for formatting C# code.
    /// </summary>
    public abstract class CSharpTemplate : TextTemplate
    {
        protected static string args(IEnumerable<string> arguments) =>
            list(arguments, ", ");
        protected static string args<T>(IEnumerable<T> arguments, Func<T, string> selector) =>
            list(arguments, ", ", selector);

        protected static string parms<T>(IEnumerable<T> parameters, Func<T, string> nameSelector, Func<T, string> typeSelector) =>
            list(parameters.Select(p => $"{typeSelector(p)} {nameSelector(p)}").Where(s => !string.IsNullOrEmpty(s)), ", ");

        /// <summary>
        /// Generates a C# parameter list.
        /// </summary>
        /// <typeparam name="T">Parameter model type</typeparam>
        /// <param name="parameters">List of parameter models</param>
        /// <param name="nameSelector">Function to get the parameter name from the model</param>
        /// <param name="typeSelector">Function to get the parameter type symbol from the model</param>
        /// <returns>The formatted parameter list, ex: `string name, int age`</returns>
        protected static string parms<T>(IEnumerable<T> parameters, Func<T, string> nameSelector,
            Func<T, INamedTypeSymbol> typeSelector) =>
                parms(parameters, nameSelector, p => typeSelector(p).ToDisplayString());
    }
}
