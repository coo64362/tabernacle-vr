using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.OpenXR;

public class QuestExplorer : MonoBehaviour {
 Transform rig, right, left;
 Camera eye;
 InputAction headPos,headRot,rightPos,rightRot,leftPos,leftRot,trigger,turn,next,stats,rightTracked;
 readonly List<InputAction> actions=new List<InputAction>();
 readonly List<XRInputSubsystem> inputs=new List<XRInputSubsystem>();
 readonly List<XRDisplaySubsystem> displays=new List<XRDisplaySubsystem>();
 readonly Vector3[] arcPoints=new Vector3[33];
 readonly FrameTiming[] timings=new FrameTiming[1];
 LineRenderer ray;
 Material teleportMaterial;
 Transform marker;
 ControllerTablet tablet;
 InputAction tabletToggle;
 bool tabletHeld,fading; float viewLift;
 Material fadeMaterial;
 TextMesh information,performance;
 bool aiming,valid,snapLatched,nextHeld,statsHeld,floorMode;
 Vector3 destination;
 int station,frames;float elapsed,reportElapsed,detailTimer;float lastFps;
 bool showStats; bool floorRequested;
 InputAction Action(string name,string binding,InputActionType type=InputActionType.Value){var a=new InputAction(name,type,binding);a.Enable();actions.Add(a);return a;}
 void Start(){
  QualitySettings.vSyncCount=0;QualitySettings.antiAliasing=4;Application.targetFrameRate=72;Screen.sleepTimeout=SleepTimeout.NeverSleep;
  rig=new GameObject("XR floor origin").transform;rig.position=TabernacleWorld.Instance.stops[0];
  var cameraObject=new GameObject("Tracked headset");cameraObject.transform.SetParent(rig,false);cameraObject.tag="MainCamera";eye=cameraObject.AddComponent<Camera>();eye.nearClipPlane=.07f;eye.farClipPlane=130;eye.clearFlags=CameraClearFlags.Skybox;eye.backgroundColor=new Color(.70f,.75f,.76f);eye.allowHDR=false;eye.allowMSAA=true;cameraObject.AddComponent<AudioListener>();
  headPos=Action("Head position","<XRHMD>/centerEyePosition");headRot=Action("Head rotation","<XRHMD>/centerEyeRotation");
  var driver=cameraObject.AddComponent<TrackedPoseDriver>();driver.positionInput=new InputActionProperty(headPos);driver.rotationInput=new InputActionProperty(headRot);
  right=new GameObject("Right controller").transform;right.SetParent(rig,false);left=new GameObject("Left controller").transform;left.SetParent(rig,false);
  rightPos=Action("Right position","<XRController>{RightHand}/devicePosition");rightRot=Action("Right rotation","<XRController>{RightHand}/deviceRotation");leftPos=Action("Left position","<XRController>{LeftHand}/devicePosition");leftRot=Action("Left rotation","<XRController>{LeftHand}/deviceRotation");
  trigger=Action("Teleport trigger","<XRController>{RightHand}/trigger");turn=Action("Snap turn","<XRController>{RightHand}/primary2DAxis");next=Action("Next tour stop","<XRController>{RightHand}/primaryButton",InputActionType.Button);stats=Action("Performance display","<XRController>{RightHand}/secondaryButton",InputActionType.Button);rightTracked=Action("Right tracked","<XRController>{RightHand}/isTracked",InputActionType.Button);
  foreach(var hand in new[]{right,left}){var g=TabernacleWorld.Instance.Shape(PrimitiveType.Capsule,"Controller representation",hand,new Vector3(0,-.025f,0),new Vector3(.04f,.085f,.04f),TabernacleWorld.Instance.ink);g.transform.localRotation=Quaternion.Euler(40,0,0);}
  ray=new GameObject("Teleport arc").AddComponent<LineRenderer>();teleportMaterial=new Material(Resources.Load<Shader>("TeleportIndicator")){color=new Color(1,.75f,.25f)};ray.sharedMaterial=teleportMaterial;ray.widthMultiplier=.008f;ray.positionCount=0;ray.useWorldSpace=true;ray.numCornerVertices=2;
  marker=TabernacleWorld.Instance.Shape(PrimitiveType.Cylinder,"Teleport destination",null,Vector3.zero,new Vector3(.5f,.006f,.5f),TabernacleWorld.Instance.gold).transform;marker.GetComponent<Renderer>().sharedMaterial=teleportMaterial;marker.gameObject.SetActive(false);
  tablet=new GameObject("Left hand tablet").AddComponent<ControllerTablet>();tablet.Initialize(left);
  tablet.VisitStation=GoToStation;tablet.SetRaisedView=SetRaisedView;
  tabletToggle=Action("Tablet toggle","<XRController>{LeftHand}/secondaryButton",InputActionType.Button);
  var fadeObject=GameObject.CreatePrimitive(PrimitiveType.Quad);fadeObject.name="Comfort transition fade";Destroy(fadeObject.GetComponent<Collider>());fadeObject.transform.SetParent(eye.transform,false);fadeObject.transform.localPosition=new Vector3(0,0,.09f);fadeObject.transform.localScale=Vector3.one*2;
  fadeMaterial=new Material(Resources.Load<Shader>("ComfortFade"));fadeMaterial.color=Color.clear;fadeObject.GetComponent<Renderer>().sharedMaterial=fadeMaterial;
  performance=TabernacleWorld.Instance.Text("Performance",eye.transform,new Vector3(0,-.19f,.75f),"",.0055f,Color.green);performance.gameObject.SetActive(false);
  MoveInfo(); Invoke(nameof(ConfigureXR),1);Debug.Log("TABERNACLE_READY native OpenXR prototype; right trigger teleport, stick turn, A next, B metrics");
 }
 void ConfigureXR(){
  SubsystemManager.GetSubsystems(inputs);foreach(var input in inputs) {if(input.running){floorRequested=true;floorMode=input.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);}}
  SubsystemManager.GetSubsystems(displays);foreach(var display in displays){if(display.running)Debug.Log("TABERNACLE_DISPLAY running");}
  Debug.Log("TABERNACLE_XR displayCount="+displays.Count+" floor="+floorMode+" loader="+UnityEngine.XR.Management.XRGeneralSettings.Instance?.Manager?.activeLoader?.name);
 }
 void Update(){
  if(!eye)return;
  var ep=eye.transform.position;float exposure=Mathf.Abs(ep.x)<2.6f&&ep.z>26&&ep.z<40?(ep.z>35?10f:5f):1f;Shader.SetGlobalFloat("_SceneExposure",Mathf.Lerp(Mathf.Max(1,Shader.GetGlobalFloat("_SceneExposure")),exposure,Time.unscaledDeltaTime*3));
  if(!floorRequested&&Time.frameCount%120==0)ConfigureXR();
  // Floor-origin is preferred; device-origin fallback adds a standing eye-height.
  if(!floorMode)eye.transform.localPosition=headPos.ReadValue<Vector3>()+Vector3.up*1.65f;
  Pose(right,rightPos,rightRot);Pose(left,leftPos,leftRot);
  bool tracked=rightTracked.ReadValue<float>()>.5f;
  bool down=!fading&&tracked&&trigger.ReadValue<float>()>.65f;
  bool toggle=tabletToggle.IsPressed();if(toggle&&!tabletHeld)tablet.Toggle();tabletHeld=toggle;
  bool overTablet=tablet.Point(right,down,out var tabletHit);
  if(overTablet){ray.positionCount=2;ray.SetPosition(0,right.position);ray.SetPosition(1,tabletHit);marker.gameObject.SetActive(false);valid=false;aiming=false;}

  if(!overTablet){if(down)Aim();else {if(tracked&&Application.isFocused&&aiming&&valid)Teleport(destination);ray.positionCount=0;marker.gameObject.SetActive(false);valid=false;}
  aiming=down;}
  float stick=turn.ReadValue<Vector2>().x;
  if(Mathf.Abs(stick)>.7f&&!snapLatched&&!down&&!fading){rig.RotateAround(eye.transform.position,Vector3.up,Mathf.Sign(stick)*30);snapLatched=true;}
  if(Mathf.Abs(stick)<.3f)snapLatched=false;
  bool n=next.IsPressed();if(n&&!nextHeld&&!down){GoToStation((station+1)%TabernacleWorld.Instance.stops.Length);}nextHeld=n;
  bool b=stats.IsPressed();if(b&&!statsHeld){showStats=!showStats;performance.gameObject.SetActive(showStats);}statsHeld=b;
  detailTimer+=Time.unscaledDeltaTime;if(detailTimer>.3f){TabernacleWorld.Instance.UpdateDetails(eye.transform.position);detailTimer=0;}
  FrameTimingManager.CaptureFrameTimings();frames++;elapsed+=Time.unscaledDeltaTime;reportElapsed+=Time.unscaledDeltaTime;
  if(elapsed>=2){lastFps=frames/elapsed;frames=0;elapsed=0;FrameTimingManager.GetLatestTimings(1,timings);performance.text=$"{lastFps:F0} FPS  |  CPU {timings[0].cpuFrameTime:F1} ms  GPU {timings[0].gpuFrameTime:F1} ms\nPrototype • 72 Hz target • B to hide";}
  if(reportElapsed>=10){reportElapsed=0;bool running=displays.Count>0&&displays[0].running;float refresh=0;if(displays.Count>0)displays[0].TryGetDisplayRefreshRate(out refresh);Debug.Log($"TABERNACLE_PERF fps={lastFps:F1} cpuMs={timings[0].cpuFrameTime:F2} gpuMs={timings[0].gpuFrameTime:F2} xrRunning={running} refresh={refresh} focused={Application.isFocused} head={eye.transform.position} rightTracked={tracked}");}
 }
 void Pose(Transform t,InputAction p,InputAction r){t.localPosition=p.ReadValue<Vector3>()+(floorMode?Vector3.zero:Vector3.up*1.65f);var q=r.ReadValue<Quaternion>();t.localRotation=Quaternion.Dot(q,q)>.1f?q:Quaternion.identity;}
 void Aim(){
  Vector3 origin=right.position+right.forward*.05f;Vector3 velocity=right.forward*7;Vector3 prev=origin;arcPoints[0]=origin;int count=1;valid=false;
  for(int i=1;i<arcPoints.Length;i++){float time=i*.075f;Vector3 p=origin+velocity*time+Vector3.down*4*time*time;
   if(Physics.Linecast(prev,p,out var hit,(1<<8)|(1<<9),QueryTriggerInteraction.Ignore)){
    arcPoints[count++]=hit.point;destination=hit.point;valid=hit.collider.gameObject.layer==9&&TabernacleWorld.Instance.CanStand(destination);break;
   }
   arcPoints[count++]=p;prev=p;
  }
  ray.positionCount=count;for(int i=0;i<count;i++)ray.SetPosition(i,arcPoints[i]);marker.gameObject.SetActive(valid);if(valid)marker.position=destination+Vector3.up*.012f;
 }
 void Teleport(Vector3 target){if(!TabernacleWorld.Instance.CanStand(target))return;var offset=eye.transform.position-rig.position;offset.y=0;rig.position=target-offset+Vector3.up*viewLift;}
 void MoveInfo(){tablet?.SetStation(station);}
 public void GoToStation(int value){if(fading||value<0||value>=TabernacleWorld.Instance.stops.Length)return;StartCoroutine(Transition(value));}
 IEnumerator Transition(int value){
  fading=true;aiming=false;valid=false;
  for(float t=0;t<.15f;t+=Time.unscaledDeltaTime){fadeMaterial.color=new Color(0,0,0,t/.15f);yield return null;}
  fadeMaterial.color=Color.black;station=value;Teleport(TabernacleWorld.Instance.stops[value]);MoveInfo();
  Debug.Log("TABERNACLE_STATION "+value+" "+TabernacleWorld.Instance.titles[value]);yield return null;
  for(float t=0;t<.15f;t+=Time.unscaledDeltaTime){fadeMaterial.color=new Color(0,0,0,1-t/.15f);yield return null;}
  fadeMaterial.color=Color.clear;fading=false;
 }
 void SetRaisedView(bool raised){if(!fading)StartCoroutine(RaiseTransition(raised?.40f:0));}
 IEnumerator RaiseTransition(float nextLift){
  fading=true;aiming=false;valid=false;
  for(float t=0;t<.15f;t+=Time.unscaledDeltaTime){fadeMaterial.color=new Color(0,0,0,t/.15f);yield return null;}
  fadeMaterial.color=Color.black;rig.position+=Vector3.up*(nextLift-viewLift);viewLift=nextLift;yield return null;
  for(float t=0;t<.15f;t+=Time.unscaledDeltaTime){fadeMaterial.color=new Color(0,0,0,1-t/.15f);yield return null;}
  fadeMaterial.color=Color.clear;fading=false;
 }
 void OnApplicationFocus(bool focus){if(!focus){aiming=false;valid=false;if(ray)ray.positionCount=0;if(marker)marker.gameObject.SetActive(false);}}
 void OnDestroy(){foreach(var a in actions)a.Dispose();if(teleportMaterial)Destroy(teleportMaterial);if(fadeMaterial)Destroy(fadeMaterial);}
}
