using UnityEngine;
using UnityEngine.XR;

// Deliberate tracked waves and nods receive a social response. No camera images
// or additional recording; ignore teleports, missing tracking and busy dialogue.
public sealed class GuideVisitorGestures : MonoBehaviour {
 TabernacleExperience experience;Transform eye,left,right;float lastTime=-1,nextAllowed;
 Vector3 previousEye,previousLeft,previousRight;float previousPitch;int waveDirection,nodDirection,waveTurns,nodTurns;float waveStart,nodStart;
 public int Responses {get;private set;}
 public void Initialize(TabernacleExperience owner,Transform head,Transform leftController,Transform rightController){experience=owner;eye=head;left=leftController;right=rightController;}
 static bool Tracked(XRNode node){var device=InputDevices.GetDeviceAtXRNode(node);return device.isValid&&device.TryGetFeatureValue(CommonUsages.isTracked,out bool tracked)&&tracked;}
 void Update(){if(!eye||!left||!right)return;Observe(Time.unscaledTime,eye.position,eye.rotation,left.position,right.position,Tracked(XRNode.Head)&&Tracked(XRNode.LeftHand)&&Tracked(XRNode.RightHand));}
 public void Observe(float now,Vector3 head,Quaternion rotation,Vector3 leftHand,Vector3 rightHand,bool tracked){
  float dt=now-lastTime;bool reset=!tracked||lastTime<0||dt<=0||dt>.15f||(head-previousEye).sqrMagnitude>.16f;
  float pitch=Mathf.DeltaAngle(0,rotation.eulerAngles.x);
  if(!reset&&experience&&experience.Guide){
   var guide=experience.Guide;Vector3 toward=guide.transform.position+Vector3.up*1.35f-head;
   bool available=!experience.IntroductionPlaying&&experience.GuideVisible&&!guide.Speaking&&!guide.Thinking&&!guide.Listening&&now>=nextAllowed&&toward.sqrMagnitude<16&&Vector3.Dot(rotation*Vector3.forward,toward.normalized)>.35f;
   if(available){
    Vector3 hand=rightHand.y>leftHand.y?rightHand:leftHand,old=rightHand.y>leftHand.y?previousRight:previousLeft;
    // Subtract head translation, so walking or moving the room cannot count as waving.
    float speed=Vector3.Dot((hand-old)-(head-previousEye),rotation*Vector3.right)/dt;
    if(now-waveStart>1.6f){waveTurns=0;waveDirection=0;waveStart=now;}
    if(hand.y>head.y-.20f&&Mathf.Abs(speed)>.35f&&Mathf.Abs(speed)<3.5f){int direction=speed>0?1:-1;if(waveDirection!=0&&direction!=waveDirection)waveTurns++;waveDirection=direction;}
    else if(hand.y<head.y-.35f){waveTurns=0;waveDirection=0;}
    if(now-nodStart>1.3f){nodTurns=0;nodDirection=0;nodStart=now;}
    float angularSpeed=Mathf.DeltaAngle(previousPitch,pitch)/dt;
    if(Mathf.Abs(pitch)<35&&Mathf.Abs(angularSpeed)>35&&Mathf.Abs(angularSpeed)<220){int direction=angularSpeed>0?1:-1;if(nodDirection!=0&&direction!=nodDirection)nodTurns++;nodDirection=direction;}
    if(waveTurns>=3){guide.PlayGesture("wave",2);Respond(now,"wave");}
    else if(nodTurns>=2){guide.PlayGesture("nod",1.5f);Respond(now,"nod");}
   }else{waveTurns=nodTurns=waveDirection=nodDirection=0;waveStart=nodStart=now;}
  }else{waveTurns=nodTurns=waveDirection=nodDirection=0;waveStart=nodStart=now;}
  previousEye=head;previousLeft=leftHand;previousRight=rightHand;previousPitch=pitch;lastTime=now;
 }
 void Respond(float now,string gesture){Responses++;nextAllowed=now+5;waveTurns=nodTurns=waveDirection=nodDirection=0;Debug.Log("TABERNACLE_VISITOR_GESTURE "+gesture+" acknowledged");}
}
