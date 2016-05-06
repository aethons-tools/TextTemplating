using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AethonsTools.TextTemplating
{
    public sealed class TemplateResult
    {
        public string Output { get; }
        public IEnumerable<BasicDiagnostic> Diagnostics { get; }

        public TemplateResult(string output, IEnumerable<BasicDiagnostic> diagnostics)
        {
            Output = output;
            Diagnostics = diagnostics; // TODO: make safe and immutable
        }

        public TemplateResult(string output) : this(output, Enumerable.Empty<BasicDiagnostic>())
        { }

        public TemplateResult(BasicDiagnostic diagnostic) : this(string.Empty, new[] { diagnostic })
        { }

        public TemplateResult(string output, BasicDiagnostic diagnostic) : this(output, new[] { diagnostic })
        { }

        public TemplateResult(IEnumerable<BasicDiagnostic> diagnostics) : this(string.Empty, diagnostics)
        { }

        //public static implicit operator TemplateResult(FormattableString pending)
        //{
        //    // TODO: manage exception from expansion
        //    return new TemplateResult(pending.ToString(),
        //        pending.GetArguments()
        //            .OfType<TemplateResult>()
        //            .SelectMany(tr => tr.Diagnostics));
        //}

        public static implicit operator TemplateResult(BasicDiagnostic diagnostic)
        {
            return new TemplateResult(diagnostic);
        }

        public override string ToString() => Output ?? string.Empty;
    }

    //public interface IDiagnosable<out T> where T : IDiagnostic
    //{
    //    IEnumerable<T> Diagnostics { get; }
    //}

    //public sealed class FormattableStringWithDiagnostics : FormattableString, IDiagnosable<BasicDiagnostic>
    //{
    //    private readonly BasicDiagnostic[] _localDiagnostics;
    //    public IEnumerable<BasicDiagnostic> Diagnostics => _localDiagnostics.Concat(_args.SelectMany(_args.))
    //    public override string Format { get; }
    //    public override int ArgumentCount => _args.Length;

    //    private readonly ImmutableArray<object> _args;

    //    public FormattableStringWithDiagnostics(string format, IEnumerable<BasicDiagnostic> diagnostics,
    //        IEnumerable<object> args)
    //    {
    //        Format = format;
    //        _localDiagnostics = diagnostics.ToArray();
    //        _args = args.ToImmutableArray();
    //    }

    //    public override object[] GetArguments() => _args.ToArray();

    //    public override object GetArgument(int index) => _args[index];

    //    public override string ToString(IFormatProvider formatProvider)
    //        => string.Format(formatProvider, Format, GetArguments());

    //    public override string ToString() => ToString(null);

    //}
}