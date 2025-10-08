using Ion.Input;
using Ion.Numeral;
using Ion.Reflect;
using System.Windows.Input;

namespace Ion;

public record class TemplateModelMatrix() : TemplateModel()
{
    public ICommand InvertCommand => Commands[nameof(InvertCommand)]
        ??= new RelayCommand<IMemberInfo>(i => i.Value.To<IMatrix>().NewType(j => (double)j).Invert().Do(j => i.Value = j), i => i.Value is IMatrix);
}