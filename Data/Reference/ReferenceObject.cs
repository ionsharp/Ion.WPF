using Ion.Reflect;
using System.Windows;

namespace Ion.Data;

public class ReferenceObject() : Reference<MemberBase>()
{
    public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(MemberBase), typeof(ReferenceObject), new FrameworkPropertyMetadata(null));
    public MemberBase Model
    {
        get => (MemberBase)GetValue(ModelProperty);
        set => SetValue(ModelProperty, value);
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == DataProperty)
        {
            MemberBase result = null;
            if (Data is not null)
            {
                result = new()
                {
                    Value = Model
                };
            }
            SetCurrentValue(ModelProperty, result);
        }
    }
}