using System;

namespace Features.UI
{
    public static class UIStateEvents
    {
        public static event Action<bool> OnGameMenuToggled;

        public static void ToggleGameMenu(bool isVisible)
        {
            OnGameMenuToggled?.Invoke(isVisible);
        }
    }
}
