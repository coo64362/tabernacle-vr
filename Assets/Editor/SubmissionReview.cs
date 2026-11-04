using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class SubmissionReview {
 static void Check(bool condition,string message){if(!condition)throw new Exception(message);Debug.Log("SUBMISSION_CHECK_PASS "+message);}
 public static void Build(){Prepare();ConversationReview.Run();ExperienceReview.Visuals();QuestBuild.Build();}
 public static void Prepare(){
  CharacterRigImport.Run();Directory.CreateDirectory("Documentation/Review-v0.8");
  foreach(string name in new[]{"MaleChild","FemaleChild"}){
   var prefab=Resources.Load<GameObject>("Characters/"+name+"Ready");Check(prefab,"Detailed prefab present: "+name);var skins=prefab.GetComponentsInChildren<SkinnedMeshRenderer>();Check(skins.Any(s=>s.sharedMesh.blendShapeCount>=6),name+" speech shapes");
   Check(prefab.GetComponentsInChildren<Transform>(true).Count(t=>t.name.Contains("Index")||t.name.Contains("Middle")||t.name.Contains("Ring")||t.name.Contains("Little")||t.name.Contains("Thumb"))>=30,name+" finger chains");
   foreach(var skin in skins){Check(skin.sharedMesh.boneWeights.All(w=>Mathf.Abs(w.weight0+w.weight1+w.weight2+w.weight3-1)<.01f),name+" normalized vertex weights");Check(skin.sharedMaterials.All(m=>m),name+" assigned materials");}
   var first=Resources.Load<GameObject>("Characters/"+name+"FirstPerson");Check(first,"First-person prefab present: "+name);Check(first.GetComponentsInChildren<SkinnedMeshRenderer>().All(s=>s.sharedMesh.blendShapeCount==0),name+" first-person strips unused mouth shapes");
  }
  AudioQueue();Gestures();
  File.WriteAllText("Documentation/Review-v0.8/structural-check.txt","Boy/girl imported skins, six speech morphs, finger chains, normalized weights, materials, headless first-person meshes: PASS\nPlayback queue: stereo downmix, actual frame count, overflow bound and stop clearing: PASS\nVisitor gestures: wave, nod, cooldown, speech suppression and missing-tracking suppression: PASS\nHeadset appearance, gesture reliability and performance: NOT TESTED\n");
  Debug.Log("SUBMISSION_PREPARE_PASS");
 }
 static void AudioQueue(){
  var go=new GameObject("Audio queue review");try{
   var tap=go.AddComponent<SpeechAudioTap>();typeof(SpeechAudioTap).GetMethod("Awake",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(tap,null);
   var feed=typeof(SpeechAudioTap).GetMethod("OnAudioFilterRead",BindingFlags.Instance|BindingFlags.NonPublic);var output=new float[2048];tap.SetAccepting(true);
   feed.Invoke(tap,new object[]{new float[]{.2f,.6f,-.3f,.1f},2});Check(tap.TryRead(output,out int count)&&count==2&&Mathf.Abs(output[0]-.4f)<.0001f&&Mathf.Abs(output[1]+.1f)<.0001f,"Playback stereo downmix preserves actual frame count");Check(!tap.TryRead(output,out _),"Playback frames consumed once");
   for(int i=0;i<12;i++)feed.Invoke(tap,new object[]{new float[]{i},1});int n=0;while(tap.TryRead(output,out _)){if(n==0)Check(output[0]==4,"Audio backlog drops oldest frames");n++;}Check(n==8,"Audio backlog bounded to eight blocks");
   feed.Invoke(tap,new object[]{new float[]{1},1});tap.SetAccepting(false);Check(!tap.TryRead(output,out _),"Stop clears pending audio");feed.Invoke(tap,new object[]{new float[]{1},1});Check(!tap.TryRead(output,out _),"Stopped audio ignored");
  }finally{UnityEngine.Object.DestroyImmediate(go);}
 }
 static void Gestures(){
  var go=new GameObject("Gesture review");var robot=new GameObject("Gesture robot");try{
   var experience=go.AddComponent<TabernacleExperience>();var guide=robot.AddComponent<RobotGuide>();robot.transform.position=new Vector3(0,0,2);typeof(TabernacleExperience).GetField("<Guide>k__BackingField",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(experience,guide);var gestures=go.AddComponent<GuideVisitorGestures>();gestures.Initialize(experience,null,null,null);
   Vector3 head=new Vector3(0,1.65f,0);float t=10;
   Action<bool,bool> wave=(tracked,busy)=>{guide.Speaking=busy;for(int i=0;i<45;i++,t+=.05f)gestures.Observe(t,head,Quaternion.identity,new Vector3(-.3f,1,0),new Vector3(.3f+Mathf.Sin(i*.65f)*.09f,1.65f,.2f),tracked);};
   wave(false,false);Check(gestures.Responses==0,"Missing tracking cannot trigger a wave");wave(true,true);Check(gestures.Responses==0,"Gesture cannot interrupt guide speech");wave(true,false);Check(gestures.Responses==1&&guide.Gesture=="wave","Deliberate wave acknowledged once");wave(true,false);Check(gestures.Responses==1,"Gesture cooldown prevents repeated replies");
   t+=6;for(int i=0;i<35;i++,t+=.05f)gestures.Observe(t,head,Quaternion.Euler(Mathf.Sin(i*.4f)*12,0,0),new Vector3(-.3f,1,0),new Vector3(.3f,1,0),true);Check(gestures.Responses==2&&guide.Gesture=="nod","Deliberate nod acknowledged");
  }finally{UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(robot);}
 }
}
