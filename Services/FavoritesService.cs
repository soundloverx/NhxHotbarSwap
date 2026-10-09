using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace NhxHotbarSwap
{
    // Favorites let the player protect (lock) an item type, or a slot: whatever sits in that grid position. Item type
    // favorites follow the items on their own, but slot favorites are positions, so after a swap they would protect
    // whatever item moved into the slot instead of the one the player locked. That item could then be put into a chest
    // by the vanilla "Place stacks" button or NhxSimpleDeposit's quick stack, which only skip favorites. To keep every
    // item exactly as protected as before, the slot favorites of the two rows are swapped along with the items.
    //
    // The favorites are AzuExtendedPlayerInventory's when it's installed. Without it, NhxSimpleDeposit has its own copy
    // of AzuEPI's favorites, built the same way where it matters here. Neither has a public API for this, so they're
    // reached through reflection, which also keeps both optional. Only one of them is ever used: with AzuEPI installed,
    // NhxSimpleDeposit's copy stays unused, and loading it here would write a stale copy over AzuEPI's file (they share it).
    //
    // Both write their favorites file on every change; here it's written when the game saves the character instead
    // (see PlayerProfilePatch), so a key press costs no file write, and the favorites are saved together with the item
    // positions they belong to (a crash before the next save loses both, not just one, unless a favorite was changed in
    // between: that writes the whole file, swapped slots included).
    internal static class FavoritesService
    {
        internal const string AzuEpiGuid = "Azumatt.AzuExtendedPlayerInventory";
        internal const string NhxSimpleDepositGuid = "NhxSimpleDeposit";

        // AzuEPI: static UserConfig UserConfig.GetPlayerConfig(long playerID)
        // NhxSimpleDeposit: static PlayerFavorites PlayerFavorites.Get(long playerId)
        private static MethodInfo _getPlayerFavoritesMethod;

        // HashSet<Vector2i> _favoritedSlots, in both
        private static FieldInfo _favoritedSlotsField;

        // void Save(), in both
        private static MethodInfo _saveMethod;

        private static string _sourceName;
        private static bool _initialized;
        private static bool _available;

        // The favorites whose slots were swapped since the character was last saved
        private static object _unsavedFavorites;

        internal static void SwapFavoritedSlotRows(Player player, int width)
        {
            EnsureInitialized();

            if (!_available || player == null)
            {
                return;
            }

            object favorites;
            HashSet<Vector2i> favoritedSlots;

            try
            {
                // Both keep one instance per player ID, so this hands back the live favorites, not a copy
                favorites = _getPlayerFavoritesMethod.Invoke(null, new object[] { player.GetPlayerID() });
                favoritedSlots = _favoritedSlotsField.GetValue(favorites) as HashSet<Vector2i>;
            }
            catch (Exception exception)
            {
                MarkUnavailable($"reading its favorited slots failed: {exception}");
                return;
            }

            if (favoritedSlots == null)
            {
                MarkUnavailable("its favorited slots aren't stored the way this mod expects (it may have changed in an update)");
                return;
            }

            bool moved = false;

            for (int x = 0; x < width; x++)
            {
                Vector2i hotbarSlot = new Vector2i(x, 0);
                Vector2i secondRowSlot = new Vector2i(x, 1);

                bool hotbarFavorited = favoritedSlots.Remove(hotbarSlot);
                bool secondRowFavorited = favoritedSlots.Remove(secondRowSlot);

                if (hotbarFavorited)
                {
                    favoritedSlots.Add(secondRowSlot);
                }

                if (secondRowFavorited)
                {
                    favoritedSlots.Add(hotbarSlot);
                }

                moved |= hotbarFavorited != secondRowFavorited;
            }

            if (moved)
            {
                _unsavedFavorites = favorites;
            }
        }

        // Called right after the game saved the character to disk
        internal static void SaveSwappedFavorites()
        {
            if (_unsavedFavorites == null)
            {
                return;
            }

            try
            {
                _saveMethod.Invoke(_unsavedFavorites, null);
                _unsavedFavorites = null;
            }
            catch (Exception exception)
            {
                // Still swapped in memory, and tried again with the next character save
                Plugin.Log.LogWarning($"Couldn't save {_sourceName}'s favorites after swapping the hotbar: {exception}");
            }
        }

        private static void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            // AzuEPI first: NhxSimpleDeposit only uses its own favorites when AzuEPI isn't installed
            if (TryGetPlugin(AzuEpiGuid, out PluginInfo pluginInfo))
            {
                Initialize(pluginInfo, "AzuExtendedPlayerInventory", "AzuEPI.Game.Favoriting.UserConfig", "GetPlayerConfig");
            }
            else if (TryGetPlugin(NhxSimpleDepositGuid, out pluginInfo))
            {
                Initialize(pluginInfo, "NhxSimpleDeposit", "NhxSimpleDeposit.PlayerFavorites", "Get");
            }
            else
            {
                Plugin.Log.LogInfo("Neither AzuExtendedPlayerInventory nor NhxSimpleDeposit found: there are no favorited slots to swap.");
            }
        }

        private static bool TryGetPlugin(string guid, out PluginInfo pluginInfo)
        {
            return Chainloader.PluginInfos.TryGetValue(guid, out pluginInfo) && pluginInfo.Instance != null;
        }

        private static void Initialize(PluginInfo pluginInfo, string sourceName, string typeName, string getterName)
        {
            _sourceName = sourceName;

            Type favoritesType = pluginInfo.Instance.GetType().Assembly.GetType(typeName);

            if (favoritesType != null)
            {
                _getPlayerFavoritesMethod = AccessTools.Method(favoritesType, getterName, new[] { typeof(long) });
                _favoritedSlotsField = AccessTools.Field(favoritesType, "_favoritedSlots");
                _saveMethod = AccessTools.Method(favoritesType, "Save", Type.EmptyTypes);
            }

            if (_getPlayerFavoritesMethod == null || _favoritedSlotsField == null || _saveMethod == null)
            {
                MarkUnavailable("its favorited slots weren't found (it may have changed in an update)");
                return;
            }

            _available = true;
            Plugin.Log.LogInfo($"Using {sourceName} {pluginInfo.Metadata.Version} favorites: favorited slots in the hotbar and second row are swapped along with their items.");
        }

        private static void MarkUnavailable(string reason)
        {
            _available = false;
            Plugin.Log.LogWarning($"{_sourceName} is installed but {reason}. The hotbar still swaps, but its favorited slots stay where they are instead of following their items.");
        }
    }
}
