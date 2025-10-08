using Ion.Controls;
using Ion.Imaging;
using Ion.Input;
using Ion.Numeral;
using Ion.Storage;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;

namespace Ion.Core;

/// <inheritdoc/>
public record class ColorFileDockViewModel() : FileDockViewModel<ColorFileDockViewModelData>()
{
    /// <see cref="Region.Property"/>

    public override int DefaultLayout => (int)ColorViewLayout.All;

    public override Uri[] DefaultLayouts =>
    [
        Resource.GetUri(AssemblyData.Name, $"{nameof(Controls)}/{nameof(ColorView)}/" + ContentSerializer.InternalLayoutPath + $"{ColorViewLayout.All}.xml"),
        Resource.GetUri(AssemblyData.Name, $"{nameof(Controls)}/{nameof(ColorView)}/" + ContentSerializer.InternalLayoutPath + $"{ColorViewLayout.Analyze}.xml"),
        Resource.GetUri(AssemblyData.Name, $"{nameof(Controls)}/{nameof(ColorView)}/" + ContentSerializer.InternalLayoutPath + $"{ColorViewLayout.Collect}.xml"),
        Resource.GetUri(AssemblyData.Name, $"{nameof(Controls)}/{nameof(ColorView)}/" + ContentSerializer.InternalLayoutPath + $"{ColorViewLayout.Explore}.xml"),
        Resource.GetUri(AssemblyData.Name, $"{nameof(Controls)}/{nameof(ColorView)}/" + ContentSerializer.InternalLayoutPath + $"{ColorViewLayout.Select}.xml")
    ];

    public override IEnumerable<string> ReadableFileExtensions => ["color"];

    /// <see cref="Region.Method"/>
    #region

    private void OnColorSaved(object sender, EventArgs<Color> e)
    {
        Panels.FirstOrDefault<ColorPanel>()?.SelectedGroup.IfNotNull(i =>
        {
            e.A.Convert(out ByteVector4 j);
            //i.Add("", j);
        });
    }

    private void OnColorSelected(object sender, EventArgs<ByteVector4> e)
        => ActiveDocument.If<ColorDocument>(i => i.NewColor = Color.FromArgb(e.A.A, e.A.R, e.A.G, e.A.B));

    private void OnHarmonyColorPicked(object sender, EventArgs<Color> e) => ActiveDocument.If<ColorDocument>(i => i.NewColor = e.A);

    private void SaveTo(Color input)
    {
        var item = new GroupValueForm<ByteVector4>(Panels.FirstOrDefault<ColorPanel>().Groups, input, Panels.FirstOrDefault<ColorPanel>().SelectedGroupIndex);

        Dialog.ShowObject($"Save color", item, Resource.GetImageUri(Images.Save), i =>
        {
            if (i == 0)
            {
                if (item.GroupIndex > -1)
                {
                    var a = (Color)item.Value;
                    a.Convert(out ByteVector4 b);

                    Panels.FirstOrDefault<ColorPanel>().Groups[item.GroupIndex].Add("", b);
                }
            }
        },
        Buttons.SaveCancel);
    }

    ///

    protected override Type GetDocumentType(string fileExtension) => typeof(ColorDocument);

    protected override SerializationType GetSerializationType(string fileExtension) => SerializationType.Binary;

    ///

    protected override void OnDocumentAdded(Document document)
    {
        base.OnDocumentAdded(document);
        document.If<ColorDocument>(i =>
        {
            i.ColorSaved
                += OnColorSaved;
            i.Profiles
                = Data.Profiles;
        });
    }

    protected override void OnDocumentRemoved(Document document)
    {
        base.OnDocumentRemoved(document);
        document.As<ColorDocument>().ColorSaved -= OnColorSaved;
    }

    ///

    public override IEnumerable<Panel> GetPanels()
    {
        yield return new ClipboardPanel();
        yield return new ColorPanel();
        yield return new ColorAnalysisPanel();
        yield return new ColorChromacityPanel();
        yield return new ColorDifferencePanel();
        yield return new ColorHarmonyPanel();
        yield return new IlluminantPanel();
        yield return new MatrixPanel();
        yield return new ProfilePanel();
        yield return new GradientPanel();
        yield return new PropertyPanel();
    }

