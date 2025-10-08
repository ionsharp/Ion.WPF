using Ion.Collect;
using Ion.Text;

namespace Ion.Core;

/// <inheritdoc/>
public record class FileDockViewModelData() : DockViewModelData()
{
    public Encoding Encoding { get => Get(Encoding.ASCII); set => Set(value); }

    public ListObservable<string> RecentFiles { get => Get(new ListObservable<string>()); set => Set(value); }
}