using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class ExperiencePlayReview {
 static TabernacleExperience experience;static ControllerTablet tablet;static Transform pointer;static double started;static int stage,visits;
 static readonly string[] Keys={"AvatarFemale","AvatarChild","AvatarStandingHeight","GuideMuted","GuideCaptions","GuideVisible","AvatarReflection","TurnAngle"};
 static ExperiencePlayReview(){if(SessionState.GetBool("ExperienceReviewRunning",false))EditorApplication.update+=Tick;}
 public static void Run(){
  EditorSceneManager.OpenScene("Assets/TabernacleRealistic.unity");var explorer=UnityEngine.Object.FindFirstObjectByType<QuestExplorer>();if(explorer)explorer.enabled=false;
  foreach(var key in Keys){SessionState.SetBool("ReviewHad"+key,PlayerPrefs.HasKey(key));if(key=="AvatarStandingHeight")SessionState.SetFloat("ReviewValue"+key,PlayerPrefs.GetFloat(key));else SessionState.SetInt("ReviewValue"+key,PlayerPrefs.GetInt(key));}
  SessionState.SetBool("ExperienceReviewRunning",true);EditorApplication.EnterPlaymode();
 }
 static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
 static void Tick(){
  if(!EditorApplication.isPlaying)return;
  try{
   if(stage==0){
    TabernacleWorld.Instance=UnityEngine.Object.FindFirstObjectByType<TabernacleWorld>();
    var rig=new GameObject("Review player origin").transform;var eye=new GameObject("Review head").AddComponent<Camera>();eye.transform.SetParent(rig,false);eye.transform.localPosition=new Vector3(0,1.65f,2);eye.cullingMask&=~(1<<11);
    var left=new GameObject("Review left").transform;left.SetParent(rig,false);left.localPosition=new Vector3(-.3f,1.1f,2.3f);var right=new GameObject("Review right").transform;right.SetParent(rig,false);right.localPosition=new Vector3(.3f,1.1f,2.3f);
    tablet=new GameObject("Review tablet").AddComponent<ControllerTablet>();tablet.Initialize(left);pointer=new GameObject("Review pointer").transform;tablet.VisitStation=i=>visits++;
    experience=new GameObject("Review experience").AddComponent<TabernacleExperience>();experience.Initialize(eye,left,right,tablet);
    Check(experience.IntroductionPlaying,"Welcome did not lock movement");
    Click(0,.205f);Click(0,.125f);Check(visits==0,"Tour selection bypassed welcome lock");
    foreach(var name in new[]{"welcome","altar","laver","holy-place","veil","ark"}){var clip=Resources.Load<AudioClip>("Audio/Guide/"+name);Check(clip&&clip.length>5,"Missing narration: "+name);}
    // Point through the actual scaled tablet collider, including all settings sections.
    Click(.134f,.205f);CaptureTablet("tablet-audio");Click(.1f,.123f);CaptureTablet("tablet-avatar");bool female=experience.Avatar.Female,child=experience.Avatar.ChildFace,reflection=experience.Reflection.Enabled;
    Click(0,.055f);Check(experience.Avatar.Female!=female,"Gender selection failed");
    Click(0,-.010f);Check(experience.Avatar.ChildFace!=child,"Face selection failed");
    Click(0,-.075f);Check(experience.Reflection.Enabled!=reflection,"Reflection selection failed");
    Click(0,-.140f);Check(Mathf.Abs(experience.Avatar.Height-1.65f)<.01f,"Height calibration changed viewpoint");
    Click(-.1f,.123f);bool muted=experience.GuideMuted,captions=experience.Captions;
    Click(0,-.075f);Check(experience.GuideMuted!=muted,"Guide mute failed");Click(0,-.140f);Check(experience.Captions!=captions,"Caption toggle failed");experience.SetGuideMuted(muted);experience.SetCaptions(captions);
    started=EditorApplication.timeSinceStartup;stage=1;
   }else if(stage==1){
    if(EditorApplication.timeSinceStartup-started>25)throw new Exception("Welcome did not finish and unlock movement");
    if(experience.IntroductionPlaying)return;
    experience.Replay();Check(experience.IntroductionPlaying,"Replay did not start welcome");
    Click(.1f,.123f);Click(.1f,.123f);CaptureTablet("tablet-comfort"); // Audio -> Avatar -> Comfort
    Click(0,-.140f);Check(!experience.IntroductionPlaying,"Skip welcome did not unlock movement");
    experience.SetGuideVisible(false);Check(experience.Guide.gameObject.activeInHierarchy,"Hiding the robot also deactivated its voice");experience.SetGuideVisible(true);
    experience.StationChanged(2);Check(!experience.IntroductionPlaying,"Normal station narration locked movement");
    Check(Vector3.Distance(experience.Guide.transform.position,new Vector3(1.45f,0,17.5f))<.02f,"Guide did not move to basin station");
    experience.Skip();Debug.Log("EXPERIENCE_PLAY_CHECK_PASS natural welcome completion, movement gate, skip, replay, all six audio clips, raycast avatar/face/reflection settings, height matching, guide mute, captions, station guide position");Finish(0);
   }
  }catch(Exception e){Debug.LogException(e);Finish(1);}
 }
 static void CaptureTablet(string name){
  if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
  var folder="../output/tabernacle-vr/experience-previews";System.IO.Directory.CreateDirectory(folder);var camera=new GameObject("Settings review camera").AddComponent<Camera>();camera.transform.position=tablet.transform.position-tablet.transform.forward*.75f;camera.transform.rotation=Quaternion.LookRotation(tablet.transform.forward,tablet.transform.up);camera.fieldOfView=52;camera.nearClipPlane=.05f;camera.cullingMask=~(1<<11);tablet.SendMessage("LateUpdate");ExperienceReview.Capture(camera,folder+"/"+name+".png",1000,1200);UnityEngine.Object.Destroy(camera.gameObject);
 }
 static void Click(float x,float y){var target=tablet.transform.TransformPoint(new Vector3(x,y,0));pointer.position=target-tablet.transform.forward*.7f;pointer.rotation=Quaternion.LookRotation(tablet.transform.forward);Physics.SyncTransforms();Check(tablet.Point(pointer,false,out var hit),"Pointer missed scaled tablet");tablet.Point(pointer,true,out hit);tablet.Point(pointer,false,out hit);}
 static void Finish(int result){
  EditorApplication.update-=Tick;SessionState.SetBool("ExperienceReviewRunning",false);
  foreach(var key in Keys){if(!SessionState.GetBool("ReviewHad"+key,false))PlayerPrefs.DeleteKey(key);else if(key=="AvatarStandingHeight")PlayerPrefs.SetFloat(key,SessionState.GetFloat("ReviewValue"+key,1.65f));else PlayerPrefs.SetInt(key,SessionState.GetInt("ReviewValue"+key,0));}PlayerPrefs.Save();
  EditorApplication.Exit(result);
 }
}
