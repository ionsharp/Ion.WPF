using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Ion.Controls;

[ContentProperty(nameof(Templates))]
public class KeyTemplateSelector : DataTemplateSelector
{
    public DataTemplate Default { get; set; }

    public List<KeyTemplate> Templates { get; set; } = [];

    public override DataTemplate SelectTemplate(object key, DependencyObject container)
        => Templates.FirstOrDefault(i => key?.Equals(i.DataKey) == true) ?? Default;
}