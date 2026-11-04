# Tabernacle Walkthrough

Native Unity/OpenXR walkthrough with offline tour narration and online questions for Quest 2 and Quest 3. Version 0.3 revises the visual treatment with generated photographic material maps, modeled cloth folds, an embroidered inner veil, worked metal edges, individually modeled wing feathers, and precomputed illumination. It is an art revision of the initial prototype, not a claim of identical fidelity to the concept image.

## Version 0.8 submission preparation

The detailed TRELLIS boy joins the accepted girl as a selectable tracked child avatar, including six speech mouth shapes and a headless first-person body. The robot's digital mouth now changes width/opening using local vowel estimates from the actual playback blocks. Playback analysis consumes each audio block once through a bounded audio-thread queue. An idle guide acknowledges deliberate tracked waves and nods, with a cooldown and suppression during speech, listening and thinking. Character texture filtering and soft shading were refined. Detailed adult avatars are deferred.

The purpose is to help visitors learn the Tabernacle's furnishings and priestly service through an embodied tour, concise questions and visible Scripture references. The main guide remains a robot, which needs to be assessed against the course's explicit virtual-human requirement. See [the assignment audit](Review-v0.8/ASSIGNMENT-AUDIT.md), [AI usage and five real prompts](Review-v0.8/AI-USAGE.md), and [video plan](Review-v0.8/VIDEO-PLAN.md).

