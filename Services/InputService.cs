using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace NhxHotbarSwap
{
    internal static class InputService
    {
        private static readonly KeyCode[] StrictModifierKeys =
        {
            KeyCode.LeftControl, KeyCode.RightControl,
            KeyCode.LeftAlt, KeyCode.RightAlt,
            KeyCode.LeftCommand, KeyCode.RightCommand,
            KeyCode.LeftWindows, KeyCode.RightWindows,
            KeyCode.AltGr
        };

        // BepInEx's KeyboardShortcut.IsDown() fails whenever any key outside the shortcut is held,
        // so it never fires while the player is moving/running (WASD, Shift). This ignores ordinary
        // keys, but still rejects extra Ctrl/Alt/Windows modifiers so e.g. a bare "`" doesn't also
        // fire on another mod's Ctrl+`. Shift is deliberately not rejected since it's the run key.
        // The keys are read through ZInput, the game's own input wrapper, like the game itself does.
        internal static bool IsDown(KeyboardShortcut shortcut)
        {
            if (shortcut.MainKey == KeyCode.None
                || !ZInput.GetKeyDown(shortcut.MainKey, false)
                || !shortcut.Modifiers.All(key => ZInput.GetKey(key, false)))
            {
                return false;
            }

            return !StrictModifierKeys.Any(key => key != shortcut.MainKey
                && !shortcut.Modifiers.Contains(key)
                && ZInput.GetKey(key, false));
        }
    }
}
