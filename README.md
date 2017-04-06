# Item Customizer Mod

This is a Terraria mod designed to allow players to apply the game's many "dyes"
to their weapons (and various other items). Projectiles fired by weapons are also affected.

### Weak reference functionality
As of 1.0, Item Customizer now supports some weak referencing using `Mod.Call()`.
These are:
- `Call(“GetItemShader”, Item item)`: returns an `int`, the shader ID of the given item
- `Call(“SetItemShader”, Item item, int shaderID)`: sets the item’s shader ID to the one given
- `Call(“GetProjShader”, Projectile proj)`: returns an `int`, the shader ID of the given projectile
- `Call(“SetProjShader”, Projectile proj, int shaderID)`: sets the projectile’s shader ID to the one given
- `Call("GetItemName", Item item)`: returns a `string`, the item's set custom name
- `Call("SetItemName", Item item, string name)`: sets the item's custom name to the given string
Functions that don't return anything will instead return `true` on successful completion.
Any incorrect inputs will be caught and the method will return an Exception instead.
