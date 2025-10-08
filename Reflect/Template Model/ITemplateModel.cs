using Ion.Core;

namespace Ion.Reflect;

/// <summary>A model used by <see cref="StyleAttribute"/> to implement template logic.</summary>
/// <remarks>Implements <see cref="ISubscribe"/>.</remarks>
public interface ITemplateModel : ISubscribe
{
    IMemberStylable Model { get; }

    /// <summary>Reset the style model with a new value.</summary>
    /// <param name="source">A new value.</param>
    void Reset((object OldValue, object NewValue) value);

    /// <summary>Set the style model.</summary>
    /// <param name="model">An instance of <see cref="IMemberInfo"/>.</param>
    void Set(IMemberStylable model);

    /// <summary>Unset the style model.</summary>
    /// <param name="model">An instance of <see cref="IMemberInfo"/>.</param>
    void Unset(IMemberStylable model);
}