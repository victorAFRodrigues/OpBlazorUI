namespace OpBlazorUI.Models;

public sealed class OpToastCloseEvent
{
    public OpToastMessage Message { get; }
    public int Index { get; }

    public OpToastCloseEvent(OpToastMessage message, int index)
    {
        Message = message;
        Index = index;
    }
}