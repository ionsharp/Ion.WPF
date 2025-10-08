using Ion.Controls;
using Ion.Core;

namespace Ion.Data;

public abstract class MultiBindLanguage : MultiBind
{
    protected MultiBindLanguage() : this(Paths.Dot) { }

    protected MultiBindLanguage(string path) : base(path)
        => Bindings.Add(new Bind(nameof(AppData.Language)) { AppSource = AppSource.Data });
}