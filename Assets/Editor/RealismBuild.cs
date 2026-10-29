using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Art and lighting are authored into the scene, rather than reconstructed unlit on the headset.
public static class RealismBuild {
 const string Generated="Assets/RealisticArt";
 static double started,nextReport;
 static bool baking;
 static bool buildAfter;
 public static void PrepareAndReview(){Prepare(false);}
 public static void PrepareBuildAndReview(){Prepare(true);}
 static void Prepare(bool build){
  buildAfter=build;
  foreach(var path in new[]{"Assets/Resources/Art/Gold.png","Assets/Resources/Art/Bronze.png","Assets/Resources/Art/Linen.png","Assets/Resources/Art/Sand.png","Assets/Resources/Art/InnerVeil.png"}){
   var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)continue;importer.maxTextureSize=2048;importer.mipmapEnabled=true;importer.anisoLevel=8;importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=path.Contains("InnerVeil")?TextureWrapMode.Clamp:TextureWrapMode.Repeat;importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();
  }
  if(AssetDatabase.IsValidFolder(Generated))AssetDatabase.DeleteAsset(Generated);Directory.CreateDirectory(Generated);AssetDatabase.Refresh();
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var root=new GameObject("Tabernacle — realistic materials and baked lighting");var world=root.AddComponent<TabernacleWorld>();root.SendMessage("Awake");world.prebuilt=true;
  var processed=new HashSet<Mesh>();int number=0;
  foreach(var filter in root.GetComponentsInChildren<MeshFilter>()){
   var mesh=filter.sharedMesh;if(!mesh||!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(mesh))||!processed.Add(mesh))continue;
   foreach(var vertex in mesh.vertices)if(float.IsNaN(vertex.x)||float.IsNaN(vertex.y)||float.IsNaN(vertex.z)||float.IsInfinity(vertex.x)||float.IsInfinity(vertex.y)||float.IsInfinity(vertex.z))throw new Exception("Invalid geometry in "+mesh.name);
   Debug.Log("REALISM_MESH "+number+" "+mesh.name+" probeLit="+ProbeLit(filter.transform));
   if(!ProbeLit(filter.transform)&&!Unwrapping.GenerateSecondaryUVSet(mesh))throw new Exception("Could not generate lightmap UVs: "+mesh.name);
   AssetDatabase.CreateAsset(mesh,$"{Generated}/Mesh-{number++:D3}.asset");
  }
  var materials=new HashSet<Material>();int matIndex=0;
  foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>()){
   if(!renderer.enabled)continue;
   renderer.shadowCastingMode=ShadowCastingMode.TwoSided;renderer.receiveShadows=true;if(ProbeLit(renderer.transform))renderer.receiveGI=ReceiveGI.LightProbes;
   GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,renderer.gameObject.name=="Natural desert terrain"?StaticEditorFlags.ReflectionProbeStatic:StaticEditorFlags.ContributeGI|StaticEditorFlags.ReflectionProbeStatic|StaticEditorFlags.OccluderStatic|StaticEditorFlags.OccludeeStatic);
   if(renderer.gameObject.name=="Natural desert terrain")renderer.scaleInLightmap=.035f;
   foreach(var m in renderer.sharedMaterials){
    if(!m||!m.shader.name.StartsWith("Tabernacle/")||!materials.Add(m))continue;
    if(m.mainTexture&&m.mainTexture.name=="Baked bread crust"&&string.IsNullOrEmpty(AssetDatabase.GetAssetPath(m.mainTexture)))AssetDatabase.CreateAsset(m.mainTexture,$"{Generated}/BreadCrust.asset");
    if(string.IsNullOrEmpty(AssetDatabase.GetAssetPath(m)))AssetDatabase.CreateAsset(m,$"{Generated}/Material-{matIndex++:D2}.mat");
   }
  }
  if(RenderSettings.skybox&&string.IsNullOrEmpty(AssetDatabase.GetAssetPath(RenderSettings.skybox)))AssetDatabase.CreateAsset(RenderSettings.skybox,Generated+"/DesertSky.mat");
  var probes=new GameObject("Baked illumination samples").AddComponent<LightProbeGroup>();var points=new List<Vector3>();
  for(float z=27;z<39.5f;z+=1.5f)foreach(float x in new[]{-1.8f,0,1.8f})foreach(float y in new[]{.35f,1.5f,3.2f})points.Add(new Vector3(x,y,z));
  for(float z=3;z<44;z+=6)foreach(float x in new[]{-7f,0,7f})points.Add(new Vector3(x,1.3f,z));probes.probePositions=points.ToArray();
  var reveal=new GameObject("Educational Ark reveal illumination — no historical fixture").AddComponent<Light>();reveal.type=LightType.Rectangle;reveal.transform.position=new Vector3(0,2.4f,35.6f);reveal.transform.rotation=Quaternion.identity;reveal.areaSize=new Vector2(2.5f,2);reveal.intensity=.45f;reveal.color=new Color(1,.82f,.56f);reveal.lightmapBakeType=LightmapBakeType.Baked;
  SoftFill("Educational soft ceiling fill",new Vector3(0,3.9f,30.5f),Quaternion.Euler(90,0,0),new Vector2(3,7),.30f);
  SoftFill("Educational veil visibility fill",new Vector3(0,2.8f,27),Quaternion.identity,new Vector2(3,2.5f),.45f);
  var key=new GameObject("Educational Ark sculpture key light").AddComponent<Light>();key.type=LightType.Spot;key.transform.position=new Vector3(0,2.7f,36.2f);key.transform.LookAt(new Vector3(0,.7f,38));key.range=6;key.spotAngle=65;key.intensity=3;key.color=new Color(1,.90f,.73f);key.shadows=LightShadows.Soft;key.lightmapBakeType=LightmapBakeType.Baked;
  var lighting=new LightingSettings{name="Tabernacle baked illumination"};
  lighting.bakedGI=true;lighting.realtimeGI=false;lighting.lightmapResolution=14;lighting.lightmapMaxSize=2048;lighting.lightmapPadding=4;Set(lighting,"directionalityMode",LightmapsMode.NonDirectional);
  // Unity 6.3's compute backend supports Apple Silicon; older backends remain a fallback.
  var enumType=typeof(LightingSettings.Lightmapper);string backend=Enum.GetNames(enumType).Contains("UnityComputeGPU")?"UnityComputeGPU":"ProgressiveGPU";
  lighting.lightmapper=(LightingSettings.Lightmapper)Enum.Parse(enumType,backend);
  Set(lighting,"directSampleCount",32);Set(lighting,"indirectSampleCount",96);Set(lighting,"environmentSampleCount",32);Set(lighting,"maxBounces",3);
  Set(lighting,"ao",true);Set(lighting,"aoMaxDistance",.55f);Set(lighting,"aoExponentIndirect",1.2f);Set(lighting,"aoExponentDirect",.6f);
  AssetDatabase.CreateAsset(lighting,Generated+"/Lighting.asset");Lightmapping.lightingSettings=lighting;
  AddProbe("Courtyard reflection",new Vector3(0,2,16),new Vector3(25,7,47),0);
  AddProbe("Holy Place reflection",new Vector3(0,2.1f,30.5f),new Vector3(4.5f,4.3f,9),2);
  AddProbe("Ark chamber reflection",new Vector3(0,2,37.4f),new Vector3(4.5f,4.3f,4.4f),3);
  EditorSceneManager.SaveScene(scene,"Assets/TabernacleRealistic.unity");AssetDatabase.SaveAssets();
  Physics.SyncTransforms();Validate(world);
  Debug.Log("REALISM_BAKE_START backend="+backend+" renderers="+root.GetComponentsInChildren<MeshRenderer>().Length);
  started=EditorApplication.timeSinceStartup;nextReport=started+15;baking=true;Lightmapping.bakeCompleted+=Baked;
  EditorApplication.update+=Watch;
  if(!Lightmapping.BakeAsync())Fail("Lightmapper did not start");
 }
 static bool ProbeLit(Transform transform){for(var t=transform;t;t=t.parent)if(t.name.StartsWith("Furnishing")||t.name.Contains("Lampstand close detail"))return true;return false;}
 static void Set(object obj,string property,object value){var p=obj.GetType().GetProperty(property);if(p!=null&&p.CanWrite)p.SetValue(obj,p.PropertyType.IsEnum?Enum.ToObject(p.PropertyType,Convert.ToInt32(value)):Convert.ChangeType(value,p.PropertyType));}
 static void SoftFill(string name,Vector3 position,Quaternion rotation,Vector2 size,float intensity){var l=new GameObject(name).AddComponent<Light>();l.type=LightType.Rectangle;l.transform.position=position;l.transform.rotation=rotation;l.areaSize=size;l.intensity=intensity;l.color=new Color(1,.88f,.68f);l.lightmapBakeType=LightmapBakeType.Baked;}
 static void AddProbe(string name,Vector3 position,Vector3 size,int importance){var g=new GameObject(name);g.transform.position=position;var p=g.AddComponent<ReflectionProbe>();p.mode=ReflectionProbeMode.Baked;p.resolution=128;p.hdr=true;p.boxProjection=true;p.size=size;p.blendDistance=1;p.importance=importance;p.nearClipPlane=.1f;p.farClipPlane=100;p.cullingMask=~0;}
 static void Watch(){if(!baking)return;var now=EditorApplication.timeSinceStartup;if(now>nextReport){Debug.Log("REALISM_BAKE_PROGRESS seconds="+(now-started).ToString("F0"));nextReport=now+15;}if(now-started>360){Lightmapping.Cancel();Fail("Lighting bake exceeded six minutes; retained the prior app build");}}
 static void Baked(){
  baking=false;Lightmapping.bakeCompleted-=Baked;EditorApplication.update-=Watch;
  try{
   if(LightmapSettings.lightmaps.Length==0)throw new Exception("Bake completed without lightmaps");
   Debug.Log("REALISM_BAKE_COMPLETE lightmaps="+LightmapSettings.lightmaps.Length);
   Shader.SetGlobalFloat("_ReflectionCapture",1);int i=0;foreach(var p in UnityEngine.Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None)){string path=$"{Generated}/Reflection-{i++}.exr";if(!Lightmapping.BakeReflectionProbe(p,path))throw new Exception("Reflection bake failed: "+p.name);}
   Shader.SetGlobalFloat("_ReflectionCapture",0);EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
   Review();if(buildAfter)QuestBuild.Build();
   Debug.Log("REALISM_REVIEW_COMPLETE");EditorApplication.Exit(0);
  }catch(Exception ex){Fail(ex.ToString());}
 }
 static void Validate(TabernacleWorld world){
  var all=world.GetComponentsInChildren<Transform>();
  Check(all.Count(t=>t.name.StartsWith("Bread loaf "))==12,"twelve loaves");Check(all.Count(t=>t.name.StartsWith("Oil lamp ")&&t.name.EndsWith("of seven"))==7,"seven oil lamps");
  foreach(var stop in world.stops)Check(world.CanStand(stop),"safe tour stop "+stop);
  foreach(var p in new[]{new Vector3(0,0,8),new Vector3(0,0,16.5f),new Vector3(1.5f,0,30)})Check(!world.CanStand(p),"blocked furnishing "+p);
 }
 static void Check(bool test,string label){if(!test)throw new Exception("REALISM_CHECK_FAILED "+label);Debug.Log("REALISM_CHECK_PASS "+label);}
 public static void Review(){
  var folder=Path.GetFullPath("../output/tabernacle-vr/realism-pass/previews");Directory.CreateDirectory(folder);
  var cam=new GameObject("Reference comparison camera").AddComponent<Camera>();cam.fieldOfView=65;cam.nearClipPlane=.07f;cam.farClipPlane=140;cam.allowHDR=true;cam.clearFlags=CameraClearFlags.Skybox;cam.allowMSAA=true;
  var rt=new RenderTexture(1800,1125,24,RenderTextureFormat.ARGB32){antiAliasing=4};cam.targetTexture=rt;
  Shot(cam,folder,"holy-place",new Vector3(0,1.65f,27.5f),new Vector3(0,1.4f,33));
  Shot(cam,folder,"twelve-loaves",new Vector3(.5f,1.35f,28.9f),new Vector3(1.5f,.98f,30));
  Shot(cam,folder,"seven-lamps",new Vector3(-.4f,1.5f,28.7f),new Vector3(-1.4f,1.15f,30));
  Shot(cam,folder,"courtyard",new Vector3(7,6,-8),new Vector3(0,1,23));
  Shot(cam,folder,"altar-utensils",new Vector3(-4,1.5f,6),new Vector3(-2.6f,.15f,8));
  Shot(cam,folder,"ark-chamber",new Vector3(0,1.4f,36.2f),new Vector3(0,.9f,38));
  cam.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(cam.gameObject);UnityEngine.Object.DestroyImmediate(rt);
 }
 static void Shot(Camera c,string folder,string name,Vector3 p,Vector3 target){c.transform.position=p;c.transform.LookAt(target);Shader.SetGlobalFloat("_SceneExposure",p.z>35?10f:p.z>26?5f:1f);c.Render();RenderTexture.active=c.targetTexture;var t=new Texture2D(1800,1125,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1800,1125),0,0);t.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);RenderTexture.active=null;}
 static void Fail(string message){baking=false;Lightmapping.bakeCompleted-=Baked;EditorApplication.update-=Watch;Debug.LogError("REALISM_FAILURE "+message);EditorApplication.Exit(1);}
}
