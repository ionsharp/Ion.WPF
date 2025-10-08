using Ion.Controls;
using Ion.Core;
using Ion.Imaging;
using Ion.Input;
using Ion.Numeral;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace Ion;

public record class DialogModel : Model, IWindow
{
    /// <see cref="Region.Property.Internal"/>

    internal Action<int> OnClosed { get; set; }

    /// <see cref="Region.Property.Public"/>
    #region

    public ByteVector4 Background { get; set; }

    public object Content { get; set; }

    public object ContentTemplate { get; set; }

    public Uri Image { get; internal set; }

    public MSize<double> ImageSize { get; private set; } = new(64);

    public Uri LargeImage { get; internal set; }

    public double MaxHeight { get; set; } = 720;

    public double MaxWidth { get; set; } = 900;

    public double MinWidth { get; set; } = 360;

    public int Result { get; set; }

    public string Title { get; private set; }

    #endregion

    /// <see cref="Region.Constructor"/>
    #region

    public DialogModel(string title, object content, object contentTemplate, object image, Action<int> result, ButtonModel[] buttons) : this(title, content, contentTemplate, image, buttons) => OnClosed = result;

    public DialogModel(string title, object content, object contentTemplate, object image, ButtonModel[] buttons)
    {
        Title = title; Content = content; ContentTemplate = contentTemplate;

        ButtonList t = [];

        if (buttons is null || buttons.Length == 0)
            buttons = Buttons.Done;

        buttons.ForEach(button =>
        {
            button.Command = CloseCommand;
            t.Add(button);
        });

        FooterButtons = t;

        Throw.If<NotSupportedException>(image is not string && image is not Images && image is not Uri);

        Image = image as Uri;
        image
            .If<Images>(i =>
            {
                Image
                    = Resource.GetImageUri(i, Ion.ImageSize.Smallest);
                LargeImage
                    = Resource.GetImageUri(i, Ion.ImageSize.Large);
            });
    }

    #endregion

    /// <see cref="ICommand"/>

    public ICommand CloseCommand => Commands[nameof(CloseCommand)] ??= new RelayCommand(()
        => Appp.Model.View.To<Window>().GetChildOfType<DisplayDialog>().IfNotNull(j => j.Close(0)));

    /// <see cref="IWindow"/>
    #region

    public IEnumerable<ButtonModel> FooterButtons { get; private set; }

    public IEnumerable<ButtonModel> HeaderButtons
    {
        get
        {
            yield return new ButtonModel()
            {
                Color = new(XColor.Convert(new ByteVector4("CC3344"))),
                Command = CloseCommand,
                Image = XImageSource.Convert(Resource.GetImageUri(Images.X)),
                Tip = "Close"
            };
        }
    }

    #endregion
}