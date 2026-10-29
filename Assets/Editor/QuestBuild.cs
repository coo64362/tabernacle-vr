using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;

public static class QuestBuild {
 public static void BuildAndReview(){Build();RealismBuild.Review();}
 [MenuItem("Tabernacle/Configure and build Quest")]
 public static void Build(){
  Directory.CreateDirectory("Assets/XR");Directory.CreateDirectory("Builds");
  PlayerSettings.companyName="Tabernacle Study";PlayerSettings.productName="Tabernacle Walkthrough";PlayerSettings.bundleVersion="0.2.0";
  PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"org.tabernacle.walkthrough");PlayerSettings.Android.bundleVersionCode=2;
  PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
  PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel29;PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;
  PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.OpenGLES3});
  PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.gpuSkinning=false;PlayerSettings.MTRendering=true;
  PlayerSettings.Android.useCustomKeystore=false;PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android,ManagedStrippingLevel.Low);
  PlayerSettings.stripEngineCode=true;PlayerSettings.Android.androidIsGame=true;PlayerSettings.Android.renderOutsideSafeArea=true;
  PlayerSettings.enableFrameTimingStats=true;
  QualitySettings.antiAliasing=4;QualitySettings.vSyncCount=0;QualitySettings.shadows=ShadowQuality.Disable;QualitySettings.pixelLightCount=0;QualitySettings.lodBias=1;
  var projectSettings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);var input=projectSettings.FindProperty("activeInputHandler");if(input!=null)input.intValue=1;projectSettings.ApplyModifiedPropertiesWithoutUndo();
  XRGeneralSettingsPerBuildTarget perTarget;
  if(!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey,out perTarget)||!perTarget){perTarget=ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();AssetDatabase.CreateAsset(perTarget,"Assets/XR/GeneralSettings.asset");EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey,perTarget,true);}
  if(!perTarget.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android))perTarget.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);
  var general=perTarget.SettingsForBuildTarget(BuildTargetGroup.Android);general.InitManagerOnStart=true;general.Manager.automaticLoading=true;general.Manager.automaticRunning=true;
  if(!XRPackageMetadataStore.AssignLoader(general.Manager,"UnityEngine.XR.OpenXR.OpenXRLoader",BuildTargetGroup.Android))throw new Exception("OpenXR loader assignment failed");
  UnityEditor.XR.OpenXR.Features.FeatureHelpers.RefreshFeatures(BuildTargetGroup.Android);
  var settings=OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);if(!settings)throw new Exception("No Android OpenXR settings");
  settings.renderMode=OpenXRSettings.RenderMode.SinglePassInstanced;settings.depthSubmissionMode=OpenXRSettings.DepthSubmissionMode.None;
  foreach(var feature in settings.GetFeatures<OpenXRFeature>()){feature.enabled=feature is MetaQuestFeature||feature is OculusTouchControllerProfile||feature is QuestRefreshRate;}
  var quest=settings.GetFeature<MetaQuestFeature>();if(!quest)throw new Exception("Missing Meta Quest feature");quest.enabled=true;
  var enableDevice=typeof(MetaQuestFeature).GetMethod("EnableTargetDevice",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);enableDevice?.Invoke(quest,new object[]{"quest2",true});enableDevice?.Invoke(quest,new object[]{"quest3",true});
  EditorUtility.SetDirty(settings);EditorUtility.SetDirty(general);EditorUtility.SetDirty(general.Manager);EditorUtility.SetDirty(perTarget);AssetDatabase.SaveAssets();
  string scenePath="Assets/TabernacleRealistic.unity";
  if(!File.Exists(scenePath))throw new Exception("Build the realistic scene with RealismBuild first");
  EditorSceneManager.OpenScene(scenePath);
  var tangentMeshes=new System.Collections.Generic.HashSet<Mesh>();
  foreach(var filter in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)){
   var mesh=filter.sharedMesh;if(!mesh||!tangentMeshes.Add(mesh)||!AssetDatabase.GetAssetPath(mesh).StartsWith("Assets/RealisticArt/"))continue;
   mesh.RecalculateTangents();var tangents=mesh.tangents;var normals=mesh.normals;
   for(int i=0;i<tangents.Length;i++)if(float.IsNaN(tangents[i].x)||float.IsNaN(tangents[i].y)||float.IsNaN(tangents[i].z)||new Vector3(tangents[i].x,tangents[i].y,tangents[i].z).sqrMagnitude<.01f){var n=normals[i];var tangent=Vector3.Cross(n,Mathf.Abs(n.y)<.9f?Vector3.up:Vector3.right).normalized;tangents[i]=new Vector4(tangent.x,tangent.y,tangent.z,1);}
   mesh.tangents=tangents;EditorUtility.SetDirty(mesh);
  }
  foreach(var renderer in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))foreach(var material in renderer.sharedMaterials)if(material&&material.name=="Bread"){material.mainTexture=Resources.Load<Texture2D>("Art/Bread");material.SetFloat("_Relief",.8f);EditorUtility.SetDirty(material);}
  var builtWorld=UnityEngine.Object.FindFirstObjectByType<TabernacleWorld>();
  if(!builtWorld||!builtWorld.prebuilt)throw new Exception("Expected the baked realistic world");
  builtWorld.bread.mainTexture=Resources.Load<Texture2D>("Art/Bread");builtWorld.bread.SetFloat("_Relief",.8f);EditorUtility.SetDirty(builtWorld.bread);
  Debug.Log("ART_BREAD_TEXTURE "+builtWorld.bread.mainTexture.name);
  AssetDatabase.SaveAssets();EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scenePath,true)};
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{scenePath},locationPathName="Builds/TabernacleVR.apk",target=BuildTarget.Android,options=BuildOptions.None});
  Debug.Log("TABERNACLE_BUILD "+report.summary.result+" bytes="+report.summary.totalSize+" errors="+report.summary.totalErrors);
  if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Quest build failed");
 }
}
