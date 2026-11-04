using System;
using System.Collections.Generic;
using UnityEngine;
using uLipSync;

// Local phoneme estimation, shared by microphone reflections and future speaking humans.
// Speech is analysed, never played back, when fed by the question microphone.
[DefaultExecutionOrder(80)]
public sealed class HumanSpeechFace : MonoBehaviour {
 static readonly string[] Sounds={"A","I","U","E","O","N"};
 sealed class Face {public SkinnedMeshRenderer skin;public int[] shapes;}
 readonly List<Face> faces=new List<Face>();readonly float[] weights=new float[6],targets=new float[6];
 uLipSync.uLipSync analyser;AudioSource source;readonly float[] playbackSamples=new float[2048];float[] resampled;SpeechAudioTap tap;
 public bool AnalyseWithoutShapes {get;set;}
 public float Openness {get;private set;}
 public float Width {get;private set;}=1;
 float lastInput=-100,level;bool capture;int rate,lastPoseFrame=-1;
 public bool HasSpeechShapes=>faces.Count>0;
 public void Initialize(){
  if(analyser)return;faces.Clear();
  foreach(var skin in GetComponentsInChildren<SkinnedMeshRenderer>()){
   var mesh=skin.sharedMesh;if(!mesh)continue;var indices=new int[6];bool valid=true;
   for(int k=0;k<6;k++){indices[k]=-1;for(int i=0;i<mesh.blendShapeCount;i++)if(mesh.GetBlendShapeName(i).EndsWith("Viseme_"+Sounds[k],StringComparison.Ordinal))indices[k]=i;valid&=indices[k]>=0;}
   if(valid)faces.Add(new Face{skin=skin,shapes=indices});
  }
  if(!HasSpeechShapes&&!AnalyseWithoutShapes)return;
  // Set the profile before enabling the analyser, so its native buffers have the right size.
  var host=new GameObject("Local speech analysis");host.transform.SetParent(transform,false);host.SetActive(false);
  analyser=host.AddComponent<uLipSync.uLipSync>();analyser.profile=Resources.Load<Profile>("Speech/VisitorProfile");
  if(!analyser.profile){Destroy(host);analyser=null;Debug.LogError("Human speech profile missing");return;}
  analyser.onLipSyncUpdate.AddListener(Receive);analyser.enabled=false;host.SetActive(true);
 }
 public void BeginCapture(){Initialize();ReleaseTap();source=null;capture=true;Clear();if(analyser)analyser.enabled=true;}
 public void EndCapture(){capture=false;Clear();if(analyser)analyser.enabled=false;}
 public void BindAudio(AudioSource speech){Initialize();ReleaseTap();capture=false;source=speech;Clear();if(analyser)analyser.enabled=speech;if(source){tap=source.GetComponent<SpeechAudioTap>();if(!tap)tap=source.gameObject.AddComponent<SpeechAudioTap>();tap.SetAccepting(true);}}
 void ReleaseTap(){if(tap)tap.SetAccepting(false);tap=null;}
 public void Feed(float[] mono,int sampleRate){
  if(!capture||!analyser||mono==null||mono.Length==0||sampleRate<8000)return;
  FeedInternal(mono,sampleRate,mono.Length);
 }
 void FeedInternal(float[] mono,int sampleRate,int count){
  rate=AudioSettings.outputSampleRate;
  int length=Mathf.RoundToInt(count*(float)rate/sampleRate);if(length<1)return;
  if(resampled==null||resampled.Length!=length)resampled=new float[length];
  double energy=0;for(int i=0;i<count;i++)energy+=mono[i]*mono[i];level=(float)Math.Sqrt(energy/count);
  for(int i=0;i<length;i++){float p=i*(float)sampleRate/rate;int a=Math.Min((int)p,count-1),b=Math.Min(a+1,count-1);resampled[i]=Mathf.Lerp(mono[a],mono[b],p-a);}
  analyser.OnDataReceived(resampled,1);lastInput=Time.realtimeSinceStartup;
 }
 void Update(){
  if(tap){bool playing=source&&source.isPlaying&&!source.mute;tap.SetAccepting(playing);if(!playing){lastInput=-100;Array.Clear(targets,0,6);}if(playing)for(int i=0;i<8&&tap.TryRead(playbackSamples,out int count);i++)FeedInternal(playbackSamples,AudioSettings.outputSampleRate,count);}
 }
 void Receive(LipSyncInfo info){
  Array.Clear(targets,0,targets.Length);
  if(Time.realtimeSinceStartup-lastInput>.15f||level<.004f||info.phonemeRatios==null)return;
  // The profile estimates vowels, not exact consonants or word timings. Blend the two
  // strongest estimates for smooth coarticulation; use real signal energy for closure.
  float volume=Mathf.Clamp01((level-.004f)/.035f);int first=-1,second=-1;float best=0,next=0;
  for(int i=0;i<6;i++){float v=info.phonemeRatios.TryGetValue(Sounds[i],out var r)?r:0;if(float.IsNaN(v)||float.IsInfinity(v))continue;if(v>best){second=first;next=best;first=i;best=v;}else if(v>next){second=i;next=v;}}
  if(first<0)return;if(info.phonemeRatios.TryGetValue("-",out float noise)&&noise>best){targets[5]=volume;return;}float total=best+next;volume*=Mathf.Clamp01(total*1.5f);if(total<.00001f)return;targets[first]=volume*best/total;if(second>=0)targets[second]=volume*next/total;
 }
 // Called immediately before the laver's reflection render, after the body pose.
 void LateUpdate()=>ApplyPose();
 public void ApplyPose(){
  if(lastPoseFrame==Time.frameCount)return;lastPoseFrame=Time.frameCount;
  if(Time.realtimeSinceStartup-lastInput>.15f||(!capture&&(!source||!source.isPlaying)))Array.Clear(targets,0,6);
  float blend=1-Mathf.Exp(-Time.unscaledDeltaTime/ .045f);
  float sum=0;for(int i=0;i<6;i++){weights[i]=Mathf.Lerp(weights[i],targets[i],blend);sum+=weights[i];foreach(var face in faces)if(face.skin)face.skin.SetBlendShapeWeight(face.shapes[i],weights[i]*100);}
  Openness=Mathf.Clamp01(weights[0]+weights[1]*.4f+weights[2]*.8f+weights[3]*.52f+weights[4]+weights[5]*.08f);
  Width=sum>.001f?Mathf.Lerp(1,(weights[0]+weights[1]*1.16f+weights[2]*.62f+weights[3]*1.13f+weights[4]*.7f+weights[5])/sum,Mathf.Clamp01(sum*3)):1;
 }
 void Clear(){Openness=0;Width=1;lastInput=-100;level=0;Array.Clear(targets,0,6);Array.Clear(weights,0,6);foreach(var face in faces)if(face.skin)foreach(int index in face.shapes)face.skin.SetBlendShapeWeight(index,0);}
 void OnDisable(){ReleaseTap();capture=false;source=null;Clear();if(analyser)analyser.enabled=false;}
}