Submission video: not recorded yet. Current release: [v0.8.0](https://github.com/coo64362/tabernacle-vr/releases/tag/v0.8.0). The walkthrough covers Demo 5; a separate mixed-reality Demo 6 is required. Hardware tests and another person's playtest are outstanding. No final headset performance or grading compliance is claimed.

## Version 0.7 human speech face

The accepted female child's reflection now has six speech mouth shapes, an inner mouth, upper teeth and tongue. While holding X, her mouth follows the visitor's microphone audio through local uLipSync phoneme estimation. The microphone is shared with the question recorder; there is no audible monitoring, extra upload or additional AI prompt. The controller supports speech playback for future human speakers too. The neutral face and existing body/finger rig are preserved; unused mouth shapes are stripped from the headless first-person body.

This estimates vowels and smooth transitions; it does not provide exact consonant or word alignment. Boy/adult faces and Sandy's existing digital mouth are unchanged. Offline speech, silence/cancellation, imported-face renders and existing experience checks passed. Quest microphone latency and sustained frame rate remain untested. See [the v0.7 review](Review-v0.7/README.md).

## Version 0.6 conversational guide

Hold X to speak and release to ask a question from anywhere after the welcome. VelChat generates a fresh, concise answer to the question, with four recent conversation turns retained for follow-ups. The old 25-answer selector is removed. The subject boundary remains the Tabernacle and related biblical history.

The app automatically retrieves relevant passages from 31,098 locally stored public-domain World English Bible verses and supplies them to the model. Cross-reference metadata helps connect furnishings to their instructions and related stories. References named in the answer are checked for real book, chapter and verse numbers before appearing on the tablet. This validates reference existence, not every generated interpretation; the model is instructed to acknowledge uncertainty. General internet search is not connected.

Audio plays from received 24 kHz Opus frames through a bounded PCM ring with a 120 ms initial jitter buffer. Playback does not wait for the full text or an exact-text approval. A hesitation already being spoken can finish before the reply begins. Each request has its own context ID, so late speech packets cannot contaminate the next reply. Conversation continuity comes from bounded in-memory history. Questions may interrupt answer playback after the service has finished generating that turn. Audio and questions are sent to the course service; visitor recordings are not saved by this app.

The tablet does not display transcription, thinking or preparation messages. Sandy responds with attentive head motion, occasional varied 'hmm', 'umm', 'well' or 'let me think', and a longer-wait phrase after 8.5 seconds. Fillers are not repeated in a loop. Nods, explanatory arm motions, a hand-to-chin pose, eye movement, a welcome wave and furnishing-directed gestures accompany relevant sentences. The digital mouth follows actual output audio amplitude and rests during silence. VelChat's documented events provide no phoneme/viseme timings; this is not exact word-shaped human lip sync. Male/adult face work is still separate.

Sandy follows through collision-aware paths while visitors explore. Large visitor teleports allow catch-up beside or behind the visitor in valid walkable space, rather than passing through furnishings. The recorded tour now covers materials, service permissions and ritual processes with references from Exodus and Leviticus. The small tablet remains approximately 22 × 32 cm. Character textures are actual Unity-supported PNGs with explicit material references.

Quest microphone behavior, visual comfort, and sustained performance still require headset testing. See Documentation/Review-v0.6 and Documentation/conversation-live-check.json for editor and service checks.

## Controls

- Raise the left controller to read the stone tablet; Y shows/hides it.
- Point at Instructions, Tour or Settings with the right controller and press its trigger.
- Tour → Most Holy Place → Enter educational view takes you beyond the veil, with a comfort fade and a historical-access explanation.
- Settings → Audio: music, volume, guide voice, captions and replay. Use the < / > heading to change sections.
- Settings → Avatar: male/female, child/adult face, reflection and standing-height match.
- Settings → Comfort: Viewing assistance / Back down, snap-turn angle, robot visibility and skip welcome. Keep your real feet on the floor.
- Aim away from the tablet, hold the right trigger and release over a destination to teleport.
- Right stick: 30° or 45° snap turns. A: next station (Most Holy Place still asks for educational-entry confirmation). B: developer performance display.
- When the guide is idle, face it and wave a raised controller hand or nod deliberately; it waves or nods back.
- Start just inside the courtyard, near its east entrance. The spoken welcome pauses virtual movement until it finishes or is skipped.

Version 0.3 rebuilds the accuracy branch's scene with supported bread, seated lamps, curved incense-altar horns, original interpretive cherubim artwork, two synthesized hymn instrumentals and controller-tablet navigation. See [correction notes](REALISM_CORRECTIONS.md), [art provenance](ART_PROVENANCE_v03.txt) and [music sources](Music/SOURCE-AND-USAGE.txt). Version 0.4 adds the sandy robot, offline British male welcome and station narration, selectable tracked pilgrim avatars, extended tablet settings and an avatar-only laver reflection. See [experience notes](EXPERIENCE_v04.md) for controls, validation and visual limitations.

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
- Geometry combined by material per environment/furnishing; no runtime shadows, smoke simulation or complex cloth simulation. The laver adds a nearby-only 512px avatar reflection at 12 Hz. A brief transparent overlay provides comfort fades for room and height changes.
- Small lampstand details are hidden beyond seven metres; the main silhouette and seven lamps remain.
- Teleport destination checks reject obstacles. No continuous camera motion.
- On-device log tags: TABERNACLE_READY, TABERNACLE_INVENTORY, TABERNACLE_XR, TABERNACLE_REFRESH, TABERNACLE_PERF, TABERNACLE_STATION.

The displayed frame rate is observed application throughput, not certification of frame pacing, thermal stability or user comfort. Validate representative stations while wearing the headset and over an extended session before claiming final performance.

## Install

The v0.7 APK has been built locally and saved on the project owner’s P7 SSD. The existing [v0.4 GitHub release](https://github.com/coo64362/tabernacle-vr/releases/tag/v0.4.0) is an older build. Build outputs are distributed through releases rather than source control.

Install Builds/TabernacleVR.apk using Meta Quest Developer Hub while the headset is connected and USB debugging is authorized. Open Tabernacle Walkthrough from the headset app library under Unknown Sources. The included install-on-quest.sh can install and launch it from this Mac once a single Quest is detected.

## Build

Open in Unity 6000.3.22f1 with Android Build Support. Packages are pinned through Packages/manifest.json and packages-lock.json. The built scene is Assets/TabernacleRealistic.unity. Select **Tabernacle → Configure and build Quest** to build that scene. For geometry/material regeneration, execute RealismBuild.PrepareAndReview first and allow the GPU lighting bake to finish. Output: Builds/TabernacleVR.apk. Package identifier: org.tabernacle.walkthrough. Earlier installers are available from their respective releases.

The tour works offline; generated questions require the course service and internet. This project does not modify the existing cloud hosting deployment.
