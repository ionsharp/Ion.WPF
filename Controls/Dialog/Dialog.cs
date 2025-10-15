using Ion.Analysis;
using Ion.Collect;
using Ion.Colors;
using Ion.Controls;
using Ion.Core;
using Ion.Data;
using Ion.Numeral;
using Ion.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Ion;

public static class Dialog
{
    /// <see cref="Region.Class"/>
    #region

    /// <see cref="ColorModel"/>
    #region

    public record class ColorModel : Model
    {
        public readonly ColorDocument Document;

        public ColorDocument ActiveDocument { get => Get<ColorDocument>(); set => Set(value); }

        public ColorFileDockViewModel ViewModel { get => Get<ColorFileDockViewModel>(); private set => Set(value); }

        public ColorModel(Color color) : base()
        {
            ViewModel = new()
            {
                DataFileExtension
                    = "data",
                DataFileName
                    = nameof(System.Windows.Forms.ColorDialog),
                DataFolderPath
                    = @$"{Appp.Model.DataFolderPathShared}\{nameof(Control)}",
            };

            ViewModel.Load();
            ViewModel.Subscribe();

            var document
                = new ColorDocument(color, typeof(HSB), ViewModel.Data.Profiles) { CanClose = false, CanFloat = false, NewColor = color };

            ViewModel.Documents.Add(document);
            Document = document;
        }

        internal void OnClosed() => ViewModel.Data.Serialize();
    }

    #endregion

    public record class DownloadModel() : Model()
    {
        public bool AutoClose { get => Get(false); set => Set(value); }

        public bool AutoStart { get => Get(false); set => Set(value); }

        public string Destination { get => Get(""); set => Set(value); }

        public string Source { get => Get(""); set => Set(value); }

        public string Message { get => Get(""); set => Set(value); }

        public string Title { get => Get(""); set => Set(value); }

        public DataTemplate MessageTemplate { get => Get<DataTemplate>(); set => Set(value); }

        public DownloadModel(string title, string message, string source, string destination) : this()
        {
            Source
                = source;
            Destination
                = destination;
            Message
                = message;
            Title
                = title;
        }
    }

    /// <see cref="ProgressModel"/>
    #region

    public record class ProgressModel : Model
    {
        public bool IsIndeterminate { get => Get(false); set => Set(value); }

        public double Progress { get => Get(0.0); set => Set(value); }

        public string Text { get => Get(""); set => Set(value); }
    }

    #endregion

    /// <see cref="MessageModel"/>
    #region

    public record class MessageModel : Model
    {
        public string Caption { get => Get<string>(null); private set => Set(value); }

        public object Content { get => Get<object>(); private set => Set(value); }

        public Uri Image { get => Get<Uri>(); private set => Set(value); }

        public Accessor<bool> NeverShow { get => Get<Accessor<bool>>(); private set => Set(value); }

        public MessageModel(string caption, object content, Uri image, Accessor<bool> neverShow) : base()
        {
            Caption = caption; Content = content; Image = image; NeverShow = neverShow;
        }
    }

    #endregion

    #endregion

    /// <see cref="Region.Field"/>
    #region

    public static readonly Controls.ResourceKey AboutTemplateKey = new();

    public static readonly Controls.ResourceKey ColorTemplateKey = new();

    public static readonly Controls.ResourceKey DownloadTemplateKey = new();

    public static readonly Controls.ResourceKey ObjectTemplateKey = new();

    public static readonly Controls.ResourceKey ProgressTemplateKey = new();

    public static readonly Controls.ResourceKey ResultTemplateKey = new();

    #endregion

    /// <see cref="Region.Property"/>

    private static Window MainWindow => Application.Current.MainWindow;

    /// <see cref="Region.Method"/>

    public static Result Export<T>(this IListWritable<T> writer, IEnumerable<T> items, string title = nameof(Export))
    {
        Result result = default;
        if (StorageDialog.Show(out string path, title, StorageDialogMode.SaveFile, [writer.FileExtension], writer.FilePath))
        {
            result = writer.Serialize(path, items);
            if (!result)
                ShowResult(title, result);
        }
        return result;
    }

    public static void Import<T>(this IListWritable<T> writer, string title = nameof(Import))
    {
        if (StorageDialog.Show(out string[] paths, title, StorageDialogMode.OpenFile, [writer.FileExtension], writer.FilePath))
        {
            if (paths?.Length > 0)
            {
                var errors = new List<Error>();

                foreach (var i in paths)
                {
                    var result = writer.Deserialize(i, out object j);
                    if (result is Error error)
                    {
                        errors.Add(error);
                        continue;
                    }

                    if (j is IEnumerable<T> list)
                        list.ForEach(k => writer.Add(k));

                    else if (j is T item)
                        writer.Add(item);
                }

                if (errors.Any())
                    ShowResult(title, new Error(errors));
            }
        }
    }

    ///

    /// <see cref="CloseDialog"/>
    #region

    public static void CloseDialog(Window parent, int result = 0) => parent.GetChild(XWindow.DisplayDialog).Close(result);

    #endregion

    /// <see cref="Show"/>
    #region

    public static void Show(string title, object content, Images image, params ButtonModel[] buttons)
        => Show(title, content, image, null, null, buttons);

    public static void Show(string title, object content, Images image, Action<int> result, params ButtonModel[] buttons)
        => Show(title, content, image, null, result, buttons);

