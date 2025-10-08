using System;
using System.Management;

namespace Ion.Storage;

public sealed class RemovableDriveEventArgs(string path) : EventArgs
{
    public readonly string Name = path;
}

public delegate void RemovableDriveEventHandler(RemovableDriveEventArgs e);

public class RemovableDrive
{
    public static event RemovableDriveEventHandler Inserted;

    public static event RemovableDriveEventHandler Removed;

    public enum EventType
    {
        Inserted = 2,
        Removed = 3
    }

    static RemovableDrive()
    {
        ManagementEventWatcher watcher = new();
        WqlEventQuery query = new("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2 or EventType = 3");

        watcher.EventArrived += (s, e) =>
        {
            string driveName = e.NewEvent.Properties["DriveName"].Value.ToString();
            EventType eventType = (EventType)(System.Convert.ToInt16(e.NewEvent.Properties["EventType"].Value));

            string eventName = Enum.GetName(typeof(EventType), eventType);

            if (eventType == EventType.Inserted)
                Inserted?.Invoke(new(driveName));

            if (eventType == EventType.Removed)
                Removed?.Invoke(new(driveName));
        };

        watcher.Query = query;
        watcher.Start();
    }
}