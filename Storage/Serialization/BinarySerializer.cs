using Ion.Analysis;
using Ion.Reflect;
using System;
using System.IO;

namespace Ion.Storage;

public static class BinarySerializer
{
    public static Result Deserialize<T>(string filePath, out T data, Action<Error> log = null, bool box = false)
    {
        data = default;

        Result result
            = null;
        FileStream stream
            = null;

        try
        {
            if (File.Exists(filePath))
            {
                stream = new FileStream(filePath, FileMode.Open);
                object i = null; //new BinaryFormatter().Deserialize(stream);

                if (box)
                {
                    if (i is Instance.Data j)
                    {
                        data = typeof(T).Create<T>();
                        Instance.Devirtualize(data, j);
                    }
                }
                else data = i is T j ? j : default;
                result = new Success();
            }
        }
        catch (Exception e)
        {
            result = new FileNotDeserialized(filePath, e.InnerException);

            Log.Write(result);
            log?.Invoke(result as Error);
        }
        finally
        {
            stream?.Close();
        }
        return result;
    }

    public static Result Serialize<T>(string filePath, T oldData, Action<Error> log = null, bool box = false)
    {
        object newData = oldData;

        Result result
            = null;
        FileStream stream
            = null;

        try
        {
            var folderPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folderPath))
                _ = Directory.CreateDirectory(folderPath);

            stream = new FileStream(filePath, FileMode.Create);

            if (box)
            {
                newData = new Instance.Data(oldData.GetType());
                Instance.Virtualize(oldData, newData as Instance.Data);
            }

            //var formatter = new BinaryFormatter();
            //formatter.Serialize(stream, newData);

            result = new Success();
        }
        catch (Exception e)
        {
            result = new FileNotSerialized(filePath, e);

            Log.Write(result);
            log?.Invoke(result as Error);
        }
        finally
        {
            stream?.Close();
        }
        return result;
    }
}