    public static void Show(string title, object content, Images image, Accessor<bool> neverShow, Action<int> result, params ButtonModel[] buttons)
    {
        var dialog = new DialogModel(title, new MessageModel(null, content, Resource.GetImageUri(image, ImageSize.Large), neverShow), ResultTemplateKey, image, buttons) { OnClosed = result };
        ShowDialog(dialog);
    }

    #endregion

    /// <see cref="ShowDialog"/>
    #region

    public static void ShowDialog(DialogModel dialog)
    {
        Appp.Get<AppData>().IfNotNull(i =>
        {
            dialog.Background
                = i.DialogBackground;

            dialog.MaxHeight
                = i.DialogMaximumHeight;
            dialog.MaxWidth
                = i.DialogMaximumWidth;
            dialog.MinWidth
                = i.DialogMinimumWidth;
        });

        var presenter = MainWindow.GetChild(XWindow.DisplayDialog);
        presenter?.Show(dialog);
    }

    #endregion

    ///

    /// <see cref="ShowAbout"/>
    #region

    public static void ShowAbout(params ButtonModel[] buttons)
    {
        var dialog = new DialogModel("About", null, AboutTemplateKey, Images.Info, buttons ?? Buttons.Done);
        ShowDialog(dialog);
    }

    #endregion

    /// <see cref="ShowColor"/>
    #region

    public static void ShowColor(string title, Color oldColor, out Color newColor, Action<int> result)
    {
        var content = new ColorModel(oldColor);

        var dialog = new DialogModel(title ?? "Color", content, ColorTemplateKey, Images.Color, Buttons.SaveCancel)
        {
            MaxHeight = 540,
            MaxWidth = 900,
            MinWidth = 720,
            OnClosed = result
        };
        ShowDialog(dialog);

        content.OnClosed();
        newColor = content.Document?.NewColor ?? oldColor;
    }

    #endregion

    /// <see cref="ShowDownload"/>
    #region

    public static async Task<int> ShowDownload(string title, Action<DownloadModel> action, TimeSpan delay = default, bool isIndeterminate = true)
    {
        var model = new DownloadModel(); //{ IsIndeterminate = isIndeterminate }

        var dialog = new DialogModel(title ?? "Download", model, DownloadTemplateKey, Images.Download, Buttons.Cancel);
        ShowDialog(dialog);

        if (delay > TimeSpan.Zero)
            await delay.TotalSeconds.Sleep();

        await Task.Run(() => action?.Invoke(model));
        CloseDialog(MainWindow);

        return dialog.Result;
    }

    #endregion

    /// <see cref="ShowResult"/>
    #region

    public static void ShowResult(string title, Result result, params ButtonModel[] buttons)
        => ShowResult(title, result, null, buttons);

    public static void ShowResult(string title, Result result, Action<int> onClosed, params ButtonModel[] buttons)
        => ShowResult(title, result, onClosed, null, buttons);

    public static void ShowResult(string title, Result result, Action<int> onClosed, Accessor<bool> neverShow, params ButtonModel[] buttons)
    {
        var image = result.Type switch
        {
            ResultType.Error   => Images.Error,
            ResultType.Message => Images.Message,
            ResultType.Success => Images.Success,
            ResultType.Warning => Images.Warning,
        };
        Show(title, result, image, neverShow, onClosed, buttons);
    }

    #endregion

    /// <see cref="ShowObject"/>
    #region

    public static void ShowObject<T>(string title, T source, object image, params ButtonModel[] buttons)
        => ShowObject(title, source, image, null, buttons);

    public static void ShowObject<T>(string title, T source, object image, Action<int> result, params ButtonModel[] buttons)
    {
        var dialog = new DialogModel(title ?? "Edit", source, ObjectTemplateKey, image, buttons) { OnClosed = result };
        ShowDialog(dialog);
    }

    #endregion

    /// <see cref="ShowPanel"/>
    #region

    public static void ShowPanel<T>(T panel, Action<int> result = null, params ButtonModel[] buttons) where T : Core.Panel
    {
        var dialog = new DialogModel(panel.Title, panel, DockControl.PanelBodyTemplateKey, ValueConverter.Cache.Get<ConvertAttributeImage>().Convert(panel), result, buttons);
        ShowDialog(dialog);
    }

    #endregion

    /// <see cref="ShowProgress"/>
    #region

    public static async Task<Result> ShowProgress(string title, Action<ProgressModel> action, TimeSpan delay = default, bool isIndeterminate = true)
        => await ShowProgress(title, null, action, delay, isIndeterminate);

    public static async Task<Result> ShowProgress(string title, string text, Action<ProgressModel> action, TimeSpan delay = default, bool isIndeterminate = true)
    {
        var model = new ProgressModel() { IsIndeterminate = isIndeterminate, Text = text };

        var dialog = new DialogModel(title ?? "Progress", model, ProgressTemplateKey, Images.HourGlass, Buttons.Cancel);
        ShowDialog(dialog);

        if (delay > TimeSpan.Zero)
            await delay.TotalSeconds.Sleep();

        Result result = new Success();
        await Task.Run(()
            => Try.Do(()
                => action?.Invoke(model), e => result = e));

        CloseDialog(MainWindow);
        return result;
    }

    #endregion
}