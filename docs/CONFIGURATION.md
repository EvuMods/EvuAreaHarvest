# Configuration

Settings are written to `BepInEx/config/evu.evuareaharvest.cfg` the first time the mod loads. Jotunn sends the server's harvest range to clients when the mod is installed on that server. Without the mod on the server, each client keeps its own range.

A configuration manager is optional. The harvest range shows an S button there. An admin can press it to send that value to the server, and the server then sends it to the other players. Editing the server's file while it is running does the same. Players who are not on the server's admin list cannot change that row while connected. The descriptions below are the same text shown in Configuration Manager.

## Server

| Setting | Default | Description |
| --- | --- | --- |
| Harvest range | 8 | Radius in meters, from 2 to 32. Harvest and the highlight use a sphere centered on you. When this mod is installed on the server, that server's value is the one used. |
| Pick guarded items | Off | When on, also picks items that aggravate nearby creatures when taken, such as Dvergr belongings. Off leaves them alone. |

## Local

These stay on the client. They have no S button.

| Setting | Default | Description |
| --- | --- | --- |
| Highlight harvestables | Off | When on, draws the range on the ground and a marker on each harvestable in range. |
| Harvest area | Numpad Delete | Key that harvests everything in range. Unity calls numpad Delete KeypadPeriod. |
| Toggle highlight | End | Key that shows or hides the range ring and the harvestable markers. |

Both keys are ignored while a menu, the map, the inventory, a store, the console, chat, or a text box is open, and while you are dead, teleporting, or in a cutscene.
