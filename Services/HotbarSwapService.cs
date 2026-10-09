using UnityEngine;

namespace NhxHotbarSwap
{
    internal static class HotbarSwapService
    {
        // Every item in the first two rows trades places with the one above/below it, favorited (locked) items
        // and equipped items included: equipped items simply stay equipped in their new slot.
        //
        // Inventory.Changed() is deliberately not called. The hotbar and the inventory grid are redrawn from the
        // items' positions every frame, so nothing needs to be told, and Changed() would make the game rebuild its
        // known recipe and build piece lists for a change that only moves items around. That's the slow part with
        // many content mods installed. The character save writes the new positions like any other change.
        internal static bool SwapHotbarRows(Player player)
        {
            if (player == null)
            {
                return false;
            }

            Inventory inventory = player.GetInventory();

            if (inventory == null || inventory.GetHeight() < 2)
            {
                return false;
            }

            bool moved = false;

            foreach (ItemDrop.ItemData item in inventory.GetAllItems())
            {
                if (item == null)
                {
                    continue;
                }

                if (item.m_gridPos.y == 0)
                {
                    item.m_gridPos = new Vector2i(item.m_gridPos.x, 1);
                    moved = true;
                }
                else if (item.m_gridPos.y == 1)
                {
                    item.m_gridPos = new Vector2i(item.m_gridPos.x, 0);
                    moved = true;
                }
            }

            // Both rows empty, e.g. after respawning while the items still lie in the tombstone: taking them back puts
            // every item in its old slot, so slot favorites swapped on their own would end up on the other row
            if (!moved)
            {
                return false;
            }

            // Done after the items, so the items still swap even if the favorites can't be reached
            FavoritesService.SwapFavoritedSlotRows(player, inventory.GetWidth());

            return true;
        }
    }
}
