using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.OpenXR.Features;
#endif

#if UNITY_EDITOR
[OpenXRFeature(UiName="Tabernacle 72 Hz",Desc="Request native 72 Hz for a conservative performance baseline.",Company="Tabernacle Study",Version="1.0.0",FeatureId="org.tabernacle.refresh",OpenxrExtensionStrings="XR_FB_display_refresh_rate",BuildTargetGroups=new[]{BuildTargetGroup.Android})]
#endif
public class QuestRefreshRate : OpenXRFeature {
 [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GetProc(ulong instance,[MarshalAs(UnmanagedType.LPStr)] string name,out IntPtr address);
 [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int RequestRefresh(ulong session,float rate);
 RequestRefresh request;
 protected override bool OnInstanceCreate(ulong instance){
  if(!OpenXRRuntime.IsExtensionEnabled("XR_FB_display_refresh_rate"))return false;
  var get=Marshal.GetDelegateForFunctionPointer<GetProc>(xrGetInstanceProcAddr);
  if(get(instance,"xrRequestDisplayRefreshRateFB",out var ptr)<0||ptr==IntPtr.Zero)return false;
  request=Marshal.GetDelegateForFunctionPointer<RequestRefresh>(ptr);return true;
 }
 protected override void OnSessionBegin(ulong session){if(request!=null)Debug.Log("TABERNACLE_REFRESH requested=72 result="+request(session,72));}
}
