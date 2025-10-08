using System;

namespace Ion.Storage;

public class ItemNotFoundException(string itemPath) : Exception($"The file or folder '{itemPath}' does not exist.")
{
}

public class ItemsNotFoundException(string folderPath) : Exception($"The folder '{folderPath}' does not contain anything.")
{
}