using HarmonyLib;

namespace NhxHotbarSwap
{
    // Writes the favorites (AzuEPI's or NhxSimpleDeposit's) after a hotbar swap moved some of them, at the moment the
    // character itself is written to disk with the swapped item positions (see FavoritesService). PlayerProfile.Save() is what the game's
    // regular autosave, logout and quit all go through.
    [HarmonyPatch(typeof(PlayerProfile), nameof(PlayerProfile.Save))]
    internal static class PlayerProfilePatch
    {
        private static void Postfix(bool __result)
        {
            if (__result)
            {
                FavoritesService.SaveSwappedFavorites();
            }
        }
    }
}
