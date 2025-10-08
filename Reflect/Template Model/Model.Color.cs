using Ion.Colors;
using Ion.Media;
using Ion.Numeral;
using Ion.Reflect;
using System;

namespace Ion;

public record class TemplateModelColor() : TemplateModel()
{
    public ColorViewModel ViewModel { get => Get<ColorViewModel>(); set => Set(value); }

    public int Length
    {
        get
        {
            if (Model?.Style.GetValue<Styles.ColorAttribute, bool>(i => i.Alpha) == true)
            {
                if (Model.ValueType != typeof(ByteVector3))
                    return 8;
            }
            return 6;
        }
    }

    public override void Set(IMemberStylable model)
    {
        base.Set(model);
        ViewModel = ColorViewModel.New(model.Style.GetValue<Styles.ColorAttribute, Type>(x => x.ColorModel).Create<IColor>());
        ViewModel.Normalize = model.Style.GetValue<Styles.ColorAttribute, bool>
            (x => x.Normalize);
        ViewModel.Precision = model.Style.GetValue<Styles.ColorAttribute, int>
            (x => x.Precision);
    }
}