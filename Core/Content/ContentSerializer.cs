using Ion.Analysis;
using Ion.Controls;
using Ion.Numeral;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Ion.Core;

public static class ContentSerializer
{
    public const string InternalLayoutPath = "Layout/";

    private static readonly XmlSerializer Serializer = new
    (
        typeof(DockLayout),
        new XmlAttributeOverrides(),
        [typeof(MSize<double>), typeof(ControlLength), typeof(ControlLengthUnit), typeof(DockLayout), typeof(DockLayoutDocumentGroup), typeof(DockLayoutElement), typeof(DockLayoutGroup), typeof(DockLayoutPanel), typeof(DockLayoutPanelGroup), typeof(DockLayoutWindow), typeof(Vector2M<double>)],
        new XmlRootAttribute("Layout"),
        typeof(DockLayout).Namespace
    );

    private static Result Deserialize(string filePath)
    {
        try
        {
            var layout = (DockLayout)Serializer.Deserialize(new StringReader(Storage.File.ReadAllText(filePath, System.Text.Encoding.UTF8)));
            return new Success<DockLayout>(layout);
        }
        catch (Exception e)
        {
            Log.Write(e);
            return e;
        }
    }

    private static Result Deserialize(Stream stream)
    {
        Result result = null;

        try
        {
            var layout = (DockLayout)Serializer.Deserialize(stream);
            result = new Success<DockLayout>(layout);
        }
        catch (Exception e)
        {
            Log.Write(e);
            result = e;
        }
        finally
        {
            stream?.Close();
            stream?.Dispose();
        }

        return result;
    }

    public static async Task<Result> Deserialize(object input)
    {
        Result result = null;
        await Task.Run(() =>
        {
            //String
            if (input is string i && !i.IsEmpty())
                result = Deserialize(i);

            //Uri
            else if (input is Uri j)
                result = Deserialize(AppResources.GetStream(j));
        });
        return result;
    }

    public static async Task<Result> Deserialize(object layout, object defaultLayout)
    {
        var result = await Deserialize(layout);
        if (result is Error)
            result = await Deserialize(defaultLayout);

        return result;
    }

    public static Result Serialize(string filePath, DockLayout layout)
    {
        var directoryName = Path.GetDirectoryName(filePath);
        Result result;
        if (!Directory.Exists(directoryName))
        {
            try
            {
                Directory.CreateDirectory(directoryName);
                result = new Success();
            }
            catch (Exception e)
            {
                result = e;
                Log.Write(result);
                return result;
            }
        }

        try
        {
            Serializer.Serialize(new StreamWriter(filePath), layout);
            result = new Success();
        }
        catch (Exception e)
        {
            result = e;
            Log.Write(result);
        }
        return result;
    }
}