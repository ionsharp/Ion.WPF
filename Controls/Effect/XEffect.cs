using Ion.Data;
using Ion.Effects;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Ion.Controls;

/// <summary>For displaying multiple effects on the same <see cref="Border"/>! :)</summary>
[Extend<Border>]
[Using<EffectList>]
public static class XEffect
{
    private static readonly Dictionary<Border, EffectList> targets = [];

    ///

    public static readonly DependencyProperty EffectsProperty = DependencyProperty.RegisterAttached("Effects", typeof(EffectList), typeof(XEffect), new FrameworkPropertyMetadata(null, OnEffectsChanged));
    public static EffectList GetEffects(Border i) => (EffectList)i.GetValue(EffectsProperty);
    public static void SetEffects(Border i, EffectList input) => i.SetValue(EffectsProperty, input);

    private static void OnEffectsChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is Border border)
        {
            border.AddHandlerAttached(e.NewValue != null, EffectsProperty, i =>
            {
                var effects = (EffectList)e.NewValue;
                if (!targets.ContainsKey(border))
                    targets.Add(border, effects);

                Unsubscribe(effects);
                Subscribe(effects);

                Update(border);
            }, i =>
            {
                if (targets.ContainsKey(border))
                {
                    Unsubscribe(targets[border]);
                    targets.Remove(border);
                }
            });
        }
    }

    private static void OnEffectsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        var result = targets.First(i => ReferenceEquals(i.Value, (EffectList)sender)).Key;
        Update(result);
    }

    ///

    private static void Subscribe(EffectList effects)
    {
        effects.CollectionChanged += OnEffectsChanged;
    }

    private static void Unsubscribe(EffectList effects)
    {
        effects.CollectionChanged -= OnEffectsChanged;
    }

    ///

    private static void Update(Border border)
    {
        var effects = GetEffects(border);

        Border parent = border, lastParent = null;

        UIElement content = null;
        while (parent != null)
        {
            content = parent.Child;
            lastParent = parent;
            parent = content as Border;
        }

        if (lastParent != null)
            lastParent.Child = null;

        border.Child = null;
        border.Child = content;

        if (effects?.Count > 0)
        {
            Border a = new(), b = null;

            var c = border.Child;
            border.Child = null;

            a.Child = c;
            foreach (var i in effects)
            {
                a.Effect = i;

                var converter = new MultiValueConverter<ImageEffect>(j => j.Values?.Length == 2 && j.Values[0] is bool a && j.Values[1] is bool b && a && b ? j.Parameter as ImageEffect : null);
                a.MultiBind(UIElement.EffectProperty, converter, i, new System.Windows.Data.Binding(nameof(EffectList.IsVisible)) { Source = effects }, new System.Windows.Data.Binding(nameof(ImageEffect.IsVisible)) { Source = i });

                b = new Border { Child = a };
                a = b;
            }

            border.Child = a;
        }
    }
}