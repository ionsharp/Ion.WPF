using Ion.Controls;
using Ion.Imaging;
using Ion.Input;
using Ion.Numeral;
using Ion.Reflect;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Ion.Core;

public abstract record class AppToolViewModel() : ViewModelWithTitle(), IAppViewModel
{
    public virtual IEnumerable<ButtonModel> HeaderButtons
    {
        get
        {
            yield return new ButtonModel()
            {
                Color = Brushes.Gold,
                Command = AppTool.Current.Model.HelpCommand,
                Image = XImageSource.Convert(Resource.GetImageUri(Images.Help)),
                Tip = "Help"
            };
            yield return new ButtonModel()
            {
                Color = new(XColor.Convert(new ByteVector4("FFAA44"))),
                Command = XWindow.MinimizeCommand,
                CommandTarget = View,
                Image = XImageSource.Convert(Resource.GetImageUri(Images.Minimize)),
                Tip = "Minimize"
            };
            yield return new ButtonModel()
            {
                Color = new(XColor.Convert(new ByteVector4("44AA44"))),
                Command = XWindow.MaximizeCommand,
                CommandTarget = View,
                Image = XImageSource.Convert(Resource.GetImageUri(Images.Maximize)),
                Tip = "Maximize"
            };
            yield return new ButtonModel()
            {
                Color = new(XColor.Convert(new ByteVector4("007ACC"))),
                Command = XWindow.RestoreCommand,
                CommandTarget = View,
                Image = XImageSource.Convert(Resource.GetImageUri(Images.Restore)),
                Tip = "Restore"
            };
            yield return new ButtonModel()
            {
                Color = new(XColor.Convert(new ByteVector4("CC3344"))),
                Command = XWindow.CloseCommand,
                CommandTarget = View,
                Image = XImageSource.Convert(Resource.GetImageUri(Images.X)),
                Tip = "Close"
            };
        }
    }

    public IAppView View { get; set; }
}