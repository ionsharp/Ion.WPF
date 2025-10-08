using Ion.Core;
using Ion.Data;
using Ion.Reflect;
using System;
using System.Collections.Generic;

namespace Ion.Controls;

public static partial class ObjectControlTemplate
{
    /// <summary>Any <see cref="Type"/> with template defined by a <see cref="ResourceKey"/> in the current assembly.</summary>
    public static readonly Dictionary<Type, (Type, Func<StyleAttribute>, Type)> Default = new()
    {        
        #region System

        {typeof(bool),
            (typeof(Styles.CheckAttribute),
            () => new Styles.CheckAttribute(Template.Check), null)},
        {typeof(byte),
            (typeof(Styles.NumberAttribute),
            () => new Styles.NumberAttribute(Template.Number) { ValueConvert = typeof(ConvertByteToDouble) }, null)},
        {typeof(char),
            (typeof(Styles.TextAttribute),
            () => new Styles.TextAttribute(Template.Text) { ValueConvert = typeof(ConvertCharacterToString), TextLength = 1 }, null)},
        {typeof(decimal),
            (typeof(Styles.NumberAttribute),
            () => new Styles.NumberAttribute(Template.Number) { ValueConvert = typeof(ConvertDecimalToDouble) }, null)},
        {typeof(double),
            (typeof(Styles.NumberAttribute),
            () => new Styles.NumberAttribute(Template.Number) { ValueConvert = typeof(ConvertObjectToDouble) }, null)},
        {typeof(float),
            (typeof(Styles.NumberAttribute),
            () => new Styles.NumberAttribute(Template.Number) { ValueConvert = typeof(ConvertSingleToDouble) }, null)},
        {typeof(int),
            (typeof(Styles.NumberAttribute),
            () => new Styles.NumberAttribute(Template.Number) { ValueConvert = typeof(ConvertInt32ToDouble) }, null)},
        {typeof(long),
            (typeof(Styles.NumberAttribute),
            () => new Styles.NumberAttribute(Template.Number) { ValueConvert = typeof(ConvertInt64ToDouble) }, null)},
        {typeof(object),
            (typeof(Styles.ObjectAttribute),
            () => new Styles.ObjectAttribute(), typeof(TemplateModelObject))},
        {typeof(sbyte),
            (typeof(Styles.NumberAttribute),
            () => new Styles.NumberAttribute(Template.Number) { ValueConvert = typeof(ConvertSByteToDouble) }, null)},
        {typeof(short),
            (typeof(Styles.NumberAttribute),
            () => new Styles.NumberAttribute(Template.Number) { ValueConvert = typeof(ConvertInt16ToDouble) }, null)},
        {typeof(string),
            (typeof(Styles.TextAttribute),
            () => new Styles.TextAttribute(Template.Text), null)},
        {typeof(uint),
            (typeof(Styles.NumberAttribute),
            () => new Styles.NumberAttribute(Template.Number) { ValueConvert = typeof(ConvertUInt32ToDouble) }, null)},
        {typeof(ulong),
            (typeof(Styles.NumberAttribute),
            () => new Styles.NumberAttribute(Template.Number) { ValueConvert = typeof(ConvertUInt64ToDouble) }, null)},
        {typeof(ushort),
            (typeof(Styles.NumberAttribute),
            () => new Styles.NumberAttribute(Template.Number) { ValueConvert = typeof(ConvertUInt16ToDouble) }, null)},
                        
        ///

        {typeof(System.Array),
            (typeof(Styles.TextAttribute),
            () => new Styles.ListAttribute(Template.List), typeof(TemplateModelList))},
        {typeof(System.DateTime),
            (typeof(StyleAttribute),
            () => new StyleAttribute(Template.Text)  { ValueConvert = typeof(ConvertDateTimeToString) }, null)},
        {typeof(System.Enum),
            (typeof(Styles.EnumAttribute),
            () => new Styles.EnumAttribute(Template.Enum), null)},
        /// <see cref="Guid"/>
        /// 36 characters (with hyphens)
        /// 38 characters (with curly braces)
        /// 22 characters (Base64 encoded)
        /// 16 characters (theoretical, using extended ASCII characters) 
        {typeof(System.Guid),
            (typeof(Styles.TextAttribute),
            () => new Styles.TextAttribute(Template.Text) { ValueConvert = typeof(ConvertGuidToString), TextExpression = Ion.Text.Expressions.Guid, TextLength = 38 }, null)},
        {typeof(System.TimeSpan),
            (typeof(StyleAttribute),
            () => new StyleAttribute(Template.Text) { ValueConvert = typeof(ConvertTimeSpanToString) }, null)},
        { typeof(System.TimeZoneInfo),
            (typeof(StyleAttribute),
            () => new StyleAttribute() {Template = nameof(TimeZoneInfo),
            TemplateType = typeof(ObjectControlTemplate) }, null)},
        {typeof(System.Type),
            (typeof(Styles.TextAttribute),
            () => new Styles.TextAttribute(Template.Text) { CanEdit = false, ValueConvert = typeof(ConvertTypeToString) }, null)},
        {typeof(System.Uri),
            (typeof(Styles.TextAttribute),
            () => new Styles.TextAttribute(Template.Text) { ValueConvert = typeof(ConvertUriToString) }, null)},
        {typeof(System.Version),
            (typeof(Styles.TextAttribute),
            () => new Styles.TextAttribute(Template.Text) { ValueConvert = typeof(ConvertVersionToString) }, null)},
                
        #endregion
        
        #region System.Collections

        {typeof(System.Collections.IList),
            (typeof(Styles.ListAttribute),
            () => new Styles.ListAttribute(Template.List), typeof(TemplateModelList))},
        {typeof(System.Collections.Specialized.INotifyCollectionChanged),
            (typeof(Styles.ListAttribute),
            () => new Styles.ListAttribute(Template.List), typeof(TemplateModelList))},
                        
        #endregion
        #region System.Collections.Generic

        {typeof(System.Collections.Generic.List<>),
            (typeof(Styles.ListAttribute),
            () => new Styles.ListAttribute(Template.List), typeof(TemplateModelList))},
        
        #endregion
        #region System.Collections.ObjectModel

        {typeof(System.Collections.ObjectModel.ObservableCollection<>),
            (typeof(Styles.ListAttribute),
            () => new Styles.ListAttribute(Template.List), typeof(TemplateModelList))},
        {typeof(System.Collections.ObjectModel.ReadOnlyCollection<>),
            (typeof(Styles.ListAttribute),
            () => new Styles.ListAttribute(Template.List), typeof(TemplateModelList))},
        {typeof(System.Collections.ObjectModel.ReadOnlyObservableCollection<>),
            (typeof(Styles.ListAttribute),
            () => new Styles.ListAttribute(Template.List), typeof(TemplateModelList))},
        
        #endregion

        #region System.Drawing

        {typeof(System.Drawing.Color),
            (typeof(Styles.ColorAttribute),
            () => new Styles.ColorAttribute(Template.ColorText) { ValueConvert = typeof(ConvertColorInt32ToString) }, typeof(TemplateModelColor))},
        
        #endregion
        
        #region System.Reflection

        {typeof(System.Reflection.MethodInfo),
            (typeof(Styles.ButtonAttribute),
            () => new Styles.ButtonAttribute(Template.Button), null)},
        
        #endregion

        #region System.Windows

        { typeof(System.Windows.FontStyle),
            (typeof(StyleAttribute),
            () => new StyleAttribute() {Template = nameof(FontStyle),
            TemplateType = typeof(ObjectControlTemplate) }, null)},
        { typeof(System.Windows.FontWeight),
            (typeof(StyleAttribute),
            () => new StyleAttribute() {Template = nameof(FontWeight),
            TemplateType = typeof(ObjectControlTemplate) }, null)},
        {typeof(System.Windows.Thickness),
            (typeof(Styles.TextAttribute),
            () => new Styles.TextAttribute(Template.Text) { ValueConvert = typeof(ConvertThicknessToString) }, null)},

        #endregion
        #region System.Windows.Data

        {typeof(System.Windows.Data.ListCollectionView),
            (typeof(Styles.ListAttribute),
            () => new Styles.ListAttribute(Template.List), typeof(TemplateModelList))},
        
        #endregion
        #region System.Windows.Input

        {typeof(System.Windows.Input.ICommand),
            (typeof(Styles.ButtonAttribute),
            () => new Styles.ButtonAttribute(Template.Button), null)},
        
        #endregion
        #region System.Windows.Media

        {typeof(System.Windows.Media.Color),
            (typeof(Styles.ColorAttribute),
            () => new Styles.ColorAttribute(Template.ColorText) { ValueConvert = typeof(ConvertColorToString) }, typeof(TemplateModelColor))},
        {typeof(System.Windows.Media.FontFamily),
            (typeof(StyleAttribute),
            () => new StyleAttribute() {Template = nameof(FontFamily),
            TemplateType = typeof(ObjectControlTemplate) }, null)},
        {typeof(System.Windows.Media.LinearGradientBrush),
            (typeof(StyleAttribute),
            () => new StyleAttribute(Template.Gradient) { ValueConvert = typeof(ConvertLinearGradientBrushToGradient) }, null)},
        {typeof(System.Windows.Media.PointCollection),
            (typeof(StyleAttribute),
            () => new StyleAttribute(Template.Point), null)},
        {typeof(System.Windows.Media.RadialGradientBrush),
            (typeof(StyleAttribute),
            () => new StyleAttribute(Template.Gradient) { ValueConvert = typeof(ConvertRadialGradientBrushToGradient) }, null)},
        {typeof(System.Windows.Media.SolidColorBrush),
            (typeof(Styles.ColorAttribute),
            () => new Styles.ColorAttribute(Template.ColorText) { ValueConvert = typeof(ConvertSolidColorBrushToString) }, typeof(TemplateModelColor))},

        #endregion
                
        #region Ion

        { typeof(Direction),
            (typeof(StyleAttribute),
            () => new StyleAttribute()
            {
                AlignX = AlignX.Left,
                Template = nameof(Direction),
                TemplateType = typeof(ObjectControlTemplate),
            },
            null)},
        {typeof(Ion.Flag),
            (typeof(Styles.EnumAttribute),
            () => new Styles.EnumAttribute(Template.EnumFlag), null)},
        {typeof(Ion.Images),
            (typeof(Styles.ImageAttribute),
            () => new Styles.ImageAttribute(Template.ImageColor) { ValueMode = BindMode.OneWay }, null)},
               
        #endregion
                 
        #region Ion.Collections

        {typeof(Ion.Collect.ListObservable<>),
            (typeof(Styles.ListAttribute),
            () => new Styles.ListAttribute(Template.List), typeof(TemplateModelList))},
        
        #endregion

        #region Ion.Colors

        {typeof(Ion.Colors.Gradient),
            (typeof(StyleAttribute),
            () => new StyleAttribute(Template.Gradient) { }, null)},
        { typeof(Ion.Colors.GradientPreview),
            (typeof(StyleAttribute),
            () => new StyleAttribute() {Template = nameof(GradientPreview),
            TemplateType = typeof(ObjectControlTemplate) }, null)},

        #endregion
      
        #region Ion.Core

        { typeof(Ion.Core.AppResources),
            (typeof(StyleAttribute),
                () => new StyleAttribute() {Template = nameof(AppResources),
                TemplateType = typeof(ObjectControlTemplate) }, null)},
        { typeof(Ion.Core.GroupItemForm),
            (typeof(StyleAttribute),
                () => new StyleAttribute() {Template = nameof(GroupItemForm),
                TemplateType = typeof(ObjectControlTemplate) }, null)},
                       
        #endregion
 
        #region Ion.Data

        { typeof(Ion.Data.SearchOptions),
            (typeof(StyleAttribute),
                () => new StyleAttribute() {Template = nameof(SearchOptions),
                TemplateType = typeof(ObjectControlTemplate) }, null)},
                    
        #endregion
                  
        #region Ion.Input

        {typeof(Ion.Input.Command),
            (typeof(Styles.ButtonAttribute),
            () => new Styles.ButtonAttribute(Template.Button), null)},
        {typeof(Ion.Input.Command<>),
            (typeof(Styles.ButtonAttribute),
            () => new Styles.ButtonAttribute(Template.Button), null)},
        {typeof(Ion.Input.RelayCommand),
            (typeof(Styles.ButtonAttribute),
            () => new Styles.ButtonAttribute(Template.Button), null)},
        {typeof(Ion.Input.RelayCommand<>),
            (typeof(Styles.ButtonAttribute),
            () => new Styles.ButtonAttribute(Template.Button), null)},

        #endregion

        #region Ion.Numeral

        { typeof(Ion.Numeral.Angle),
            (typeof(StyleAttribute),
            () => new StyleAttribute(Template.Angle) { ValueConvert = typeof(ConvertDegreeToDouble) }, null)},

        {typeof(Ion.Numeral.Shape),
            (typeof(StyleAttribute),
            () => new StyleAttribute(Template.Point), null)},
        {typeof(Ion.Numeral.ShapePoint),
            (typeof(StyleAttribute),
            () => new StyleAttribute(Template.Point), null)},
        {typeof(Ion.Numeral.ShapeVector),
            (typeof(StyleAttribute),
            () => new StyleAttribute(Template.Point), null)},

        {typeof(Ion.Numeral.ByteVector2),
            (typeof(Styles.ColorAttribute),
            () => new Styles.ColorAttribute(Template.ColorText) { ValueConvert = typeof(ConvertByteVector2ToString) }, typeof(TemplateModelColor))},
        {typeof(Ion.Numeral.ByteVector3),
            (typeof(Styles.ColorAttribute),
            () => new Styles.ColorAttribute(Template.ColorText) { ValueConvert = typeof(ConvertByteVector3ToString) }, typeof(TemplateModelColor))},
        {typeof(Ion.Numeral.ByteVector4),
            (typeof(Styles.ColorAttribute),
            () => new Styles.ColorAttribute(Template.ColorText) { ValueConvert = typeof(ConvertByteVector4ToString) }, typeof(TemplateModelColor))},

        {typeof(Ion.Numeral.IVector),
            (typeof(Styles.TextAttribute),
            () => new Styles.ListAttribute(), typeof(TemplateModelList))},
        {typeof(Ion.Numeral.Vector),
            (typeof(Styles.TextAttribute),
            () => new Styles.ListAttribute(), typeof(TemplateModelList))},
        {typeof(Ion.Numeral.Vector<>),
            (typeof(Styles.TextAttribute),
            () => new Styles.ListAttribute(), typeof(TemplateModelList))},
        {typeof(Ion.Numeral.Vector2),
            (typeof(Styles.TextAttribute),
            () => new Styles.ListAttribute(), typeof(TemplateModelList))},
        {typeof(Ion.Numeral.Vector2<>),
            (typeof(Styles.TextAttribute),
            () => new Styles.ListAttribute(), typeof(TemplateModelList))},
        {typeof(Ion.Numeral.Vector3),
            (typeof(Styles.TextAttribute),
            () => new Styles.ListAttribute(), typeof(TemplateModelList))},
        {typeof(Ion.Numeral.Vector3<>),
            (typeof(Styles.TextAttribute),
            () => new Styles.ListAttribute(), typeof(TemplateModelList))},
        {typeof(Ion.Numeral.Vector4),
            (typeof(Styles.TextAttribute),
            () => new Styles.ListAttribute(), typeof(TemplateModelList))},
        {typeof(Ion.Numeral.Vector4<>),
            (typeof(Styles.TextAttribute),
            () => new Styles.ListAttribute(), typeof(TemplateModelList))},

        {typeof(Ion.Numeral.IMatrix),
            (typeof(Styles.MatrixAttribute),
            () => new Styles.MatrixAttribute(Template.Matrix), typeof(TemplateModelMatrix))},

        {typeof(Ion.Numeral.Double1),
            (typeof(Styles.NumberAttribute),
            () => new Styles.NumberAttribute(Template.Number) { ValueConvert = typeof(ConvertDouble1ToDouble) }, null)},
        {typeof(Ion.Numeral.Single1),
            (typeof(Styles.NumberAttribute),
            () => new Styles.NumberAttribute(Template.Number) { ValueConvert = typeof(ConvertSingle1ToDouble) }, null)},
        {typeof(Ion.Numeral.UDouble),
            (typeof(Styles.NumberAttribute),
            () => new Styles.NumberAttribute(Template.Number) { ValueConvert = typeof(ConvertUDoubleToDouble) }, null)},
        {typeof(Ion.Numeral.USingle),
            (typeof(Styles.NumberAttribute),
            () => new Styles.NumberAttribute(Template.Number) { ValueConvert = typeof(ConvertUSingleToDouble) }, null)},

        {typeof(Ion.Numeral.Unit),
            (typeof(Styles.UnitAttribute), null, null)},

        #endregion

        #region Ion.Storage
                   
        {typeof(Ion.Storage.FileSize),
            (typeof(Styles.TextAttribute),
            () => new Styles.TextAttribute(Template.Text) { ValueConvert = typeof(ConvertFileSize) }, null)},
  
        #endregion

        #region Ion.Text

        {typeof(Ion.Text.Bullet),
            (typeof(Styles.EnumAttribute),
            () => new Styles.EnumAttribute(Template.Enum) { ItemTemplate = Bullet }, null)},
        
        #endregion
    };
}