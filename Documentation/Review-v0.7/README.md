# Human speech face, v0.7

The accepted female child now has a split lip seam, six imported speech morphs (A, I, U, E, O and a near-closed N), and a small skinned inner mouth, upper tooth band and tongue. The neutral face, hair, veil, body and finger rig are retained. The Blender images in this folder show authored poses; they are not headset screenshots.

While holding X, the same microphone recording used to ask the guide feeds a local uLipSync MFCC phoneme estimator. No second microphone capture, audible microphone monitoring, new service upload, or additional AI prompt is introduced. Sample blocks are converted from the microphone rate to Unity's output rate, as required by the analyser. The two strongest estimated vowels blend smoothly; energy gates silence and cancellation closes the mouth. The pose is applied before the laver reflection render. The existing nearby-only reflection remains capped at 12 Hz to retain its rendering budget; fast mouth movement in that reflection can therefore appear less smooth than a directly rendered speaking character. Analysis is disabled when inactive.

`HumanSpeechFace.BindAudio` also supports playback-driven faces on future human speakers; the current girl is the visitor's reflection, so her mouth follows the visitor, not the robot's answers. Boy/adult characters have not been redesigned. The robot retains its existing digital speech animation.

This is estimated audio-driven speech animation, not exact word/phoneme alignment. The included generic profile is a starting point: different voices, consonants and noisy microphones need visual evaluation and potentially voice calibration. The N shape serves as a near-closed fallback for unvoiced sounds; it is not a separately trained consonant classifier. Precise consonants would need a broader calibrated profile or timed viseme data from the voice service.

Editable source is saved separately at `v0.7/CharacterSources/FemaleChild-Speech.blend` on the P7 SSD. `Tools/add_speech_shapes.py` reproduces the changes from the approved rigged source using explicit input/output paths.

No Quest was connected for this update. Headset microphone latency, sustained frame rate and in-headset visual quality remain to be tested.

Validation: imported rig/material checks and Unity face renders passed. An offline synthesized English test sentence drove all six shapes through the actual MFCC/Burst jobs, with finite blend weights, silence closure, and immediate closure on capture stop. Existing welcome/avatar/reflection settings checks passed. These checks do not establish phoneme accuracy for every visitor voice.

Quest v0.7.0 (8) built successfully with zero build errors. APK archive integrity passed; ARM64-only libraries include the compiled Burst analyser. APK SHA-256 and size are recorded in `build-check.json`. Hardware installation was deferred because the Quest is not connected.