    public override void Subscribe()
    {
        base.Subscribe();
        Documents.ForEach<ColorDocument>(j =>
        {
            j.ColorSaved += OnColorSaved;
            j.Profiles = Data.Profiles;
        });

        Panels.FirstOrDefault<ColorPanel>()
            .Selected += OnColorSelected;
        Panels.FirstOrDefault<ColorHarmonyPanel>()
            .Picked += OnHarmonyColorPicked;

        Panels.FirstOrDefault<ColorPanel>()
            .Groups = Data.Colors;
        Panels.FirstOrDefault<GradientPanel>()
            .Groups = Data.Gradients;
        Panels.FirstOrDefault<IlluminantPanel>()
            .Groups = Data.Illuminants;
        Panels.FirstOrDefault<MatrixPanel>()
            .Groups = Data.Matrices;
        Panels.FirstOrDefault<ProfilePanel>()
            .Groups = Data.Profiles;

        Panels.FirstOrDefault<ColorAnalysisPanel>()
            .Profiles = Data.Profiles;
        Panels.FirstOrDefault<ColorAnalysisPanel>()
            .Profile = new(Data.Profiles, 0, 0);

        Panels.FirstOrDefault<ColorDifferencePanel>()
            .Profiles = Data.Profiles;
        Panels.FirstOrDefault<ColorDifferencePanel>()
            .Profile1 = new(Data.Profiles, 0, 0);
        Panels.FirstOrDefault<ColorDifferencePanel>()
            .Profile2 = new(Data.Profiles, 0, 0);
    }

    public override void Unsubscribe()
    {
        base.Unsubscribe();
        Documents.ForEach<ColorDocument>(j => j.ColorSaved -= OnColorSaved);

        Panels.FirstOrDefault<ColorPanel>().Selected
            -= OnColorSelected;
        Panels.FirstOrDefault<ColorHarmonyPanel>().Picked
            -= OnHarmonyColorPicked;
    }

    #endregion

    /// <see cref="ICommand"/>
    #region 

    public ICommand CopyColorCommand => Commands[nameof(CopyColorCommand)]
        ??= new RelayCommand<Color>(i => Appp.Cache.Add([i]), i => i != null);

    public ICommand CopyHexadecimalCommand => Commands[nameof(CopyHexadecimalCommand)]
        ??= new RelayCommand<Color>(i =>
        {
            i.Convert(out ByteVector4 result);
            System.Windows.Clipboard.SetText(result.XYZ.ToString());
        },
        i => i != null);

    public ICommand PasteOldColorCommand => Commands[nameof(PasteOldColorCommand)]
        ??= new RelayCommand(() => ActiveDocument.As<ColorDocument>().OldColor = Appp.Cache.Get<Color>(),
        () => ActiveDocument.As<ColorDocument>() != null && Appp.Cache.Contains<Color>());

    public ICommand PasteNewColorCommand => Commands[nameof(PasteNewColorCommand)]
        ??= new RelayCommand(() => ActiveDocument.As<ColorDocument>().NewColor = Appp.Cache.Get<Color>(), () => ActiveDocument.As<ColorDocument>() != null && Appp.Cache.Contains<Color>());

    public ICommand SaveOldColorCommand => Commands[nameof(SaveOldColorCommand)]
        ??= new RelayCommand(() => Panels.FirstOrDefault<ColorPanel>()?.SelectedGroup.IfNotNull(i =>
        {
            ActiveDocument.As<ColorDocument>().OldColor.Convert(out ByteVector4 j);
            //i.Add("", j);
        }),
        () => ActiveDocument.As<ColorDocument>() != null);

    public ICommand SaveOldColorToCommand => Commands[nameof(SaveOldColorToCommand)]
        ??= new RelayCommand(() => Panels.FirstOrDefault<ColorPanel>()?.SelectedGroup.IfNotNull(i => SaveTo(ActiveDocument.As<ColorDocument>().OldColor)), () => ActiveDocument.As<ColorDocument>() != null);

    public ICommand SaveNewColorCommand => Commands[nameof(SaveNewColorCommand)]
        ??= new RelayCommand(() => Panels.FirstOrDefault<ColorPanel>()?.SelectedGroup.IfNotNull(i =>
        {
            ActiveDocument.As<ColorDocument>().NewColor.Convert(out ByteVector4 j);
            //i.Add("", j);
        }),
        () => ActiveDocument.As<ColorDocument>() != null);

    public ICommand SaveNewColorToCommand => Commands[nameof(SaveNewColorToCommand)]
        ??= new RelayCommand(() => Panels.FirstOrDefault<ColorPanel>()?.SelectedGroup.IfNotNull(i => SaveTo(ActiveDocument.As<ColorDocument>().NewColor)), () => ActiveDocument.As<ColorDocument>() != null);

    #endregion
}