using Ion.Reflect;

namespace Ion;

public record class TemplateModelNumber() : TemplateModel()
{
    /*
    private static readonly Dictionary<Type, Vector3<object>> DefaultRanges = new()
    {
        { typeof(byte),
            new(byte.MinValue, byte.MaxValue, (byte)1) },
        { typeof(DateTime),
            new(DateTime.MinValue, DateTime.MaxValue, DateTime.Now) },
        { typeof(decimal),
            new(decimal.MinValue, decimal.MaxValue, (decimal)1) },
        { typeof(Degree),
            new(Degree.Minimum, Degree.Maximum, (Degree)1) },
        { typeof(double),
            new(double.MinValue, double.MaxValue, (double)1) },
        { typeof(short),
            new(short.MinValue, short.MaxValue, (short)1) },
        { typeof(int),
            new(int.MinValue, int.MaxValue, (int)1) },
        { typeof(long),
            new(long.MinValue, long.MaxValue, (long)1) },
        { typeof(One),
            new(One.Minimum, One.Maximum, (One)0.01) },
        { typeof(Radian),
            new(Radian.Minimum, Radian.Maximum, (Radian)0.01745329) },
        { typeof(sbyte),
            new(sbyte.MinValue, sbyte.MaxValue, (sbyte)1) },
        { typeof(float),
            new(float.MinValue, float.MaxValue, (float)1) },
        { typeof(TimeSpan),
            new(TimeSpan.MinValue, TimeSpan.MaxValue, TimeSpan.FromSeconds(1)) },
        { typeof(UDouble),
            new(UDouble.MinValue, UDouble.MaxValue, (UDouble)1) },
        { typeof(ushort),
            new(ushort.MinValue, ushort.MaxValue, (ushort)1) },
        { typeof(uint),
            new(uint.MinValue, uint.MaxValue, (uint)1) },
        { typeof(ulong),
            new(ulong.MinValue, ulong.MaxValue, (ulong)1) },
        { typeof(USingle),
            new(USingle.MinValue, USingle.MaxValue, (USingle)1) },
    };
    */

    public override void Set(IMemberStylable model)
    {
        base.Set(model);
        /*
        if (model.ValueType is not null && DefaultRanges.ContainsKey(model.ValueType))
        {
            var j = DefaultRanges[model.ValueType];

            var a = model.Style.GetValue<Styles.NumberAttribute, object>(i => i.Step);
            var b = model.Style.GetValue<Styles.NumberAttribute, object>(i => i.Maximum);
            var c = model.Style.GetValue<Styles.NumberAttribute, object>(i => i.Minimum);

            model.Style.SetValue<Styles.NumberAttribute, object>
                (i => i.Step,
                a is null
                ? j.Z : a);

            model.Style.SetValue<Styles.NumberAttribute, object>
                (i => i.Maximum,
                b is null
                ? j.Y : b);

            model.Style.SetValue<Styles.NumberAttribute, object>
                (i => i.Minimum,
                c is null
                ? j.X : c);
        }
        */
    }
}