using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

public static class PrototypeReview {
 public static void Run(){
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var root=new GameObject("Review environment");var world=root.AddComponent<TabernacleWorld>();root.SendMessage("Awake");Physics.SyncTransforms();
  var objects=root.GetComponentsInChildren<Transform>();
  Require(objects.Count(t=>t.name.StartsWith("Bread loaf "))==12,"Exactly twelve bread loaves");
  Require(objects.Count(t=>t.name.StartsWith("Oil lamp ")&&t.name.EndsWith("of seven"))==7,"Exactly seven oil lamps");
  foreach(var name in new[]{"Ash pot","Shovel blade","Blood basin","Meat fork shaft","Firepan","Gold plate beneath bread","Gold cup","Gold pouring bowl","Gold pitcher","Gold wick tray"})Require(objects.Any(t=>t.name==name),"Inventory: "+name);
  foreach(var p in world.stops)Require(world.CanStand(p),"Tour stop unobstructed: "+p);
  Require(!world.CanStand(new Vector3(20,0,10)),"Cannot teleport outside courtyard width");Require(!world.CanStand(new Vector3(0,0,8)),"Cannot teleport into altar");Require(!world.CanStand(new Vector3(0,0,16.5f)),"Cannot teleport into basin");Require(!world.CanStand(new Vector3(1.5f,0,30)),"Cannot teleport into bread table");
  Require(objects.Count(t=>t.name=="Veil gold post")==4,"Four inner veil posts");
  var output=Path.GetFullPath("../output/tabernacle-vr/prototype-previews");Directory.CreateDirectory(output);
  var cam=new GameObject("Review camera").AddComponent<Camera>();cam.nearClipPlane=.07f;cam.farClipPlane=140;cam.fieldOfView=65;cam.allowHDR=false;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.70f,.75f,.76f);
  var rt=new RenderTexture(1600,1000,24){antiAliasing=4};cam.targetTexture=rt;
  Shot(cam,output,"welcome",new Vector3(-2.7f,1.65f,-4.5f),new Vector3(-2.7f,1.5f,-1.5f));
  Shot(cam,output,"courtyard",new Vector3(7,6,-8),new Vector3(0,1,23));
  Shot(cam,output,"holy-place",new Vector3(0,1.65f,27.5f),new Vector3(0,1.4f,33));
  Shot(cam,output,"twelve-loaves",new Vector3(.5f,1.35f,28.9f),new Vector3(1.5f,.98f,30));
  Shot(cam,output,"seven-lamps",new Vector3(-.4f,1.5f,28.7f),new Vector3(-1.4f,1.15f,30));
  Shot(cam,output,"altar-utensils",new Vector3(-4,1.5f,6),new Vector3(-2.6f,.15f,8));
  Shot(cam,output,"ark-chamber",new Vector3(0,1.4f,36.2f),new Vector3(0,.9f,38));
  int renderers=root.GetComponentsInChildren<MeshRenderer>().Count(r=>r.enabled);
  long triangles=root.GetComponentsInChildren<MeshFilter>().Sum(m=>(long)m.sharedMesh.triangles.Length/3);
  File.WriteAllText(Path.Combine(output,"review.txt"),$"PASS: biblical counts and listed inventory, six safe stops, boundary and furniture collision rejection, four veil posts.\nEnvironment renderers: {renderers}; triangles: {triangles}.\nThese desktop renders do not establish headset frame rate or stereoscopic correctness.\n");
  Debug.Log($"TABERNACLE_REVIEW_PASS renderers={renderers} triangles={triangles}");
  cam.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);
 }
 static void Require(bool ok,string message){if(!ok)throw new Exception("REVIEW FAILED: "+message);Debug.Log("REVIEW PASS: "+message);}
 static void Shot(Camera cam,string folder,string name,Vector3 position,Vector3 look){cam.transform.position=position;cam.transform.LookAt(look);cam.Render();RenderTexture.active=cam.targetTexture;var tex=new Texture2D(1600,1000,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,1000),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);RenderTexture.active=null;}
}
