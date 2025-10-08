using Ion.Analysis;
using Ion.Collect;
using Ion.Colors;
using Ion.Controls;
using Ion.Media;
using Ion.Numeral;
using Ion.Storage;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Media;

namespace Ion.Controls;

[Extend<ResourceDictionary>]
public static class XResourceDictionary
{
    public static ObjectDictionary Convert(this ResourceDictionary input)
    {
        var result = new ObjectDictionary();
        foreach (DictionaryEntry i in input)
        {
            if (i.Key is string key)
            {
                if (i.Value is SolidColorBrush solidColorBrush)
                    result.Add(key, new ByteVector4(solidColorBrush.Color.R, solidColorBrush.Color.G, solidColorBrush.Color.B, solidColorBrush.Color.A));

                else if (i.Value is LinearGradientBrush bLinear)
                {
                    bLinear.Convert(out Gradient gLinear);
                    result.Add(key, gLinear);
                }

                else if (i.Value is RadialGradientBrush bRadial)
                {
                    bRadial.Convert(out Gradient gRadial);
                    result.Add(key, gRadial);
                }
            }
        }
        return result;
    }

    public static ResourceDictionary Convert(this ObjectDictionary input)
    {
        var result = new ResourceDictionary();
        foreach (var i in input)
        {
            if (i.Value is ByteVector4 color)
                result.Add(i.Key, new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B)));

            else if (i.Value is Gradient gLinear)
            {
                gLinear.Convert(out LinearGradientBrush bLinear);
                result.Add(i.Key, bLinear);
            }

            //We'll have to come back to this!
            else if (i.Value is Gradient gRadial)
            {
                gRadial.Convert(out RadialGradientBrush bRadial);
                result.Add(i.Key, bRadial);
            }
        }
        return result;
    }

    public static Result TryDeserialize(string filePath, out ResourceDictionary result)
    {
        result = null;
        try
        {
            BinarySerializer.Deserialize(filePath, out ObjectDictionary finalResult);
            result = finalResult.Convert();
            return true;
        }
        catch (Exception e)
        {
            Log.Write(e);
            return e;
        }
    }

    public static Result TrySerialize(this ResourceDictionary input, string filePath)
    {
        try
        {
            var result = input.Convert();
            BinarySerializer.Serialize(filePath, result);
            Log.Write(new Success($"Saved theme '{filePath}'!"));
            return true;
        }
        catch (Exception e)
        {
            Log.Write(e);
            return e;
        }
    }
}