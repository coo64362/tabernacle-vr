using UnityEngine;

// Gestures run after animation, relative to imported bind rotations; mouth motion follows actual audio.
[DefaultExecutionOrder(100)]
public sealed class RiggedGuideVisual : MonoBehaviour {
 sealed class Arm {public Transform upper,lower,hand;public Quaternion upperRest,lowerRest,handRest;public float a,b,side;public Quaternion previousUpper,previousLower;}
 Transform viewer,head,smile,leftEye,rightEye;Quaternion headRest;Vector3 smileRest,leftRest,rightRest,leftEyePosition,rightEyePosition;RobotGuide owner;Arm right,left;
 Transform rightMiddle;
 Transform[] legs=new Transform[4];Quaternion[] legRest=new Quaternion[4];float blend;Vector3 previousPosition;string oldGesture="";float gestureStart;
 public static Transform Find(Transform root,string name){foreach(var t in root.GetComponentsInChildren<Transform>(true))if(t.name==name)return t;return null;}
 Arm BindArm(string side){var arm=new Arm{upper=Find(transform,side+"UpperArm"),lower=Find(transform,side+"LowerArm"),hand=Find(transform,side+"Hand")};if(arm.upper&&arm.lower&&arm.hand){arm.upperRest=arm.upper.localRotation;arm.previousUpper=arm.upperRest;arm.lowerRest=arm.lower.localRotation;arm.previousLower=arm.lowerRest;arm.handRest=arm.hand.localRotation;arm.side=Mathf.Sign(transform.InverseTransformPoint(arm.upper.position).x);arm.a=Vector3.Distance(arm.upper.position,arm.lower.position);arm.b=Vector3.Distance(arm.lower.position,arm.hand.position);}return arm;}
 public void Initialize(RobotGuide guide,Transform user){
  owner=guide;viewer=user;head=Find(transform,"Head");right=BindArm("Right");left=BindArm("Left");smile=Find(transform,"Smile");leftEye=Find(transform,"Eye_Left");rightEye=Find(transform,"Eye_Right");if(head)headRest=Quaternion.Inverse(transform.rotation)*head.rotation;if(smile)smileRest=smile.localScale;if(leftEye){leftRest=leftEye.localScale;leftEyePosition=leftEye.localPosition;}if(rightEye){rightRest=rightEye.localScale;rightEyePosition=rightEye.localPosition;}
  string[] names={"LeftUpperLeg","LeftLowerLeg","RightUpperLeg","RightLowerLeg"};for(int i=0;i<4;i++){legs[i]=Find(transform,names[i]);if(legs[i])legRest[i]=legs[i].localRotation;}previousPosition=transform.position;
  rightMiddle=Find(transform,"RightMiddle1");
 }
 void ScaleVertical(Transform part,Vector3 rest,float value){var axis=part.InverseTransformDirection(transform.up);axis=new Vector3(Mathf.Abs(axis.x),Mathf.Abs(axis.y),Mathf.Abs(axis.z));var scale=Vector3.one;if(axis.x>axis.y&&axis.x>axis.z)scale.x=value;else if(axis.z>axis.y)scale.z=value;else scale.y=value;part.localScale=Vector3.Scale(rest,scale);}
 void Rest(Arm arm){if(arm.upper)arm.upper.localRotation=arm.upperRest;if(arm.lower)arm.lower.localRotation=arm.lowerRest;if(arm.hand)arm.hand.localRotation=arm.handRest;}
 void Reach(Arm arm,Vector3 target,Vector3 hint,float weight){
  if(!arm.upper||!arm.lower||!arm.hand||weight<.001f)return;Vector3 shoulder=arm.upper.position;Vector3 offset=target-shoulder;float d=Mathf.Clamp(offset.magnitude,Mathf.Abs(arm.a-arm.b)+.025f,arm.a+arm.b-.01f);var axis=offset.normalized;var bend=Vector3.ProjectOnPlane(hint-shoulder,axis).normalized;if(bend.sqrMagnitude<.1f)bend=transform.forward;float along=(arm.a*arm.a-arm.b*arm.b+d*d)/(2*d);Vector3 elbow=shoulder+axis*along+bend*Mathf.Sqrt(Mathf.Max(0,arm.a*arm.a-along*along));var rotation=Quaternion.FromToRotation(arm.lower.position-shoulder,elbow-shoulder)*arm.upper.rotation;arm.upper.rotation=Quaternion.Slerp(arm.upper.rotation,rotation,weight);rotation=Quaternion.FromToRotation(arm.hand.position-arm.lower.position,shoulder+axis*d-arm.lower.position)*arm.lower.rotation;arm.lower.rotation=Quaternion.Slerp(arm.lower.rotation,rotation,weight);
 }
 void Neutral(Arm arm){if(arm.upper)Reach(arm,arm.upper.position+transform.right*(.08f*arm.side)-transform.up*.51f+transform.forward*.035f,arm.upper.position+transform.right*(.24f*arm.side)-transform.up*.2f,1);}
 void SmoothArm(Arm arm,float delta){if(!arm.upper||!arm.lower)return;float k=1-Mathf.Exp(-delta*10);arm.upper.localRotation=Quaternion.Slerp(arm.previousUpper,arm.upper.localRotation,k);arm.lower.localRotation=Quaternion.Slerp(arm.previousLower,arm.lower.localRotation,k);arm.previousUpper=arm.upper.localRotation;arm.previousLower=arm.lower.localRotation;}
 void LateUpdate(){Evaluate(Time.unscaledTime,Time.unscaledDeltaTime);}
 public void Evaluate(float t,float delta){
  if(!owner)return;bool thinking=owner.Thinking;string gesture=owner.Welcoming?"wave":thinking?"think":owner.Listening?"listen":t<owner.GestureUntil?owner.Gesture:owner.Speaking?"beat":"idle";
  if(gesture!=oldGesture){oldGesture=gesture;gestureStart=t;blend=0;}blend=Mathf.MoveTowards(blend,1,delta*3.5f);float age=t-gestureStart;
  if(head&&viewer){var local=transform.InverseTransformDirection(viewer.position-head.position);float yaw=Mathf.Clamp(Mathf.Atan2(local.x,local.z)*Mathf.Rad2Deg,-40,40);float pitch=Mathf.Clamp(-Mathf.Atan2(local.y,new Vector2(local.x,local.z).magnitude)*Mathf.Rad2Deg,-20,20);float roll=0;
   if(gesture=="think"){pitch-=10*blend;yaw+=Mathf.Sin(age*.45f)*8*blend;roll=4*blend;}else if(gesture=="listen")roll=-5*blend;else if(gesture=="nod")pitch+=Mathf.Sin(age*5)*7*Mathf.Exp(-age*.6f)*blend;else if(gesture=="shake")yaw+=Mathf.Sin(age*5)*10*Mathf.Exp(-age*.6f)*blend;
   head.rotation=transform.rotation*Quaternion.Euler(pitch,yaw,roll)*headRest;
  }
  Rest(right);Rest(left);
  Neutral(right);Neutral(left);
  float beat=Mathf.Sin(age*2.5f)*.025f;var shoulder=right.upper?right.upper.position:transform.position+Vector3.up;
  if(gesture=="think"&&head)Reach(right,head.position-transform.up*.20f+transform.forward*.22f,shoulder+transform.right*(.32f*right.side)-transform.up*.25f,blend*.95f);
  else if(gesture=="wave")Reach(right,shoulder+transform.right*(.22f*right.side)+transform.up*(.28f+Mathf.Sin(age*5)*.035f)+transform.forward*.12f,shoulder+transform.right*(.3f*right.side),blend);
  else if(gesture=="point")Reach(right,shoulder+(owner.PointTarget-shoulder).normalized*.50f,shoulder+transform.right*(.28f*right.side)-transform.up*.2f,blend*.85f);
  else if(gesture=="explain"||gesture=="shrug"||gesture=="beat"&&owner.Speaking){Reach(right,shoulder+transform.right*(.22f*right.side)-transform.up*(gesture=="shrug"?.08f:.23f)+transform.forward*(.30f+beat),shoulder+transform.right*(.32f*right.side)-transform.up*.25f,blend*(gesture=="beat"?.4f:1));if(gesture=="shrug"&&left.upper)Reach(left,left.upper.position+transform.right*(.2f*left.side)-transform.up*.08f+transform.forward*.25f,left.upper.position+transform.right*(.3f*left.side)-transform.up*.2f,blend);}
  if(gesture=="think"&&right.hand&&rightMiddle){var direction=rightMiddle.position-right.hand.position;var desired=transform.up*.85f-transform.forward*.5f;right.hand.rotation=Quaternion.Slerp(right.hand.rotation,Quaternion.FromToRotation(direction,desired)*right.hand.rotation,blend);}
  SmoothArm(right,delta);SmoothArm(left,delta);
  if(smile){ScaleVertical(smile,smileRest,.45f+owner.MouthOpen*1.1f);var horizontal=smile.InverseTransformDirection(transform.right);horizontal=new Vector3(Mathf.Abs(horizontal.x),Mathf.Abs(horizontal.y),Mathf.Abs(horizontal.z));var scale=smile.localScale;if(horizontal.x>horizontal.y&&horizontal.x>horizontal.z)scale.x=smileRest.x*owner.MouthWidth;else if(horizontal.z>horizontal.y)scale.z=smileRest.z*owner.MouthWidth;else scale.y=smileRest.y*owner.MouthWidth;smile.localScale=scale;}
  float blink=Mathf.Repeat(t,4.7f);float openness=blink<.16f?Mathf.Max(.10f,Mathf.Abs(blink-.08f)/.08f):1;
  if(leftEye){ScaleVertical(leftEye,leftRest,openness);leftEye.localPosition=leftEyePosition+leftEye.parent.InverseTransformVector(transform.up*(.008f*(thinking?blend:0)));}if(rightEye){ScaleVertical(rightEye,rightRest,openness);rightEye.localPosition=rightEyePosition+rightEye.parent.InverseTransformVector(transform.up*(.008f*(thinking?blend:0)));}
  for(int i=0;i<4;i++)if(legs[i]){legs[i].localRotation=legRest[i];if(owner.Walking){float phase=t*6+(i>=2?Mathf.PI:0);float angle=i%2==0?Mathf.Sin(phase)*14:Mathf.Max(0,-Mathf.Sin(phase))*18;legs[i].rotation=Quaternion.AngleAxis(angle,transform.right)*legs[i].rotation;}}
  previousPosition=transform.position;
 }
}
