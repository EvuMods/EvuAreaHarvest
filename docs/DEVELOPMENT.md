# Development

## Layout

- `src/EvuAreaHarvest.Core` holds the range clamp, the sphere test, the harvest tally, and the log lines. It targets `netstandard2.0` and does not reference Unity.
- `src/EvuAreaHarvest` is the BepInEx plugin (`net48`). It reads config, finds pickables, calls Valheim's own pick interaction, and draws the range.
- A healthy `Plant` whose grown prefab carries a `Pickable` counts as in range and not ready. Tree saplings, plants that cannot grow where they stand, and anything without a live `ZNetView` (such as the cultivator's placement ghost) are skipped. A `Pickable` or `PickableItem` is harvested only when Valheim already considers it ready. Respawn timers, skills, and stats stay on that vanilla path. `Pickable`s with an aggravate range are skipped unless `Pick guarded items` is on.
- The hotkeys use `Player.TakeInput()`, the same gate as Valheim's own keys, so the map, menus, and text boxes swallow them.
- `HarvestMarkers` draws the small rings. That drawing is the piece to replace if the highlight style changes. The range ring is separate.

## Prerequisites

- .NET SDK 8. `make` uses `~/.dotnet` when that install exists, including when `/usr/bin/dotnet` is only a runtime.
- `make`, `curl`, `unzip`, and `python3`
- Either a local Valheim install (`VALHEIM_INSTALL` pointing at the game directory) or SteamCMD, which `scripts/fetch-refs.sh` downloads for you

The script also downloads the BepInEx pack version in `deps/bepinex-pack.version` and the Jotunn version in `deps/jotunn.version`. Assemblies land in `.refs/`, which is gitignored. `BepInEx.AssemblyPublicizer.MSBuild` publicizes `assembly_valheim.dll` during the plugin build.

Reference assemblies come from the Valheim dedicated server (Steam app 896660). `VALHEIM_INSTALL` copies from a local client instead.

## Commands

```sh
make fetch-refs
make build
make test
make verify
make package
```

`make verify` is the gate: fetch references if needed, build, and test. `make package` writes `dist/EvuAreaHarvest-<version>.zip` in the Hexium layout: `manifest.json`, `icon.png`, `README.md`, `CHANGELOG.md`, `EvuAreaHarvest.dll`, and `EvuAreaHarvest.Core.dll` all at the zip root. `icon.png` must be 256×256. `version_number` in the packaged manifest is taken from `version.txt`. Jotunn is a manifest dependency. BepInExPack is not; Hexium assumes it and strips that entry on upload.

The release workflow attaches that zip and the two raw DLLs to the GitHub release. Publishing that release also publishes the same version to Thunderstore. A release created from this workflow does not start other workflows by itself, because it uses `GITHUB_TOKEN`, so release-please dispatches the Thunderstore workflow with the new tag. A release published any other way starts that workflow directly. The manual Action remains: leave the tag empty to package the selected branch, or set a release tag such as `v0.1.0`. It publishes team `EvuMods` to the Valheim community with categories Mods, AI Generated, Tweaks, Client-side, Server-side, and the update slug from the `game_category` input (`deep-north-update` when a release starts it). NSFW is off. The service account token belongs in the `TCLI_AUTH_TOKEN` repository secret, not in the repo.

## Version

The release pull request is the only edit of `version.txt` and of `version_number` in `manifest.json`. release-please writes those, plus `CHANGELOG.md`, from the conventional commits since the last release. Merging that pull request tags `vX.Y.Z`. `make package` on that tag reads `version.txt`, so the zip is `EvuAreaHarvest-X.Y.Z.zip` and the packaged manifest uses the same number. `Directory.Build.props` reads `version.txt` for the plugin version. A later hand edit of either file is overwritten on the next release.

## Game updates

`.valheim-buildid` is the dedicated-server build this tree was last verified against. The Valheim update workflow compares it with the public Steam build. A green `make verify` opens a pull request whose message is `fix: rebuild against Valheim <buildid>`. A failed build opens a GitHub issue and does not release.

A green compile means the referenced members and the core tests still hold. It does not play the game. Check a harvest in a client after a game update that you care about.

For that chain to publish on its own, the release-please workflow dispatches itself after merging a release pull request that contains only those rebuild commits. GitHub does not start a new workflow from `GITHUB_TOKEN` alone, so the dispatch is explicit. See `.github/workflows/`.

## In-game check

There is no automated playtest here. After a harvest or highlight change, confirm:

- Ten mushrooms in range, three of them ready, logs `Harvested 3 out of 10 in range`. The same line shows in the center of the screen.
- No harvestables in range logs `Harvested 0 out of 0 in range`.
- A dandelion planted under the surface is still harvested.
- A berry bush that respawned while you stayed nearby is counted on the second harvest, not only the first.
- A planted beech sapling is not counted and gets no marker. A carrot seedling is counted and gets a dim marker.
- Holding the cultivator with a seed selected does not add the placement ghost to the count.
- With the map or the inventory open, neither key does anything.
- A Dvergr lantern or chest item in range is left alone and not counted until `Pick guarded items` is on.
- End turns the highlight on and logs `Highlight harvestables on`. The range ring sits under you and pulses. A second ring expands out to the edge. Ready plants get a bright marker. Waiting ones get a dim marker. Markers uphill and downhill from you sit on their own ground.
- The highlight and both hotkeys stay as you set them when the server has a different harvest range.
- With the mod on the server, a client's harvest range follows the server. Without the mod on the server, the client's own range is used.
