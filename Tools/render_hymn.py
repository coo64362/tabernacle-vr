"""Original synthesized instrumental; melody from the credited public-domain score."""
from pathlib import Path
import xml.etree.ElementTree as ET
import numpy as np
import wave, json, sys
score=Path(sys.argv[1]); out=Path(sys.argv[2]); out.parent.mkdir(parents=True,exist_ok=True)
root=ET.parse(score).getroot(); divisions=256; position=0.; melody=[]; bars=[]
semitones={'C':0,'D':2,'E':4,'F':5,'G':7,'A':9,'B':11}
for measure in root.find('part').findall('measure'):
    cursor=0.; previous=0.; length=0.; groups={}
    for item in measure:
        if item.tag=='attributes': divisions=int(item.findtext('divisions',str(divisions)))
        elif item.tag=='backup': cursor-=int(item.findtext('duration'))/divisions
        elif item.tag=='forward': cursor+=int(item.findtext('duration'))/divisions
        elif item.tag=='note':
            duration=int(item.findtext('duration','0'))/divisions
            chord=item.find('chord') is not None
            onset=previous if chord else cursor
            if not chord: previous=cursor; cursor+=duration
            length=max(length,cursor)
            pitch=item.find('pitch')
            if pitch is not None:
                midi=12*(int(pitch.findtext('octave'))+1)+semitones[pitch.findtext('step')]+int(pitch.findtext('alter','0'))
                if onset not in groups or midi>groups[onset][0]: groups[onset]=(midi,duration,{t.get('type') for t in item.findall('tie')})
    for onset,(midi,dur,ties) in sorted(groups.items()):
        if 'stop' in ties and melody and melody[-1][2]==midi and abs(melody[-1][0]+melody[-1][1]-position-onset)<.01:
            melody[-1]=(melody[-1][0],melody[-1][1]+dur,midi)
        else: melody.append((position+onset,dur,midi))
    bars.append((position,length)); position+=length
rate=32000; beat=60/66; total=(position+4)*beat
mix=np.zeros((int(total*rate),2),dtype=np.float64)
def note(start,duration,midi,amp,pan=0):
    f=440*2**((midi-69)/12); seconds=duration*beat
    t=np.arange(int((seconds+1.6)*rate))/rate
    attack=1-np.exp(-t/.025)
    release=np.exp(-np.maximum(0,t-seconds)/.42)
    env=attack*np.exp(-t/2.7)*release
    tone=sum(a*np.sin(2*np.pi*f*k*t) for k,a in [(1,1),(2,.22),(3,.075),(4,.025)])
    signal=tone*env*amp
    i=int((start*beat+.3)*rate); n=min(len(signal),len(mix)-i)
    mix[i:i+n,0]+=signal[:n]*np.sqrt((1-pan)/2)
    mix[i:i+n,1]+=signal[:n]*np.sqrt((1+pan)/2)
for start,dur,midi in melody: note(start,dur*.94,midi,.25,-.1)
# Original sparse broken-triad accompaniment; choose diatonic harmony from melody tones.
chords=[[49,53,56],[54,58,61],[44,48,51],[46,49,53],[53,56,60],[51,54,58]]
tonic=(7*int(root.findtext(".//key/fifths","0")))%12
shift=(tonic-1+6)%12-6
chords=[[n+shift for n in c] for c in chords]
last=0
for j,(start,length) in enumerate(bars):
    pitches=[(p%12,d) for s,d,p in melody if start<=s<start+length]
    costs=[sum(d*(0 if p in {n%12 for n in c} else 1) for p,d in pitches)+(.18 if k!=last else .3) for k,c in enumerate(chords)]
    k=0 if j in (0,len(bars)-1) else int(np.argmin(costs)); last=k; c=chords[k]
    note(start,length,c[0]-12,.08,-.3)
    for b in range(int(np.ceil(length))): note(start+b,min(1.8,length-b),c[b%3],.055,.3)
# Short stereo room reflections using only this original synthesis.
original=mix.copy()
for delay,gain in [(.083,.14),(.147,.10),(.233,.07),(.371,.04)]:
    d=int(delay*rate); mix[d:]+=original[:-d,::-1]*gain
mix*=.68/max(1e-9,np.max(np.abs(mix)))
fade=int(.5*rate); mix[:fade]*=np.linspace(0,1,fade)[:,None]
fade=int(2*rate); mix[-fade:]*=np.linspace(1,0,fade)[:,None]
with wave.open(str(out),'wb') as w:
    w.setnchannels(2); w.setsampwidth(2); w.setframerate(rate); w.writeframes((mix*32767).astype('<i2').tobytes())
print(json.dumps({'duration_seconds':round(total,2),'melody_notes':len(melody),'measures':len(bars),'peak':float(np.max(np.abs(mix))),'rms':float(np.sqrt(np.mean(mix**2))),'file':str(out)}))
