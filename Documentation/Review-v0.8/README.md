# v0.8: detailed boy and speech/interaction polish

The detailed TRELLIS boy is selectable in Settings → Avatar → Male / Child. His body and fingers are skinned, the reflection has six fitted speech shapes, and the first-person body omits head/neck triangles and unused mouth morphs. Source face textures are preserved; no replacement face was painted over the model. The raw master remains separate. The rig uses around 87,000 triangles before the small mouth surfaces. Sleeve weights were reviewed in a raised-arm pose and spatially smoothed without welding UV geometry. Extreme clothing poses still need headset evaluation.

Both children's microphone-driven mouths use local MFCC vowel estimates. The robot now uses those vowel estimates to vary its digital mouth width and opening. Actual playing AudioSource frames are downmixed and transferred from the audio callback through a fixed eight-block queue, consumed once on the main thread. Stopping/muting clears the queue. There is no audible microphone monitoring, additional upload or additional AI prompt. This improves shape variety and playback sampling; it does not provide exact timed consonants or word alignment.

The robot responds to deliberate tracked waves and nods while idle and nearby. Recognition excludes missing tracking, sudden head translations, busy speech/listening/thinking and a five-second cooldown. The hand/head tracking used is already part of the experience. No camera recording is added. The acknowledgement is a wave or nod, without interrupting narration or answers.

Character shading uses softer wrapped diffuse light and restrained edge light, while preserving the accepted textures. Character textures use mipmaps, trilinear filtering and 4× anisotropic filtering. The nearby-only reflection stays 512px at 12 Hz; no unmeasured reflection-rate increase was added.

## Validation

- Character import, normalized weights, materials, six morphs, finger chains and headless first-person assets checked in Unity.
- Audio queue downmix, frame counts, single consumption, overflow bound, stop/mute clearing checked.
- Deliberate wave/nod responses, cooldown, speech suppression and absent tracking checked with simulated trajectories. Real-world recognition remains untested.
- Recorded speech drove the girl's six shapes and varied the robot's width/opening; silence and capture stop reset the face.
- Final boy and robot passed the actual welcome-narration analysis; the boy exercised all six mouth shapes. Actual AudioSource playback also drove the robot through the callback queue and closed at the end.
- Welcome, movement gating, skip/replay, six narration clips, tablet selection, avatar/reflection settings, height match, guide mute and station positions passed in Unity.
- Quest v0.8.0 (9) built with zero errors. APK: 124,569,124 bytes; ZIP integrity, APK signature, ARM64/Burst libraries and microphone permission passed. See `build-check.json`.
- No connected Quest: on-device microphone, visual comfort, sustained frame rate and another person's playtest remain outstanding.

Detailed adults were deferred explicitly by the owner. The conversational character remains the sandstone robot; the course specifically requests a virtual human, so see the assignment audit before claiming complete rubric alignment. The VR app covers Demo 5 only. A separate Demo 6 and graduate-only study, if applicable, require their own evidence.

## Reproduce boy authoring

Using the approved raw `male-child-trellis-direct.glb`, run `Tools/prepare_boy.py` with `TABERNACLE_ART` pointing to the art directory, then `Tools/smooth_boy_weights.py --source <base blend> --output <body rig blend>`, then `Tools/add_boy_speech_shapes.py --source <body rig blend> --output <speech blend> --fbx <Unity MaleChild.fbx>`. Run `SubmissionReview.Prepare` to import and validate. The reviewed source for this build is `MaleChild-Speech.blend`, not rejected welding experiments.
