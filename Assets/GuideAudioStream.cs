using System;
using UnityEngine;

// A bounded PCM ring consumes received Opus frames immediately with a 120 ms jitter buffer.
// No whole-answer or whole-sentence wait. Unity's audio callback performs no allocation.
public sealed class GuideAudioStream : MonoBehaviour {
 const int Rate=24000;readonly float[] ring=new float[Rate*100];readonly object gate=new object();long written,read;int head,tail,used;AudioSource source;AudioClip clip;float drainedAt;bool started;
 public long SamplesRead {get{lock(gate)return read;}}public long SamplesWritten {get{lock(gate)return written;}}
 public bool Started=>started;public bool Finished {get{lock(gate)return started&&used==0&&Time.realtimeSinceStartup-drainedAt>.2f;}}
 public void Initialize(AudioSource voice){source=voice;clip=AudioClip.Create("Live guide speech",Rate,1,Rate,true,Read);source.clip=clip;source.loop=true;}
 public bool Enqueue(float[] samples,int count){lock(gate){if(count>ring.Length-used)return false;for(int i=0;i<count;i++){ring[tail]=samples[i];tail=(tail+1)%ring.Length;}used+=count;written+=count;}return true;}
 public void Tick(bool complete){if(!started){int available;lock(gate)available=used;if(available>=2880||complete&&available>0){started=true;source.Play();}}lock(gate){if(used>0)drainedAt=Time.realtimeSinceStartup;}}
 void Read(float[] data){lock(gate){int n=Math.Min(data.Length,used);for(int i=0;i<n;i++){data[i]=ring[head];head=(head+1)%ring.Length;}Array.Clear(data,n,data.Length-n);used-=n;read+=n;}}
 public void ResetStream(){if(source)source.Stop();lock(gate){head=tail=used=0;written=read=0;}started=false;drainedAt=Time.realtimeSinceStartup;}
 void OnDestroy(){if(source)source.Stop();if(clip)Destroy(clip);}
}
