using Ion.Analysis;
using Ion.Threading;
using Ion.Time;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ion.Controls;

public class DownloadControl : Control
{
    public static readonly ResourceKey ProgressBarStyleKey = new();

    public static readonly ResourceKey TextBlockStyleKey = new();

    private class Data(string source, string destination)
    {
        public readonly string Source
                = source;

        public readonly string Destination
                = destination;
    }

    private readonly Taskable<Data> downloadTask;
    private readonly Stopwatch stopwatch = new();

    ///

    public event DownloadControlEventHandler Downloaded;

    ///

    public static readonly DependencyProperty AutoStartProperty = DependencyProperty.Register(nameof(AutoStart), typeof(bool), typeof(DownloadControl), new FrameworkPropertyMetadata(false, OnAutoStartChanged));
    public bool AutoStart
    {
        get => (bool)GetValue(AutoStartProperty);
        set => SetValue(AutoStartProperty, value);
    }

    private static void OnAutoStartChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => sender.As<DownloadControl>().OnAutoStartChanged(e.Convert<bool>());

    public static readonly DependencyProperty DestinationProperty = DependencyProperty.Register(nameof(Destination), typeof(string), typeof(DownloadControl), new FrameworkPropertyMetadata(string.Empty, OnDestinationChanged));
    public string Destination
    {
        get => (string)GetValue(DestinationProperty);
        set => SetValue(DestinationProperty, value);
    }

    private static void OnDestinationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => sender.As<DownloadControl>().OnDestinationChanged(e.Convert<string>());

    private static readonly DependencyPropertyKey MessageKey = DependencyProperty.RegisterReadOnly(nameof(Message), typeof(object), typeof(DownloadControl), new FrameworkPropertyMetadata(null));
    public static readonly DependencyProperty MessageProperty = MessageKey.DependencyProperty;
    public object Message
    {
        get => (object)GetValue(MessageProperty);
        private set => SetValue(MessageKey, value);
    }

    public static readonly DependencyProperty MessageTemplateProperty = DependencyProperty.Register(nameof(MessageTemplate), typeof(DataTemplate), typeof(DownloadControl), new FrameworkPropertyMetadata(null));
    public DataTemplate MessageTemplate
    {
        get => (DataTemplate)GetValue(MessageTemplateProperty);
        set => SetValue(MessageTemplateProperty, value);
    }

    public static readonly DependencyProperty MessageTemplateSelectorProperty = DependencyProperty.Register(nameof(MessageTemplateSelector), typeof(DataTemplateSelector), typeof(DownloadControl), new FrameworkPropertyMetadata(null));
    public DataTemplateSelector MessageTemplateSelector
    {
        get => (DataTemplateSelector)GetValue(MessageTemplateSelectorProperty);
        set => SetValue(MessageTemplateSelectorProperty, value);
    }

    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(string), typeof(DownloadControl), new FrameworkPropertyMetadata(string.Empty, OnSourceChanged));
    public string Source
    {
        get => (string)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    private static void OnSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => sender.As<DownloadControl>().OnSourceChanged(e.Convert<string>());

    private static readonly DependencyPropertyKey ProgressKey = DependencyProperty.RegisterReadOnly(nameof(Progress), typeof(double), typeof(DownloadControl), new FrameworkPropertyMetadata(0.0));
    public static readonly DependencyProperty ProgressProperty = ProgressKey.DependencyProperty;
    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        private set => SetValue(ProgressKey, value);
    }

    private static readonly DependencyPropertyKey SpeedKey = DependencyProperty.RegisterReadOnly(nameof(Speed), typeof(double), typeof(DownloadControl), new FrameworkPropertyMetadata(0.0));
    public static readonly DependencyProperty SpeedProperty = SpeedKey.DependencyProperty;
    public double Speed
    {
        get => (double)GetValue(SpeedProperty);
        private set => SetValue(SpeedKey, value);
    }

    private static readonly DependencyPropertyKey ProcessedKey = DependencyProperty.RegisterReadOnly(nameof(Processed), typeof(string), typeof(DownloadControl), new FrameworkPropertyMetadata("0 MB / 0 MB"));
    public static readonly DependencyProperty ProcessedProperty = ProcessedKey.DependencyProperty;
    public string Processed
    {
        get => (string)GetValue(ProcessedProperty);
        private set => SetValue(ProcessedKey, value);
    }

    private static readonly DependencyPropertyKey RemainingKey = DependencyProperty.RegisterReadOnly(nameof(Remaining), typeof(long), typeof(DownloadControl), new FrameworkPropertyMetadata(0L));
    public static readonly DependencyProperty RemainingProperty = RemainingKey.DependencyProperty;
    public long Remaining
    {
        get => (long)GetValue(RemainingProperty);
        private set => SetValue(RemainingKey, value);
    }

    ///

    public DownloadControl() : base()
    {
        downloadTask = new(null, DownloadAsync, TaskStrategy.Ignore);
    }

    ///

    private async Task DownloadAsync(Data data, CancellationToken token)
    {
        Result result = null;

        var watch = stopwatch;
        using (var client = new WebClient())
        {
            client.DownloadProgressChanged
                += new DownloadProgressChangedEventHandler(OnDownloadProgressChanged);

            Uri uri = data.Source.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(data.Source) : new Uri("http://" + data.Source);
            await Task.Run(new Action(() => Try.Do(() => System.IO.Directory.CreateDirectory(data.Destination), e => result = new Error($"Download failed: {e.Message}"))), token);

            if (result is not Error)
            {
                await Try.DoAwait(async () =>
                {
                    watch.Start();
                    await client.DownloadFileTaskAsync(uri, string.Concat(data.Destination, @"\", System.IO.Path.GetFileName(data.Source)));
                    watch.Reset();

                    result = new Success($"Download succeeded: '{data.Source}'");
                },
                e => result = new Error($"Download failed: {e.Message}"));
            }
        }
        Log.Write(result);
        OnDownloaded(result);
    }

    ///

    private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        Speed
            = Math.Round(e.BytesReceived / 1024d / stopwatch.Elapsed.TotalSeconds, 3);
        Progress
            = System.Convert.ToDouble(e.ProgressPercentage);
        Processed
            = $"{e.BytesReceived / 1024d / 1024d:0.00} MB / {e.TotalBytesToReceive / 1024d / 1024d:0.00} MB";
        Remaining
            = Convert.ToInt64(stopwatch.Elapsed.ToLeft(e.TotalBytesToReceive, e.BytesReceived).TotalSeconds);
    }

    ///

    protected virtual void OnAutoStartChanged(ValueChange<bool> input)
    {
        if (input.NewValue)
        {
            if (!downloadTask.IsStarted)
                Start();
        }
    }

    protected virtual void OnDestinationChanged(ValueChange<string> input)
    {
        if (AutoStart) Start();
    }

    protected virtual void OnDownloaded(Result input) => Downloaded?.Invoke(this, new(input));

    protected virtual void OnSourceChanged(ValueChange<string> input)
    {
        if (AutoStart) Start();
    }

    ///

    public void Start() => _ = downloadTask.Start(new(Source, Destination));
}