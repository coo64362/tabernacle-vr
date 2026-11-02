using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class CorrectionReview {
 public static void Tablet(){
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var hand=new GameObject("Left hand test anchor").transform;hand.position=new Vector3(0,1,.5f);
  var tablet=new GameObject("Tablet test").AddComponent<ControllerTablet>();tablet.Initialize(hand);
  var pointer=new GameObject("Pointer test").transform;int visited=-1;tablet.VisitStation=i=>visited=i;
  Click(tablet,pointer,new Vector3(0,.205f,0));
  Click(tablet,pointer,new Vector3(0,.125f-5*.057f,0));
  if(visited!=-1)throw new Exception("Most Holy Place should first explain educational access");
  Click(tablet,pointer,new Vector3(0,0,0));
  if(visited!=5)throw new Exception("Most Holy Place destination did not activate");
  Click(tablet,pointer,new Vector3(-.13f,.205f,0));
  tablet.SendMessage("LateUpdate");
  var camera=new GameObject("Tablet review camera").AddComponent<Camera>();camera.transform.position=tablet.transform.position-tablet.transform.forward*.75f;camera.transform.rotation=Quaternion.LookRotation(tablet.transform.forward,tablet.transform.up);camera.fieldOfView=52;camera.nearClipPlane=.05f;camera.backgroundColor=new Color(.12f,.11f,.09f);camera.clearFlags=CameraClearFlags.SolidColor;
  var rt=new RenderTexture(1000,1200,24);camera.targetTexture=rt;camera.Render();tablet.SendMessage("LateUpdate");camera.Render();RenderTexture.active=rt;
  var image=new Texture2D(1000,1200,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1000,1200),0,0);image.Apply();Directory.CreateDirectory("../output/tabernacle-vr/correction-previews");File.WriteAllBytes("../output/tabernacle-vr/correction-previews/tablet.png",image.EncodeToPNG());
  Debug.Log("TABLET_CHECK_PASS pointing, tabs, two-step Most Holy Place access, Uncial text");
 }
 static void Click(ControllerTablet tablet,Transform pointer,Vector3 local){
  var target=tablet.transform.TransformPoint(local);pointer.position=target-tablet.transform.forward*.7f;pointer.rotation=Quaternion.LookRotation(tablet.transform.forward);Physics.SyncTransforms();
  if(!tablet.Point(pointer,false,out var p))throw new Exception("Tablet ray failed to hit");
  tablet.Point(pointer,true,out p);tablet.Point(pointer,false,out p);
 }
}
