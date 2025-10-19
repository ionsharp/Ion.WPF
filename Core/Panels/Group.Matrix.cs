using Ion;
using Ion.Collect;
using Ion.Colors;
using Ion.Controls;
using Ion.Numeral;
using Ion.Reflect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ion.Core;

public class DefaultMatrices : ItemGroup<IMatrix>
{
    public static IMatrix Gaussian3x3 => new Matrix3x3<double>(4, 2, 1, Matrix3x3Fill.Alternate);

    public static IMatrix Gaussian5x5 => new Matrix<double>
    ([
        [2, 04, 05, 04, 2 ],
        [4, 09, 12, 09, 4 ],
        [5, 12, 15, 12, 5 ],
        [4, 09, 12, 09, 4 ],
        [2, 04, 05, 04, 2 ],
    ]);

    public DefaultMatrices() : base("Default")
    {
        for (var i = 3; i < 15; i += 2)
            Add(new($"Mean Blur {i}x{i}", new Matrix<double>(i, 1)));

        Add(nameof(Gaussian3x3), Gaussian3x3);
        Add(nameof(Gaussian5x5), Gaussian5x5);

        for (var i = 3; i < 15; i++)
            Add(new($"Motion Blur {i}x{i}", new Matrix<double>(i, 1, 0, MatrixFill.DiagonalBoth)));

        for (var i = 3; i < 15; i++)
            Add(new($"Motion Blur {i}x{i} At 135°", new Matrix<double>(i, 1, 0, MatrixFill.DiagonalRight)));

        for (var i = 3; i < 15; i++)
            Add(new($"Motion Blur {i}x{i} At 45°", new Matrix<double>(i, 1, 0, MatrixFill.DiagonalLeft)));
    }
}

[Image(Images.Matrix), Name("Matrix")]
[Styles.Object(Strict = MemberTypes.All, MemberViewType = MemberViewType.Tab)]
[Description("Manage groups of matrices.")]
public record class MatrixPanel(IListWritable input) : DataGroupPanel<IMatrix>(input)
{
    private enum Group { AddRemove }

    [TabView(View = Ion.View.Main)]
    private new enum Tab { }

    public static readonly new ResourceKey Template = new();

    public override string ItemName => "Matrix";

    public override IEnumerable<Type> ItemTypes
    {
        get
        {
            yield return typeof(ChromacityMatrix);
            yield return typeof(Matrix<double>);
        }
    }

    protected override Dictionary<Type, Func<object>> ItemTypeHandlers => new()
    {
        { typeof(Matrix<double>),
            () => new Matrix<double>(3) },
        { typeof(ChromacityMatrix),
            () => new ChromacityMatrix(ColorProfile.Default) },
    };

    public MatrixPanel() : this(default(IListWritable)) { }

    protected override IEnumerable<ItemGroup<IMatrix>> GetDefaultGroups()
    {
        yield return new DefaultMatrices();
        yield return new ItemGroup<IMatrix>("LMS",
            typeof(ChromacityAdaptationTransform).GetProperties()
            .Where(i => i.PropertyType.Implements<IMatrix>())
            .Select(i => new Item<IMatrix>(Instance.GetName(i), Instance.GetDescription(i), (IMatrix)i.GetValue(null))));
    }

    protected override object GetDefaultItem() => new Matrix3x3<double>();

    protected override Images GetItemIcon() => Images.Matrix;
}