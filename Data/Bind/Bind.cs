using Ion.Core;
using Ion.Data;
using Ion.Reflect;
using System;
using System.Linq;
using System.Windows.Data;

namespace Ion.Controls
{
    /// <inheritdoc/>
    public class Bind : Binding
    {
        /// <see cref="Region.Property"/>

        public AppSource AppSource { set => Source = Appp.GetSource(value); }

        private Type convert;
        public Type Convert
        {
            get => convert;
            set
            {
                convert = value;
                SetConverter();
            }
        }

        private MultiConverter convertMultiple;
        public MultiConverter ConvertMultiple
        {
            get => convertMultiple;
            set
            {
                convertMultiple = value;
                SetConverter();
            }
        }

        private Type convertNext;
        public Type ConvertNext
        {
            get => convertNext;
            set
            {
                convertNext = value;
                SetConverter();
            }
        }

        public RelativeSourceMode From { set => RelativeSource = value == RelativeSourceMode.FindAncestor ? new(RelativeSourceMode.FindAncestor) { AncestorType = fromType } : new(value); }

        private Type fromType;
        public Type FromType { get => fromType; set { fromType = value; RelativeSource = new(RelativeSourceMode.FindAncestor) { AncestorType = value }; } }

        public UpdateSourceTrigger Trigger { set => UpdateSourceTrigger = value; }

        public int Way { set => Mode = value == 0 ? BindingMode.OneWayToSource : value == 1 ? BindingMode.OneWay : value == 2 ? BindingMode.TwoWay : throw new NotSupportedException(); }

        /// <see cref="Region.Constructor"/>

        public Bind() : this(Paths.Dot) { }

        public Bind(string path) : base(path)
        {
            Trigger = UpdateSourceTrigger.PropertyChanged; Way = 1;
        }

        public Bind(string path, AppSource source) : this(path)
        {
            AppSource = source;
        }

        public Bind(Type converter, object converterParameter) : this()
        {
            Convert = converter; ConverterParameter = converterParameter;
        }

        /// <see cref="Region.Method"/>

        private void SetConverter()
        {
            IValueConverter result = null;
            if (Convert is not null)
            {
                if (ConvertNext is null)
                    result = ValueConverter.Cache[Convert];

                if (ConvertNext is not null)
                {
                    result = new ValueConverter<object, object>(true, i =>
                    {
                        var result = i.Value;
                        result = ValueConverter.Cache[Convert]
                            .Convert(result, i.TargetType, i.ActualParameter, i.Culture);
                        result = ValueConverter.Cache[ConvertNext]
                            .Convert(result, i.TargetType, i.ActualParameter, i.Culture);
                        return result;
                    });
                }
            }
            if (ConvertMultiple is not null)
            {
                result = new ValueConverter<object, object>(true, i =>
                {
                    var result = i.Value;
                    foreach (var j in convertMultiple)
                    {
                        if (j.Type is not null)
                            result = ValueConverter.Cache[j.Type].Convert(result);
                    }

                    return result;
                },
                i => default);

            }
            Converter = result;
        }
    }
}

namespace Ion.Data
{
    using Ion.Controls;

    /// <inheritdoc/>
    public abstract class BindInvert(string path) : Bind(path)
    {
        protected bool invert;
        public virtual bool Invert
        {
            get => invert;
            set
            {
                invert = value;
                ConverterParameter = value ? 1 : 0;
            }
        }

        protected BindInvert() : this(Paths.Dot) { }
    }

    /// <summary>Gets panel by type. Panels managed by <see cref="IDockAppModel"/>. To work, remove, then add panel; don't replace.</summary>
    /// <remarks>Updates when <see cref="IDockAppModel.ViewModel"/>, <see cref="IDockViewModel.Panels"/>, or <see cref="PanelCollection"/> changes.</remarks>
    public class BindPanel : MultiBind
    {
        public Type Panel
        {
            set
            {
                Bindings.Clear();
                Bindings.Add(new Binding()
                { Source = value });

                Bindings.Add(new Binding(nameof(IDockAppModel.ViewModel))
                { Source = Appp.Model });

                Bindings.Add(new Binding($"{nameof(IDockAppModel.ViewModel)}.{nameof(IDockViewModel.Panels)}")
                { Source = Appp.Model });

                Bindings.Add(new Binding($"{nameof(IDockAppModel.ViewModel)}.{nameof(IDockViewModel.Panels)}.{nameof(PanelCollection.Count)}")
                { Source = Appp.Model });

                Converter = new MultiValueConverter(i =>
                {
                    if (i.Values[0] is Type a)
                    {
                        if (i.Values[1] is IDockViewModel b)
                        {
                            if (i.Values[2] is PanelCollection c)
                                return c.FirstOrDefault(j => j.GetType() == a);
                        }
                    }
                    return null;
                });
            }
        }
    }

    /// <inheritdoc/>
    public abstract class BindResult : BindInvert
    {
        public enum Results { Boolean, Visibility }

        public override bool Invert { get => invert; set => SetValue(ref invert, value); }

        protected Results result = Results.Boolean;
        public virtual Results Result { get => result; set => SetValue(ref result, value); }

        protected BindResult() : this(Paths.Dot, null) { }

        protected BindResult(string path, IValueConverter converter) : base(path) => Converter = converter;

        protected static object GetResult(bool i, bool invert, Results result)
        {
            i = invert ? !i : i;
            return result switch
            {
                Results.Boolean => i,
                Results.Visibility => i.ToVisibility()
            };
        }

        protected abstract object GetConverterParameter();

        protected void SetConverterParameter() => ConverterParameter = GetConverterParameter();

        protected void SetValue<T>(ref T field, T value)
        {
            field = value;
            SetConverterParameter();
        }
    }

    /// <inheritdoc/>
    public class BindSection : Bind
    {
        public View Section { get; set; }

        public BindSection() : this(Paths.Dot) { }

        public BindSection(string path) : base(path)
        {
            Converter = new ValueConverter<object, SourceFilter>(i => new SourceFilter(i.Value, Section));
        }
    }

    /// <inheritdoc/>
    public class Ancestor : Bind
    {
        public Ancestor() : this(Paths.Dot, null) { }

        public Ancestor(Type fromType) : this(Paths.Dot, fromType) { }

        public Ancestor(string path, Type fromType) : base(path) { FromType = fromType; }
    }
}