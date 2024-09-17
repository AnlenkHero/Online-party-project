using System;

public static class UIEventManager
{
    // Global event for when UI interaction ends
    public static event Action OnUIInteractionEnded;

    public static void RaiseUIInteractionEnded()
    {
        OnUIInteractionEnded?.Invoke();
    }
}