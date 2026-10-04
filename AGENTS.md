# Agent Instructions

These instructions apply to the EvuAreaHarvest repository.

## Read Order

1. Read [README.md](README.md) for what the mod does.
2. Read [ROADMAP.md](ROADMAP.md) before adding a feature that is listed as later work.
3. Read [docs/DEVELOPMENT.md](docs/DEVELOPMENT.md) for the toolchain, project split, and verification.
4. Read [docs/CONFIGURATION.md](docs/CONFIGURATION.md) before changing settings or defaults.

## Product Boundary

EvuAreaHarvest harvests press-E pickables inside a sphere centered on the local player. Plants that are not ready yet are counted and not picked. The pick itself is Valheim's `Pickable.Interact` and `PickableItem.Interact`. Wards and respawn stay on that path.

The mod loads on a dedicated server so the harvest range can sync, and on any client that wants the hotkeys. Other players do not need the mod. The highlight and the hotkeys do not sync. Jotunn is a hard dependency. Configuration Manager is a soft dependency.

The small marker on each harvestable is a stand-in. Replace `HarvestMarkers` when that style changes. Leave the range ring.

## Project Split

- `src/EvuAreaHarvest.Core` is `netstandard2.0` and has no Unity types. The range clamp, the sphere test, the tally, and the log lines live here so they can be tested without the game.
- `src/EvuAreaHarvest` is the `net48` BepInEx plugin. It reads config, scans the loaded area, and draws the rings.
- `tests/EvuAreaHarvest.Core.Tests` covers the core. In-game checks are manual.

## Verification

Run `make verify` after code changes. That fetches reference assemblies when needed, builds the solution, and runs the tests.

Do not commit `.refs/`, `bin/`, `obj/`, or `dist/`. Valheim, BepInEx, and Jotunn binaries stay out of git.

## Commits

Use [Conventional Commits](CONTRIBUTING.md). release-please opens release pull requests from those messages. A `fix: rebuild against Valheim <buildid>` commit is reserved for the game-update workflow.
