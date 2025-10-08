using System;

namespace Ion.Storage;

[Serializable]
public enum CopyStatus
{
    Active,
    Inactive,
    Monitoring
}