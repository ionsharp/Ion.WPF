namespace Ion.Core;

public interface IAppView : IAppComponent
{
    object DataContext { get; set; }

    void Show();
}