# Full VR experience, v0.4

The original sandy robot guide looks toward the visitor, smiles, waves during
the welcome, gestures while speaking and moves to each selected tour stop.
Its original British male narration is generated offline; no network service
or microphone is required. This version does not include live conversation.

The 16-second welcome starts inside the courtyard. Teleportation, snap turns,
station shortcuts and viewing assistance wait until the welcome finishes.
Physical headset and controller tracking always continue. Settings → Comfort
→ Skip welcome releases the movement gate; captions and settings remain usable.
Normal station narration does not lock movement. Music ducks during audible
speech. Settings can mute speech, toggle captions, hide the robot while retaining
its audio, replay a station, and return to the entrance.

Settings → Avatar selects male/female and child/adult face independently. These
are original stylized working 3D models, not Superbook assets or exact replicas
of earlier concept images. Both wear a woven cream robe and russet shoulder
sash without a staff. Controllers drive hands and two-segment arms; there is
no full-body tracking. Torso lean is estimated from head pitch so the robe does
not obscure the face when looking into the water. The standing-height calibration scales the robe/body;
face selection never changes the headset height. The first-person head is
hidden, and the full tracked model appears in the laver reflection.

Settings → Comfort provides Viewing assistance (40 cm lift with a fade, then
Back down), 30°/45° snap turns, guide visibility, skip/stop speech and entrance
return. Most Holy Place access still requires the educational-access notice,
including when requested using A. Settings row separators and tablet tab
borders remain visible. Music and avatar/audio preferences persist.

The water uses one avatar-only 512×512 render texture, updated at 12 Hz within
4 m of the laver and suspended elsewhere. The courtyard is not rendered again.
The water surface itself renders in both eyes. Its reflection uses a shared
central camera image, an intentional approximation rather than a per-eye
planar mirror; close-range stereo comfort needs headset review. To see your
face in horizontal water, lean over the bowl and look down. It reflects the
chosen tracked avatar; no real-world camera feed or face capture is used.

## Validation

ExperiencePlayReview.Run checks natural welcome completion, intro gating,
replay/skip, the actual tablet collider and settings hit regions, all six audio
clips, avatar and face changes, reflection toggle, standing-height match,
captions, voice mute, guide visibility and station placement.
ExperienceReview.Visuals renders all four character variants, the isolated
robot and the leaned-over water view. CorrectionReview.Tablet checks navigation
and the two-step Most Holy Place entry. Android startup and performance are
recorded separately; short samples are not full-tour performance certification.
