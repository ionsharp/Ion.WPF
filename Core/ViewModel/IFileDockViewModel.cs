using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ion.Core;

public interface IFileDockViewModel : IDockViewModel
{
    Task Open();

    Task Open(IList<string> filePaths);

    Task Open(string filePath);
}