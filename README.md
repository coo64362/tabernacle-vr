# Tabernacle Walkthrough

Native, offline Unity/OpenXR walkthrough for Quest 2 and Quest 3. Version 0.2 revises the visual treatment with generated photographic material maps, modeled cloth folds, an embroidered inner veil, worked metal edges, individually modeled wing feathers, and precomputed illumination. It is an art revision of the initial prototype, not a claim of identical fidelity to the concept image.

## Controls

- Hold the right trigger to aim a teleport arc; release over a gold destination marker to move.
- Right thumbstick left/right: 30-degree snap turn.
- Right A: advance through six guided stops, including an explicitly educational reveal behind the veil.
- Right B: toggle frame-rate display.
- Physical movement remains available within the headset's configured boundary.

## Accuracy baseline

Coordinates: positive Z west, positive X north. One cubit is represented as 0.4572 metres, a modeling convention rather than a claim of a uniquely established ancient unit.

- Courtyard: 100 × 50 cubits; enclosure 5 cubits high; east entrance.
- Tent: conventional 30 × 10 cubit reconstruction, 10 cubits high; interior division at 20 cubits.
- Bronze altar: 5 × 5 × 3 cubits, horns, network and poles. All five utensil types: ash pots, shovels, basins, forks and firepans.
- Bronze washing basin and stand between altar and tent; no invented faucets.
- Gold-overlaid table: 2 × 1 × 1.5 cubits; twelve individual loaves in two stacks of six, frankincense and gold vessel types.
- Gold lampstand: six side branches and central shaft, seven oil lamps, short wicks, almond ornamentation, trimming tongs, tray and oil vessel.
- Gold incense altar: 1 × 1 × 2 cubits before the inner veil.
- Ark: 2.5 × 1.5 × 1.5 cubits, mercy seat, two cherubim and retained carrying poles.
- Entrance posts have bronze bases; four inner veil posts have silver bases. Entrance screens have no cherubim.

Utensil counts beyond explicit prescriptions, vessel and basin profiles, lampstand height/branch curves, bread shapes and cherubim anatomy are interpretive. Symbolic cherubim are not definitive anatomical reconstructions. Ceiling embroidery and construction details are simplified in this prototype. This is not yet an exhaustive model of every construction fastening or ritual implement mentioned throughout Scripture. No later Temple implements are silently added.

Sources: [Exodus 25](https://bible.usccb.org/bible/exodus/25), [Exodus 26](https://bible.usccb.org/bible/exodus/26), [Exodus 27](https://bible.usccb.org/bible/exodus/27), [Exodus 30](https://bible.usccb.org/bible/exodus/30), [Exodus 38](https://bible.usccb.org/bible/exodus/38), [Exodus 40](https://bible.usccb.org/bible/exodus/40), [Leviticus 24](https://bible.usccb.org/bible/leviticus/24), [Numbers 4](https://www.biblegateway.com/passage/?search=Numbers+4&version=NKJV). Historical inner-sanctuary access is described in Leviticus 16.

## Performance design

- Native ARM64 build; OpenXR single-pass instanced rendering.
- Geometry, materials, lightmaps, light probes and reflection probes are authored into the scene before building.
- Material grain and fine relief use mipmapped photographic surface maps, with distance filtering and four-sample antialiasing.
- Interior exposure adapts smoothly for visibility; additional baked display illumination is an educational presentation choice, not a claim about historical light fixtures.
- 72 Hz requested using XR_FB_display_refresh_rate; actual refresh and runtime frame rate recorded.
- Four-sample antialiasing, mipmapped woven texture, anisotropic filtering.
- Geometry combined by material per environment/furnishing; no runtime shadows, transparency, smoke simulation, realtime reflections or complex cloth simulation.
- Small lampstand details are hidden beyond seven metres; the main silhouette and seven lamps remain.
- Teleport destination checks reject obstacles. No continuous camera motion.
- On-device log tags: TABERNACLE_READY, TABERNACLE_INVENTORY, TABERNACLE_XR, TABERNACLE_REFRESH, TABERNACLE_PERF, TABERNACLE_STATION.

The displayed frame rate is observed application throughput, not certification of frame pacing, thermal stability or user comfort. Validate representative stations while wearing the headset and over an extended session before claiming final performance.

## Install

Install Builds/TabernacleVR.apk using Meta Quest Developer Hub while the headset is connected and USB debugging is authorized. Open Tabernacle Walkthrough from the headset app library under Unknown Sources. The included install-on-quest.sh can install and launch it from this Mac once a single Quest is detected.

## Build

Open in Unity 6000.3.22f1 with Android Build Support. Packages are pinned through Packages/manifest.json and packages-lock.json. The built scene is Assets/TabernacleRealistic.unity. Select **Tabernacle → Configure and build Quest** to build that scene. For geometry/material regeneration, execute RealismBuild.PrepareAndReview first and allow the GPU lighting bake to finish. Output: Builds/TabernacleVR.apk. Package identifier: org.tabernacle.walkthrough. The initial version is retained as Builds/TabernacleVR-v0.1.apk.

The prototype is local and offline. It does not modify the existing cloud hosting deployment.
