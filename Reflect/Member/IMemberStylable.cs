using System.Runtime.CompilerServices;

namespace Ion.Reflect;

public interface IMemberStylable : IMemberInfo
{
    Orient Orientation { get; }

    IMemberStylable Parent { get; }

    InstanceStyle Style { get; }

    View View { get; }

    /// <inheritdoc cref="System.Diagnostics.Debug.WriteLine(object)"/>
    void WriteLine(MemberLogType type, object message = null, [CallerMemberName] string sender = null, [CallerLineNumber] int line = 0);
}