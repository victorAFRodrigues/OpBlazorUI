using System;

namespace OpBlazorUI.Showcase.Components.Layout;

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
