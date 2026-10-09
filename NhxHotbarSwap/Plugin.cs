using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NhxHotbarSwap
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    // Optional: when installed, their favorited slots in the two rows are swapped along with the items (see FavoritesService).
    // NhxSimpleDeposit's favorites are its own copy of AzuEPI's, only used without AzuEPI.
    [BepInDependency(FavoritesService.AzuEpiGuid, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(FavoritesService.NhxSimpleDepositGuid, BepInDependency.DependencyFlags.SoftDependency)]
    // NhxQualityPack contains this same swap on the same key, so with both installed one key press would swap twice and undo itself
    [BepInIncompatibility("NhxQualityPack")]
    // HotbarSwitch does the same job by changing which row the number keys use, so combined the two would undo each other
    [BepInIncompatibility("cjayride.HotbarSwitch")]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "NhxHotbarSwap";
        private const string PluginName = "NhxHotbarSwap";
        private const string PluginVersion = "1.0.0";

        internal static Plugin Instance { get; private set; }
        internal static ManualLogSource Log { get; private set; }

        private ConfigEntry<KeyboardShortcut> _hotbarSwapShortcut;
        private Harmony _harmonyInstance;

        internal static KeyboardShortcut HotbarSwapShortcut => Instance?._hotbarSwapShortcut != null ? Instance._hotbarSwapShortcut.Value : new KeyboardShortcut(KeyCode.BackQuote);

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            _hotbarSwapShortcut = Config.Bind(
                "General",
                "Hotbar Swap Shortcut",
                new KeyboardShortcut(KeyCode.BackQuote),
                "Keyboard shortcut used to swap the hotbar (first inventory row) with the second inventory row."
            );

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmonyInstance = new Harmony(PluginGuid);
            _harmonyInstance.PatchAll(assembly);

            Log.LogInfo($"{PluginName} v{PluginVersion} loaded.");
        }

        private void Update()
        {
            HandleHotbarSwap();
        }

        private static void HandleHotbarSwap()
        {
            if (!InputService.IsDown(HotbarSwapShortcut) || IsTypingInInputField())
            {
                return;
            }

            Player player = Player.m_localPlayer;

            if (player == null)
            {
                return;
            }

            if (HotbarSwapService.SwapHotbarRows(player))
            {
                player.Message(MessageHud.MessageType.Center, "Hotbar swapped");
            }
        }

        private void OnDestroy()
        {
            _harmonyInstance?.UnpatchSelf();

            Log?.LogInfo($"{PluginName} v{PluginVersion} unloaded.");

            Instance = null;
        }

        private static bool IsTypingInInputField()
        {
            if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null)
            {
                return false;
            }

            GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

            // The EventSystem keeps pointing at an input field after its panel is closed (e.g. the portal/sign
            // text dialog only deactivates its panel), so being selected isn't enough - it has to be focused.
            InputField legacyField = selectedObject.GetComponent<InputField>();
            TMP_InputField tmpField = selectedObject.GetComponent<TMP_InputField>();

            return (legacyField != null && legacyField.isFocused) || (tmpField != null && tmpField.isFocused);
        }
    }
}
