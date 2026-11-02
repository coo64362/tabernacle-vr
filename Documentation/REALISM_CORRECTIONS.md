# Furnishing and tablet corrections — v0.3

Based on claude/scripture-accuracy-quest2 at cda97381.

- Twelve modeled loaves now have flat supporting faces and contact their plates or the loaf below. Both stacks have a plate. Round shapes, profiles and vessel quantities remain interpretive. Leviticus 24:5–9 prescribes twelve loaves, six in each arrangement.
- Seven oil lamps now sit on gold seats joining the branch/shaft tips. Oil and wick details remain; Exodus 25:31–40 distinguishes the lampstand and its seven lamps. Scaled bowl wall/floor thickness replaces fixed dimensions that distorted small cups.
- Incense altar remains 1 × 1 × 2 cubits, gold over acacia, with four tapered horns and two carrying rings. Ash/embers on the top are illustrative, not a prescribed separate plate. Exodus 30:1–10 does not describe a horn-shaped censer as the altar. Leviticus 16:12–13 describes a portable censer for the special entry behind the veil.
- Removed the earlier specifically Assyrian-looking winged-lion textile figures. New original, abstract kneeling winged figures have no crowns, curled royal beards or animal bodies. Sculptures likewise use schematic crownless kneeling figures. Exodus 25:18–20 describes number, facing and wings but does not settle anatomy. These alternatives are explicitly interpretive, not a claim of a recovered Mosaic likeness. Historical winged hybrid motifs are older and broader than the Neo-Assyrian examples; it would be inaccurate to claim all human-headed winged figures originated after Moses.
- All normal instructions and destination controls move to a stone tablet attached to the left controller. Uncial Antiqua is bundled under the SIL Open Font License. Y shows/hides the tablet; right trigger selects; Instructions / Tour / Settings tabs. Music on/off and volume, plus Help me see higher / Back down, are functional. Performance display remains an optional developer diagnostic on B.
- Tour → Most Holy Place first explains historically restricted access (Leviticus 16), then Enter educational view fades to a verified safe destination beyond the veil. A still advances the guided stations. A full closed veil remains in the historical layout.
- Start position is inside the courtyard at z=2 m, near the east entrance.
- A low-cost two-hymn playlist uses newly synthesized public-domain tunes; see Music/SOURCE-AND-USAGE.txt. The robot guide, avatars, welcome speech and live water reflection remain future work.

## Video reviewed

https://www.youtube.com/watch?v=1mFGsLEpKKk
Messages of Christ, “Tabernacle of Moses,” 3:49. Description identifies a modern replica built for the 2016 Huntington Beach Stake Tabernacle Youth Camp. Reviewed sections: opening/gate (0:00–0:28), altar (0:28–0:58), courtyard (0:58–1:08), laver (1:08–1:29), Holy Place (1:29–1:45), menorah (1:45–1:56), bread table (1:56–2:16), incense altar (2:16–2:41), veil/Most Holy Place (2:41–2:47), Ark and contents (2:47 onward), exterior ending. Used as a material/scale/placement reference alongside Scripture, not an archaeological authority. No cloth cherubim or video assets copied; no assertion that its plate, fittings or relic arrangement are biblically prescribed.

## Sources

- https://www.biblegateway.com/passage/?search=Exodus+25&version=ESV
- https://www.biblegateway.com/passage/?search=Exodus+30%3A1-10&version=ESV
- https://www.biblegateway.com/passage/?search=Leviticus+24%3A5-9&version=NIV
- https://www.biblegateway.com/passage/?search=Leviticus+16%3A12-13&version=ESV
- https://www.metmuseum.org/art/collection/search/322609 (Neo-Assyrian lamassu, 883–859 BCE; comparative style evidence only)
- https://github.com/google/fonts/tree/main/ofl/uncialantiqua

## Validation

VerifyFurnishingContacts checks actual rendered mesh bounds for every bread-to-support contact and seven lamp seats before batching. Existing frame/socket/clasp/loaf/lamp counts and safe/blocked destinations remain checked during regeneration. CorrectionReview.Tablet raycasts against the tablet collider and checks tabs and two-step Most Holy Place navigation. RealismBuild rebuilds meshes, lightmaps and reflection probes and generates six room/furnishing previews. Test on headset for controller ergonomics and representative interior frame pacing; short startup FPS is not full performance certification.
