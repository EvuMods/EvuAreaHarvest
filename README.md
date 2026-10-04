# EvuAreaHarvest

Harvest everything in an area with a simple hotkey, even things planted under the ground.

## Features

- One key harvests mushrooms, flowers, berries, crops, and the other press-E pickables inside a sphere centered on you. Buried plants are included. The search does not need a clear look at the object.
- The count includes plants and bushes that are not ready yet. `Harvested 3 out of 10 in range` means 3 were picked and 7 more were in range but waiting.
- A second key draws the range on the ground and a small marker on each harvestable in that range. Ready ones are bright. Waiting ones are dim.
- Harvest range can be locked by installing the mod on the server. Hotkeys and the highlight stay on each client.

## Installation

Install [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/) and [Jotunn](https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/). Copy `EvuAreaHarvest.dll` and `EvuAreaHarvest.Core.dll` into `BepInEx/plugins`. Both files have to sit in the same folder.

The mod works from a client. Other players do not need it. Install it on the server as well when you want the server's harvest range to be the one every client uses.

Start Valheim once. Settings are written to `BepInEx/config/evu.evuareaharvest.cfg`. The default harvest key is numpad Delete. The default highlight key is End. The full list is in [docs/CONFIGURATION.md](docs/CONFIGURATION.md).

## Build

See [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md). `make verify` builds the plugin and runs the tests.

## License

[MIT](LICENSE)
