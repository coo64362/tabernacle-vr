using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class GuideIntegrationReview {
 static void Check(bool value,string message){if(!value)throw new Exception(message);}
 public static void ImportAndRun(){CharacterRigImport.Run();PlayerSettings.gpuSkinning=true;PlayerSettings.Android.forceInternetPermission=true;PlayerSettings.bundleVersion="0.6.0";PlayerSettings.Android.bundleVersionCode=7;AssetDatabase.SaveAssets();Run();}
 public static void Run(){
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);string folder="Documentation/Review-v0.6";Directory.CreateDirectory(folder);
  foreach(string name in new[]{"SandstoneGuideReady","FemaleChildReady"}){
   var prefab=Resources.Load<GameObject>("Characters/"+name);Check(prefab,"Missing rigged prefab: "+name);var root=UnityEngine.Object.Instantiate(prefab);var skins=root.GetComponentsInChildren<SkinnedMeshRenderer>();Check(skins.Length>0,"Missing skinned mesh");int bones=skins.Max(s=>s.bones.Length);Check(RiggedGuideVisual.Find(root.transform,"LeftIndex1")&&RiggedGuideVisual.Find(root.transform,"RightThumb3"),"Finger chains missing: "+name);
   foreach(var skin in skins){Check(skin.sharedMaterials.All(m=>m&&m.mainTexture),"Missing character texture reference: "+name);foreach(var w in skin.sharedMesh.boneWeights)Check(Mathf.Abs(w.weight0+w.weight1+w.weight2+w.weight3-1)<.002f,"Unnormalized skin weights");var baked=new Mesh();skin.BakeMesh(baked);Check(baked.vertexCount>10,"Empty baked skin");foreach(var v in baked.vertices)Check(float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z),"Non-finite rig pose");UnityEngine.Object.DestroyImmediate(baked);}
   var b=skins[0].bounds;foreach(var r in root.GetComponentsInChildren<Renderer>())b.Encapsulate(r.bounds);var camera=new GameObject("Rig review camera").AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.15f,.16f,.18f);camera.orthographic=true;camera.orthographicSize=Mathf.Max(.9f,b.size.y*.58f);camera.transform.position=new Vector3(0,b.center.y,4);camera.transform.rotation=Quaternion.Euler(0,180,0);if(SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null)ExperienceReview.Capture(camera,folder+"/"+name+".png",700,1000);
   Debug.Log("RIG_REVIEW "+name+" bones="+bones+" triangles="+skins.Sum(s=>s.sharedMesh.triangles.Length/3));UnityEngine.Object.DestroyImmediate(camera.gameObject);UnityEngine.Object.DestroyImmediate(root);
  }
  var hand=new GameObject("Tablet hand").transform;var tablet=new GameObject("Tablet test").AddComponent<ControllerTablet>();tablet.Initialize(hand);Physics.SyncTransforms();var slab=tablet.GetComponentsInChildren<Collider>().First(c=>c.name=="Stone instruction tablet");var scaled=Vector3.Scale(slab.transform.localScale,slab.transform.lossyScale);Check(Mathf.Abs(tablet.transform.localScale.x-.5f)<.001,"Tablet was not reduced");
  var pointer=new GameObject("Tablet ray").transform;pointer.position=tablet.transform.position-tablet.transform.forward*.7f;pointer.rotation=Quaternion.LookRotation(tablet.transform.forward);Check(tablet.Point(pointer,false,out var hit),"Smaller tablet cannot be selected");
  ConversationReview.Run();
  var encoder=new Concentus.Structs.OpusEncoder(16000,1,Concentus.Enums.OpusApplication.OPUS_APPLICATION_VOIP);var input=new float[640];for(int i=0;i<input.Length;i++)input[i]=Mathf.Sin(i*.1f)*.2f;var packet=new byte[4000];int count=encoder.Encode(input,0,640,packet,0,packet.Length);var decoder=new Concentus.Structs.OpusDecoder(16000,1);var output=new float[640];Check(count>0&&decoder.Decode(packet,0,count,output,0,640)==640,"Microphone Opus codec round trip failed");
  File.WriteAllText(folder+"/verification.txt","Skinned prefabs: pass\nFinger chains: pass\nFinite baked rest poses: pass\nTablet scale 0.5: pass\nTablet ray selection: pass\nGenerated Scripture-grounded answers: pass\nQuest hardware test: not run\n");Debug.Log("GUIDE_INTEGRATION_REVIEW_PASS");
 }
}
