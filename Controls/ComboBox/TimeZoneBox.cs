using Ion.Collect;
using System;
using System.Windows.Controls;

namespace Ion.Controls;

public class TimeZoneBox : ComboBox
{
    public TimeZoneBox() : base()
        => SetCurrentValue(ItemsSourceProperty,
            new ListObservable<TimeZoneInfo>(TimeZoneInfo.GetSystemTimeZones()));
}