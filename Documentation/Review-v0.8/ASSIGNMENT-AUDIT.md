# Portfolio Project 2 requirement check

Read from the signed-in course page on 4 November 2026:
https://vr26.vn.ugavel.com/PortfolioProject2/
Deadline: Friday 6 November 2026, 11:59 pm. Individual work.

## Demo 5: virtual human (35 points)

- Purpose: educational Tabernacle tour; narrated stations and concise Scripture-grounded AI questions.
- Quality virtual human: tracked human self-avatar and animated robot guide. The main conversational agent is a robot, not a human-looking guide. The rubric specifically requests a quality virtual human with motion capture, gaze, facial expression and lip sync; robot acceptance is not confirmed. Tracked avatar movement is live tracking, not a prerecorded motion-capture animation library.
- AI listens/talks: VelChat generated conversation, streamed audio; offline tour narration. Service checks passed previously; current headset microphone and network test outstanding.
- Speech and gestures both ways: live visitor questions, guide gestures; v0.8 adds deliberate wave/nod recognition and acknowledgement. Needs user testing for reliable recognition and false positives.
- Avatar driven by tracking: head/controllers; detailed girl and new boy; adult versions deferred at owner's instruction.
- Embodiment: visible robe/hands and avatar-only laver reflection.
- Appropriate locomotion: teleport, snap turn, comfort fade, viewing assistance.
- Usability: Unity checks cannot establish headset frame rate or comfort. Test on Quest and with another person, as required.

## Submission (5 of the 35 points)

- Required class bot `/accept portfolio-2` repository is `portfolio-2-<GitHub username>` with `Demo5` and `Demo6` folders. This separate `tabernacle-vr` repository is not yet verified as satisfying that structure. Owner previously asked to keep this project separate from the assignment repository; no assignment repository was modified.
- Release APK: v0.8.0 (9) built and verified. [v0.8 release](https://github.com/coo64362/tabernacle-vr/releases/tag/v0.8.0); use this candidate rather than the older v0.4 release.
- YouTube video: 3–6 minutes, user demonstrating it, headset view with the user in an inset. Not recorded here.
- Short README: purpose, controls, video and release links. A real video URL remains missing.
- AI usage: tools/models, rough prompt/session counts, what worked quickly and took retries, 3–5 real prompt examples. Include an honest count estimate from the owner; no invented counts.

## Demo 6: separate mixed-reality app (35 points)

This VR walkthrough does not satisfy Demo 6. Demo 6 needs meaningful use of real surfaces/objects, occlusion or a design around it, spatial anchors and room-fitting lighting/shadows/scale. Computer vision is a plus. No changes to a separate MR app were requested in this update.

## Graduate-only requirement (CSCI 6830)

If enrolled as a graduate student: compare two copresence conditions based on a published study, use a published or justified behavioral measure, at least five people trying both conditions with counterbalanced order, chart/analysis, and a `Demo5/Study` folder with data, analysis and one-page summary. Enrollment and study completion are not known. Do not fabricate participants or results.

Quiz: twelve multiple-choice questions based on the owner's demos, in class after the deadline.
