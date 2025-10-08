using Ion.Controls;
using Ion.Core;
using Ion.Local;
using Ion.Text;
using System;
using System.Windows.Data;

namespace Ion.Data;

public class MultiBindLocalize : MultiBind
{
    private readonly Bind KeyBinding;

    ///

    public Type ConvertKey { set => KeyBinding.Convert = value; }

    ///

    public RelativeSourceMode From { set => KeyBinding.RelativeSource = new(value); }

    public Type FromType { set => KeyBinding.RelativeSource = new(RelativeSourceMode.FindAncestor) { AncestorType = value }; }

    ///

    public AppSource GlobalSource { set => KeyBinding.Source = Appp.GetSource(value); }

    ///

    public Casing Case { get; set; }

    public string Format { get; set; }

    public string Prefix { get; set; }

    public string Suffix { get; set; }

    ///

    public MultiBindLocalize() : this(Paths.Dot) { }

    public MultiBindLocalize(string path) : base()
    {
        Converter = new MultiValueConverter<string>(i => Try.Get(() => i.Values[0]?.ToString()?.Localize(Prefix, Suffix, Format, Case)));

        KeyBinding = new Bind(path);
        Bindings.Add(KeyBinding);

        Bindings.Add(new Bind(nameof(AppData.Language)) { AppSource = AppSource.Data });
    }

    public MultiBindLocalize(Type converter) : this(Paths.Dot, converter) { }

    public MultiBindLocalize(string path, Type converter) : this(path) => ConvertKey = converter;
}