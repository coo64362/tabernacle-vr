"""Generate original British male narration using Kokoro bm_daniel (Apache-2.0).
Install kokoro-onnx and soundfile in an isolated environment. Supply the official
kokoro-v1.0.onnx and voices-v1.0.bin paths. Models are never bundled with the APK.
"""
import argparse,json,pathlib
import numpy as np
import soundfile as sf
from kokoro_onnx import Kokoro
parser=argparse.ArgumentParser();parser.add_argument('model');parser.add_argument('voices');args=parser.parse_args()
root=pathlib.Path(__file__).resolve().parents[1]
k=Kokoro(args.model,args.voices)
for item in json.loads((root/'Documentation/Guide/narration.json').read_text()):
 out=root/'Assets/Resources/Audio/Guide'/f"{item['name']}.wav";out.parent.mkdir(parents=True,exist_ok=True)
 if out.exists(): print('Existing',out.name,flush=True);continue
 samples,rate=k.create(item['text'],voice='bm_daniel',speed=.98,lang='en-gb')
 peak=np.max(np.abs(samples));samples=samples*(.85/max(.85,peak))
 sf.write(out,samples,rate,subtype='PCM_16');print(out.name,round(len(samples)/rate,2),'seconds',flush=True)
