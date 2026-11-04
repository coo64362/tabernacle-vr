# AI usage disclosure

This was one extended development chat over multiple days, with over 100 requests and refinements visible in the supplied conversation. This is a rough count of the conversation, not an exact count of model/API calls. Individual image-generation and service calls were not counted separately.

Tools: Codex for Unity/C#, build tooling, checks and documentation; built-in image generation for the character references and visual materials; Microsoft TRELLIS.2 for the boy, girl and robot source meshes; Blender for skinning, geometry and mouth shapes; local uLipSync for audio-derived vowel estimates; VelChat for generated in-experience questions and streaming speech. Meshy adult models were explored but are not included in this version. Adult integration was deferred by the owner.

The environment layout, controls and offline narration were relatively straightforward to prototype. Matching the approved character art took repeated image and mesh iterations. Rigging hands, preserving the face, keeping long robes from stretching, and placing mouth surfaces behind the lips required visual review and corrections. The guide's live speech is generated; its tour narration is reviewed and stored. Citations are retrieved from Scripture and checked for valid verse numbers, but generated interpretations still need judgment.

Five actual user prompt examples from this project's conversation:

1. “make the settings tablet smaller, make there borders between instructions, tour and .. ther other thing”
2. “the face you put ontop should match the generated image dont be lazy”
3. “rig her and the robot perferctly, i still think the instrution tablets are too big. so do that and connect v elchat, we should be able to ask question to the robot guide but it must only be about the tabernacle”
4. “if we are giving hard coded anwers instead of letting the ai act like a human, then we arent doing well. the hardcoded answers would only be for the tour, and then whenever a question is asked the ai should generate quick, because any question can be asked.”
5. “I HAVENT GOT THE ADULTS DO EVERYTHING ELSE”

Limitations disclosed: vowel estimation is not exact consonant/word alignment; the digital guide is a robot; adult detailed models are not included; no current Quest test or participant playtest is claimed. No participant results or headset performance numbers were generated.
