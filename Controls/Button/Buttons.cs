namespace Ion.Controls;

public static class Buttons
{
    public static readonly ButtonModel[] AbortRetryIgnore
        = [new("Abort", 0), new("Retry", 1, true), new("Ignore", 2, false, true)];

    public static readonly ButtonModel[] Cancel
        = [new("Cancel", 0, false, true)];

    public static readonly ButtonModel[] Continue
        = [new("Continue", 0, true)];

    public static readonly ButtonModel[] ContinueCancel
        = [new("Continue", 0, true), new("Cancel", 1, false, true)];

    public static readonly ButtonModel[] Done
        = [new("Done", 0)];

    public static readonly ButtonModel[] Ok
        = [new("Ok", 0, true)];

    public static readonly ButtonModel[] OkCancel
        = [new("Ok", 0, true), new("Cancel", 1, false, true)];

    public static readonly ButtonModel[] SaveCancel
        = [new("Save", 0, true), new("Cancel", 1, false, true)];

    public static readonly ButtonModel[] YesCancel
        = [new("Yes", 0, true), new("Cancel", 1, false, true)];

    public static readonly ButtonModel[] YesNo
        = [new("Yes", 0, true), new("No", 1, false, true)];

    public static readonly ButtonModel[] YesNoCancel
        = [new("Yes", 0, true), new("No", 1), new("Cancel", 2, false, true)];
}