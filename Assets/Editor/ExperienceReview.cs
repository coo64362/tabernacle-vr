using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ExperienceReview {
 public static void Build(){CorrectionReview.Tablet();Visuals();QuestBuild.BuildAndReview();}
 public static void Visuals(){
  EditorSceneManager.OpenScene("Assets/TabernacleRealistic.unity");TabernacleWorld.Instance=UnityEngine.Object.FindFirstObjectByType<TabernacleWorld>();
  var output="../output/tabernacle-vr/experience-previews";Directory.CreateDirectory(output);
  var eye=new GameObject("Review tracked eye").AddComponent<Camera>();eye.transform.position=new Vector3(0,1.65f,15.55f);eye.transform.rotation=Quaternion.LookRotation(new Vector3(0,1.017f,16.5f)-eye.transform.position);eye.nearClipPlane=.05f;eye.farClipPlane=100;
  var left=new GameObject("Review left hand").transform;left.position=new Vector3(-.3f,1.12f,15.85f);var right=new GameObject("Review right hand").transform;right.position=new Vector3(.3f,1.12f,15.85f);
  var avatar=new GameObject("Review pilgrim").AddComponent<PilgrimAvatar>();avatar.Initialize(eye.transform,left,right);avatar.Calibrate();avatar.UpdatePose();
  var camera=new GameObject("Portrait camera").AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.12f,.15f,.17f);camera.fieldOfView=40;camera.nearClipPlane=.05f;camera.farClipPlane=4;camera.cullingMask=1<<11;
  camera.transform.position=new Vector3(0,1.02f,18.7f);camera.transform.rotation=Quaternion.LookRotation(new Vector3(0,1.02f,15.55f)-camera.transform.position);
  bool female=avatar.Female,child=avatar.ChildFace;
  for(int i=0;i<4;i++){avatar.SetFemale(i>=2);avatar.SetChild(i%2==0);eye.transform.rotation=Quaternion.identity;avatar.UpdatePose();Capture(camera,output+"/"+(i>=2?"female":"male")+"-"+(i%2==0?"child":"adult")+".png",800,1000);}
  avatar.SetFemale(female);avatar.SetChild(child);eye.transform.position=new Vector3(0,1.65f,16.3f);eye.transform.rotation=Quaternion.Euler(84,0,0);left.position+=Vector3.forward*.75f;right.position+=Vector3.forward*.75f;avatar.UpdatePose();
  foreach(var renderer in avatar.GetComponentsInChildren<Renderer>())if(renderer.gameObject.layer!=11)renderer.enabled=false;
  var reflection=new GameObject("Review water reflection").AddComponent<LaverReflection>();reflection.Initialize(eye);reflection.RenderIfNeeded();Capture(eye,output+"/laver-reflection.png",1000,1000);
  var saved=RenderTexture.active;RenderTexture.active=reflection.ReflectionTexture;var raw=new Texture2D(reflection.ReflectionTexture.width,reflection.ReflectionTexture.height,TextureFormat.RGB24,false);raw.ReadPixels(new Rect(0,0,reflection.ReflectionTexture.width,reflection.ReflectionTexture.height),0,0);raw.Apply();File.WriteAllBytes(output+"/reflection-buffer.png",raw.EncodeToPNG());RenderTexture.active=saved;UnityEngine.Object.DestroyImmediate(raw);
  var guide=new GameObject("Review robot guide").AddComponent<RobotGuide>();guide.Initialize(eye.transform);Physics.SyncTransforms();for(int i=0;i<6;i++){guide.SetStation(i);if(!TabernacleWorld.Instance.CanStand(guide.transform.position))throw new Exception("Guide position overlaps a furnishing at station "+i);}
  guide.transform.position=new Vector3(0,0,15.55f);guide.transform.rotation=Quaternion.identity;avatar.gameObject.SetActive(false);foreach(var r in guide.GetComponentsInChildren<Renderer>())r.gameObject.layer=14;
  camera.cullingMask=1<<14;camera.transform.position=new Vector3(0,1.0f,18.5f);camera.transform.rotation=Quaternion.LookRotation(new Vector3(0,.9f,15.55f)-camera.transform.position);Capture(camera,output+"/robot-guide.png",800,1000);
  Debug.Log("EXPERIENCE_VISUALS_PASS four avatar choices, robot guide, avatar-only water reflection");
 }
 public static void Capture(Camera camera,string path,int width,int height){
  var previous=camera.targetTexture;var rt=new RenderTexture(width,height,24);camera.targetTexture=rt;camera.Render();var active=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());RenderTexture.active=active;camera.targetTexture=previous;UnityEngine.Object.DestroyImmediate(image);rt.Release();UnityEngine.Object.DestroyImmediate(rt);
 }
}
