# Scripture accuracy and Quest 2 audit

Reviewed against the v0.2 source (`TabernacleWorld.cs`, `FurnishingDetail.cs`, `WorldRealism.cs`), the shaders, the texture import settings and the preview renders in `Documentation/Previews`.

**Not verified:** Unity was not available when this was written. The C# was syntax-checked only (not compiled), and the new textures were checked as images, not in the engine. Re-run the realism build and read the `REALISM_CHECK_*` log lines before trusting any of it.

Key: ✅ matches the text · ⚠ the text doesn't say, so it's a modelling choice · ❌ was wrong in v0.2, changed in this branch · 🔎 not changed, needs checking in Unity or on the headset

## Courtyard (Exodus 27:9-19)

| Item | What the text says | v0.2 | Status |
|---|---|---|---|
| Size and hangings | 100 × 50 cubits, fine linen hangings 5 cubits high | correct | ✅ |
| Pillar count | 20 south, 20 north, 10 west; east has 3 pillars per 15-cubit wing and 4 for the 20-cubit gate (60 total) | 10 evenly spaced across the whole east side, ignoring the wings and the gate | ❌ now 3 + 3 + 4 on the east |
| Pillar parts | bronze bases, silver hooks and bands, silver-overlaid capitals (27:10-11; 38:10-12, 17) | whole pillar was bronze | ❌ bronze base, silver hook, silver fillet and capital band |
| Pillar shaft material | not stated | bronze | ⚠ now acacia, which is an inference |
| Gate screen | 20 cubits, blue, purple and scarlet with twisted linen, embroidered (27:16) | two narrow plain-purple panels | ❌ two panels with a woven blue/purple/scarlet texture, drawn back in the middle so visitors can walk in (⚠ deliberate) |
| Hangings | hung from the hooks between pillars | stiff, regular corrugation, which reads as sheet metal | ❌ softer irregular folds, with a shallow sag between pillars |

## Bronze altar and basin (Exodus 27:1-8; 30:17-21; Leviticus 6:12-13)

| Item | v0.2 | Status |
|---|---|---|
| 5 × 5 × 3 cubits, four horns, hollow, bronze network halfway up | present | ✅ horns made larger so they read from a distance |
| Ledge around the altar (27:5) | missing | ❌ added |
| Bronze rings, with the poles in them below the ledge (27:4, 6-7) | no rings, poles above the ledge | ❌ rings and brackets added, poles lowered |
| Five utensil types (pots, shovels, basins, forks, firepans) | present | ✅ |
| Fire kept burning on the altar (Lev 6:12-13) | none | ❌ wood and flame added, with a baked warm light |
| No steps up to the altar (Ex 20:26) | none | ✅ left without a ramp or steps |
| Basin: bronze, with a bronze base, between altar and tent | present, with no size given in the text | ✅ ⚠ size is a choice |

## Tent of meeting (Exodus 26; 36; 38:27)

