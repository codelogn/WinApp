using System;

public static class LinksEvents
{
    public static event EventHandler LinksChanged;

    public static void RaiseLinksChanged()
    {
        LinksChanged?.Invoke(null, EventArgs.Empty);
    }
}