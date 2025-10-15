using Ion.Analysis;
using Ion.Collect;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Ion.Core;

public interface IAppModel : IAppElement
{
    /// <see cref="Region.Property"/>
    #region

    /// <summary>The folder where data used by the app is saved.</summary>
    string DataFolderPath { get; }

    /// <summary>The folder where data shared by all apps is saved.</summary>
    string DataFolderPathShared { get; }

    AppLinkList Links { get; }

    IListWritable<Entry> Log { get; }

    IListWritable<Notification> Notifications { get; }

    IEnumerable<AppTask> DefaultStartTasks { get; }

    AppResources Theme { get; }

    Window Window { get; }

    #endregion

    /// <see cref="Region.Method"/>

    void Start(StartupEventArgs e);

    /// <see cref="IAppComponent"/>
    #region

    AppData Data { get; }

    Type DataType { get; }

    AppMenu Menu { get; }

    Type MenuType { get; }

    IAppView View { get; set; }

    IViewModel ViewModel { get; }

    Type ViewType { get; }

    #endregion
}