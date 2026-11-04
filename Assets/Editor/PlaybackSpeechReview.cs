using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlaybackSpeechReview {
 static HumanSpeechFace face;static AudioSource source;static double started;static float peak,minWidth=1,maxWidth=1;
 static PlaybackSpeechReview(){if(SessionState.GetBool("PlaybackSpeechReview",false))EditorApplication.update+=Tick;}
 public static void Run(){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);SessionState.SetBool("PlaybackSpeechReview",true);EditorApplication.update+=Tick;EditorApplication.EnterPlaymode();}
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static void Tick(){if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;try{
  if(!face){
   AudioListener.volume=1;new GameObject("Silent playback review listener").AddComponent<AudioListener>();var go=new GameObject("Playback speech review");source=go.AddComponent<AudioSource>();source.playOnAwake=false;
   source.clip=Resources.Load<AudioClip>("Audio/Guide/welcome");Check(source.clip,"Welcome audio missing");source.clip.LoadAudioData();source.priority=0;
   face=go.AddComponent<HumanSpeechFace>();face.AnalyseWithoutShapes=true;face.BindAudio(source);source.Play();started=EditorApplication.timeSinceStartup;
  }
  double elapsed=EditorApplication.timeSinceStartup-started;face.ApplyPose();Check(float.IsFinite(face.Openness)&&float.IsFinite(face.Width),"Non-finite playback face");peak=Mathf.Max(peak,face.Openness);minWidth=Mathf.Min(minWidth,face.Width);maxWidth=Mathf.Max(maxWidth,face.Width);
  if(elapsed>source.clip.length+.5){Check(peak>.02f,"Actual AudioSource callbacks did not drive mouth; frames="+source.GetComponent<SpeechAudioTap>().ReceivedFrames+" sample peak="+source.GetComponent<SpeechAudioTap>().PeakSample);Check(maxWidth-minWidth>.02f,"Actual playback did not vary vowel width");Check(face.Openness<.01f,"Stopped playback did not close mouth");File.WriteAllText("Documentation/Review-v0.8/playback-check.txt","Actual AudioSource welcome playback → audio callback queue → local MFCC → digital mouth: PASS\nPeak opening: "+peak+"; width range: "+minWidth+"–"+maxWidth+"\nPlayback stop closes mouth: PASS\nPlayback reviewed with the normal audio graph. Quest latency and phoneme accuracy: NOT TESTED\n");Debug.Log("PLAYBACK_SPEECH_PASS peak="+peak);Finish(0);}
  if(elapsed>30)throw new Exception("Playback review timed out");
 }catch(Exception e){Debug.LogException(e);Finish(1);}}
 static void Finish(int code){SessionState.SetBool("PlaybackSpeechReview",false);EditorApplication.update-=Tick;AudioListener.volume=1;EditorApplication.Exit(code);}
}