| Item | What the text says | v0.2 | Status |
|---|---|---|---|
| Size | 30 × 10 × 10 cubits, 20-cubit Holy Place and a 10-cubit cube Holy of Holies | correct, veil at 20 cubits | ✅ |
| Frames | 48 frames, 10 × 1.5 cubits: 20 + 20 + 8 (6 plus 2 corner) | 40 frames and one solid gold slab for the west wall | ❌ 48 frames |
| Silver sockets | 2 under each frame (96), plus 4 under the veil pillars (100 in all, 38:27) | 80 + 4 | ❌ 96 + 4 |
| Crossbars and gold rings (26:26-29) | five bars per side | not modelled | ⚠ not added: the coverings hide them, so they'd cost triangles for nothing. Add them if visitors can ever walk around the outside. |
| Ceiling | 10 linen curtains, 4 cubits wide, blue/purple/scarlet with cherubim (26:1-6) | one plain purple slab plus three patches of the veil art | ❌ eight panels (7½ curtains' worth) using your veil embroidery, so the cherubim match the veil |
| Clasps | 50 gold clasps join the two sets of five curtains (26:6); the veil hangs beneath them (26:33) | none | ❌ 50 gold clasps at the 20-cubit line |
| Coverings | goat hair, then ram skins dyed red, then tahash skins on top (26:7-14) | one dark brown slab | ❌ three separate layers, each with its own texture, hems stepped so all three show |
| Entrance | 5 pillars, bronze bases, gold hooks, an embroidered screen with no cherubim (26:36-37) | 5 pillars ✅, plain purple screen | ❌ screen now uses the striped weave |
| Veil | blue, purple, scarlet with cherubim, on 4 gold-overlaid pillars on silver sockets (26:31-32) | correct | ✅ |

## Furnishings (Exodus 25; 30:1-10; Leviticus 24:5-9)

| Item | Status |
|---|---|
| Ark: 2.5 × 1.5 × 1.5 cubits, gold crown, four rings, poles kept in the rings | ✅ |
| Mercy seat with two hammered cherubim facing each other, wings spread up and over | ✅ layout. 🔎 the figures themselves look like blobs (see below) |
| Table: 2 × 1 × 1.5 cubits, rim, four rings, poles, dishes, pans, flagons and bowls | ✅ |
| Twelve loaves in two rows of six, frankincense | ✅ round loaves are ⚠ (shape isn't stated) |
| Lampstand: gold, six branches plus the shaft, seven lamps, almond cups (4 on the shaft, 3 per branch), knops, tongs and trays | ✅ |
| Table north, lampstand south, incense altar (1 × 1 × 2 cubits) before the veil | ✅ |
| Entrance faces east | ✅ |

## Choices the text leaves open

These are not errors, but you should decide them deliberately.

- **Cherubim form.** Exodus 25:18-20 gives their number, position and wing posture, not their appearance. The veil art shows human-headed winged lions with Neo-Assyrian hair and headbands. That style is first-millennium BC, well after the Exodus setting. Scholars more often compare earlier Canaanite and Egyptian winged sphinxes (for example the Megiddo ivories). It's attractive art; just don't present it as established. The README already calls it interpretive.
- **Ark orientation.** 1 Kings 8:7-8 says the pole ends could be seen from the Holy Place, which suggests the poles ran east-west. In the model they run north-south, so the cherubim face each other across north-south. Rotating the ark group 90° is one line, but then the first view from the veil changes.
- **Ark contents and light.** The Holy of Holies had no lamp. The model adds a baked reveal light, which the README already labels as educational.
- **Where the inner curtains hang.** Many readers have them draped over the outside of the gold frames (so the frames aren't visible from inside). The model shows the frames inside with the curtains as the ceiling. Both are defensible.
- **Tahash.** The word is uncertain (sea cow, fine leather, others). The texture is a deliberately generic tough hide.
- **Gate drawn back.** The text gives the screen a full 20 cubits. It's split in two with a 2.5 m opening for teleporting.

## Setting

The wilderness of Sinai is mostly gravel plains, dry wadis and granite mountains, with scattered acacia (the likely "shittim" wood) rather than soft dunes. The Levites camped around the tabernacle (Moses and Aaron east, Kohathites south, Gershonites west, Merarites north: Numbers 3:21-38), with the tribes beyond in the order of Numbers 2. None of this is modelled. A gravel texture, rocky ridge silhouettes and a few acacia shrubs would improve both accuracy and realism cheaply. Distant black tents would be a good low-poly use of GPU instancing.

## Problems visible in the v0.2 preview renders (not fixed, needs Unity)

1. **Bronze tools render solid black** (`altar-utensils.png`). Almost certainly high metallic with no usable reflection data at that spot. Try lowering `_Metallic` on Bronze to about 0.5 as a test, or check the courtyard reflection probe's capture.
2. **Gold frames in the Holy Place read dark grey** (`holy-place.png`). Same cause.
3. **South court wall looks black from inside** (`courtyard.png`). Both long walls were rotated the same way, so their normals pointed the same direction and one faced outward. This branch turns the south wall around. Check it.
4. **The altar reads as a brown crate.** The bronze albedo is dark and matte. Brighter albedo and a little more smoothness would help.
5. **Cherubim look lumpy** (`ark-chamber.png`). Spheres and tubes won't read as hammered gold. See the model slot below.

## Cherubim model slot

`Guardian()` now looks for `Assets/Resources/Models/Cherub.prefab`. If it exists, it's placed at each end of the mercy seat instead of the procedural figure, turned to face the other. Author it facing +X with its feet at the origin, about 0.45 m tall, gold material, under roughly 6,000 triangles.

## Quest 2 notes

- **Texture import** (done in `RealismBuild.Prepare`): anisotropy 8 → 4, and on Android the tiling maps go to 1024 px ASTC 6×6. The two cherubim textures stay 2048 px ASTC 4×4. The old 1254 px PNGs were non-power-of-two.
- **Shader cost:** `TabernaclePhysical` samples every texture three times per pixel to fake relief from the albedo. Baking real normal maps (one sample) is the biggest remaining per-pixel saving.
- **Graphics API:** the build forces OpenGL ES 3. Vulkan is worth a trial on Unity 6.
- **Foveated rendering:** not enabled. Fixed foveated rendering is usually a large, cheap win on Quest 2.
- **Triangles:** this branch adds geometry (pillar fillets, 8 more frames, 50 clasps, three covering layers on three sides each, altar flames) but the added count has not been measured. `PrototypeReview.Run` writes the current total to `review.txt`. Compare it with v0.2 and aim for well under 750,000.
- **Frame rate:** `verification.json` still has `on_device_fps: null`. None of the above has been measured on a headset.

## To apply

1. Merge this branch.
2. Optional: `python3 Tools/make_textures.py` regenerates the new textures (needs numpy, scipy, Pillow). The PNGs are already included.
3. Unity 6000.3.22f1: run `RealismBuild.PrepareAndReview` and wait for the lighting bake. The new `REALISM_CHECK_PASS` lines should list 48 frames, 96 sockets, 65 posts (60 court + 5 tent) and 50 clasps.
4. Look at the six new preview renders, especially the items under "Problems visible" above.
5. Tabernacle → Configure and build Quest, then test on the headset.
