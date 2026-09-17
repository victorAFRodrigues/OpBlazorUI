namespace OpBlazorUI.Showcase.Components.Layout;

public static class DarkModeState
{
    public static bool Dark { get; private set; }

    public static event Action? Changed;

    public static void Set(bool dark)
    {
        if (Dark == dark) return;
        Dark = dark;
        Changed?.Invoke();
    }
}

public static class LayoutState
{
    public static bool SidebarOpen { get; private set; }

    public static event Action? SidebarToggled;

    public static void ToggleSidebar()
    {
        SidebarOpen = !SidebarOpen;
        SidebarToggled?.Invoke();
    }

    public static void CloseSidebar()
    {
        if (!SidebarOpen) return;
        SidebarOpen = false;
        SidebarToggled?.Invoke();
    }
}
