using Ion.Analysis;
using Ion.Reflect;
using System;
using System.Windows.Data;

namespace Ion.Core;

/// <inheritdoc/>
public class AppAnalyzer(TypeAnalyzerOptions options) : AssemblyAnalyzer(options)
{
    protected override void OnTypeAnalyzed(Type type)
    {
        base.OnTypeAnalyzed(type);
        if (type.Implements<IValueConverter>())
        {
            if (!type.HasAttribute<ValueConversionAttribute>())
                Log.Write(new MemberMissingAttributeWarning<ValueConversionAttribute>(type));

            if (type.GetConstructor(Type.EmptyTypes) is null)
                Log.Write(new TypeMissingParameterlessConstructorWarning(type));
        }
    }
}