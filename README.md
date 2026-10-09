# NhxHotbarSwap

A private mod for personal use with friends - not published on Thunderstore.

Swap your hotbar (the first inventory row) with the second inventory row using a keyboard shortcut. This lets you keep two hotbar setups, such as tools and weapons, and switch between them without opening the inventory. Press it again to swap back.

This is the Hotbar Swap feature of **NhxQualityPack**, split out into a small mod of its own so it can be used alongside other mods, as a replacement for **HotbarSwitch**. Don't install it together with either of them: NhxHotbarSwap won't load while NhxQualityPack or HotbarSwitch is installed.

## Instructions

Press the hotbar swap shortcut (default backtick `` ` ``).

- Only the first two rows are swapped; the rest of your inventory, and AzuEPI's equipment and quick slots, are untouched
- [Favorited](#favorites) (locked) items are swapped like any other item, they're never left behind
- Equipped items stay equipped and simply move with their row
- A big `Hotbar swapped` message is shown in the middle of the screen, so an accidental key press is noticeable (like any other message, it's not shown while the HUD is hidden)
- The shortcut works while moving or running, but is ignored while typing in a text field (chat, console, signs, ...) and while Ctrl, Alt or Windows is held, unless they're part of the shortcut

### Favorites

**AzuExtendedPlayerInventory** (AzuEPI) lets you favorite items to protect them, for example from the chest's **Place stacks** button and NhxSimpleDeposit's quick stack. Without AzuEPI, **NhxSimpleDeposit** has its own copy of AzuEPI's favorites, which works the same way and is handled the same way here. With the default `Favoriting Modifier Key` (Left Alt):

- **Left Alt** + **left-click** favorites an item type. That follows the items wherever they go, so swapping changes nothing for it
- **Left Alt** + **right-click** favorites a slot, i.e. whatever sits in it. When the rows swap, the favorited slots in the two rows swap with them, so the favorite stays with the item you put there: the coloured border moves along with the item

Every item is exactly as protected after a swap as it was before. With both rows empty, for example right after respawning while your items are still in your tombstone, the shortcut does nothing: the tombstone gives every item back to its old slot, so favorites swapped on their own would end up on the wrong row. Moved slot favorites are saved when the game saves your character (autosave, logout or quit), together with the new item positions, so a swap doesn't write any file.

Without AzuEPI or NhxSimpleDeposit there's nothing to keep track of. If one of them is installed but an update changed it so that its slot favorites can't be found any more, the hotbar still swaps, but slot favorites stay where they are (the reason is in the BepInEx log).

## Differences From HotbarSwitch

HotbarSwitch never moves items: it changes which row the number keys and the hotbar use, and repaints the hotbar every frame to do that. NhxHotbarSwap really moves the items between the two rows, once per key press, and does no work at all in between.

- The number keys and the hotbar always use the first row, so other mods that look at "the hotbar" see the items you're actually using
- Only the first two rows are swapped (HotbarSwitch's `RowsToSwitch` could cycle through more)
- A message is shown when the hotbar swaps
- The shortcut is a `KeyboardShortcut` setting (pick it in the Configuration Manager) instead of a key name typed as text

## Compatibility

- **AzuExtendedPlayerInventory**: optional, see [Favorites](#favorites). Its extra inventory rows, equipment slots and quick slots are never touched
- **HotbarSwitch**: replaced by this mod, NhxHotbarSwap won't load while it's installed
- **NhxQualityPack**: not compatible (it already contains this swap, so one key press would swap twice), NhxHotbarSwap won't load while it's installed
- **NhxSimpleDeposit**: optional, see [Favorites](#favorites). Its quick stack never takes items from the first row, but the second row is a normal row to it, and the swapped-out hotbar ends up there. To keep both hotbar setups out of quick stack, favorite their item types, or favorite the slots of both rows: a slot favorite moves with its item, so favoriting only the second row's slots protects one setup, not whichever one is in the second row
- Nothing in the game's hotbar or inventory code is patched, so mods that draw on the hotbar or inventory (AzuEPI's quick slots, Jewelcrafting's socket colours, ...) keep working as usual. The only patch writes the favorites when the character is saved

## Multiplayer

- Client-side mod
- Does not have server sync
- Item positions are part of your character, so the swap is saved with it like any other inventory change

## Configuration

- Hotbar swap keyboard shortcut (default backtick, `BackQuote`)
