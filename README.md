# Tabernacle Walkthrough — Demo 5

Explore the Tabernacle of Moses, learn the furnishings and priestly service, and ask a conversational guide concise questions grounded in Scripture. Built for Quest 2/3 using Unity 6000.3.22f1 and OpenXR.

## Download and demonstration

- [v0.8.0 release and Quest APK](https://github.com/coo64362/tabernacle-vr/releases/tag/v0.8.0)
- **Video: not recorded yet.** The assignment requires a 3–6 minute YouTube demonstration with the user in an inset. [Recording plan](Documentation/Review-v0.8/VIDEO-PLAN.md).
- Install the APK using Meta Quest Developer Hub; open **Tabernacle Walkthrough** under **Unknown Sources**.

## Basic controls

- Begin inside the courtyard; the welcome locks movement until it finishes. Settings → Comfort can skip it.
- Raise the left controller for the instructions/tour/settings tablet; **Y** hides/shows it. Point with the right controller and press its trigger to select.
- Aim away from the tablet, hold/release the right trigger to teleport. Right stick snap-turns. **A** advances the tour.
- Hold **X**, speak, then release to ask the guide. Questions need internet and microphone permission; the recorded tour works offline.
- Face the idle guide and wave a raised controller or nod deliberately; it acknowledges the gesture.
- Settings → Avatar selects male/female and child/adult. **Male + Child** uses the new detailed boy; **Female + Child** uses the detailed girl. Adults remain basic placeholders.
- Look into the laver for your tracked reflection. Hold X while speaking to animate your avatar's mouth. Tour → Most Holy Place → Enter educational view provides access beyond the veil. Viewing assistance helps you see higher while keeping your real feet on the floor.

## AI usage

One extended chat over several days, with over 100 requests/refinements in the supplied conversation; individual tool/API calls were not counted. Codex helped with Unity code, authoring tools, checks and documentation. Built-in image generation produced references/materials; Microsoft TRELLIS.2 produced the child/robot mesh sources. Blender handled rigs/mouth geometry; local uLipSync estimates vowels; VelChat generates live answers and streamed speech. Meshy adult models are not included.

Layout, controls and stored narration came together relatively quickly. Matching the approved characters, fingers, long-robe deformation and mouth interiors took repeated attempts and visual corrections. Three real user prompts:

1. “make the settings tablet smaller, make there borders between instructions, tour and .. ther other thing”
2. “the face you put ontop should match the generated image dont be lazy”
3. “if we are giving hard coded anwers instead of letting the ai act like a human, then we arent doing well. the hardcoded answers would only be for the tour, and then whenever a question is asked the ai should generate quick, because any question can be asked.”

[Further AI disclosure](Documentation/Review-v0.8/AI-USAGE.md).

## Validation and remaining requirements

The current candidate has editor rig, speech, playback, gesture and experience checks. Mouth motion estimates vowels; exact consonant/word timing is not provided. No current Quest performance, microphone-latency or other-person playtest is claimed. Detailed adults were deferred by the owner. The guide is a robot; acceptance under the rubric's **virtual human** requirement is not established.

This project covers the VR Demo 5. The assignment also requires a separate MR Demo 6, the class repository structure, release/video links, and a graduate study if applicable. See the [requirement audit](Documentation/Review-v0.8/ASSIGNMENT-AUDIT.md). This repository remains separate from the assignment repository as requested.

[Build/implementation documentation](Documentation/IMPLEMENTATION.md) · [v0.8 checks and limits](Documentation/Review-v0.8/README.md) · [Biblical accuracy](Documentation/ACCURACY_AUDIT.md) · [Art provenance](Documentation/ART_PROVENANCE_v03.txt) · [Music sources](Documentation/Music/SOURCE-AND-USAGE.txt)
