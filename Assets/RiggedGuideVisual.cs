using UnityEngine;

public sealed class RiggedGuideVisual : MonoBehaviour {
 Transform viewer,head,upper,lower,smile,leftEye,rightEye;Quaternion headRest,upperRest,lowerRest;float armSide;Vector3 smileRest,leftRest,rightRest;RobotGuide owner;
 public static Transform Find(Transform root,string name){foreach(var t in root.GetComponentsInChildren<Transform>(true))if(t.name==name)return t;return null;}
 public void Initialize(RobotGuide guide,Transform user){owner=guide;viewer=user;head=Find(transform,"Head");upper=Find(transform,"RightUpperArm");lower=Find(transform,"RightLowerArm");smile=Find(transform,"Smile");leftEye=Find(transform,"Eye_Left");rightEye=Find(transform,"Eye_Right");if(head)headRest=Quaternion.Inverse(transform.rotation)*head.rotation;if(upper)upperRest=Quaternion.Inverse(transform.rotation)*upper.rotation;if(lower)lowerRest=lower.localRotation;armSide=upper?Mathf.Sign(transform.InverseTransformPoint(upper.position).x):1;if(smile)smileRest=smile.localScale;if(leftEye)leftRest=leftEye.localScale;if(rightEye)rightRest=rightEye.localScale;}
 void ScaleVertical(Transform part,Vector3 rest,float value){var axis=part.InverseTransformDirection(transform.up);axis=new Vector3(Mathf.Abs(axis.x),Mathf.Abs(axis.y),Mathf.Abs(axis.z));var scale=Vector3.one;if(axis.x>axis.y&&axis.x>axis.z)scale.x=value;else if(axis.z>axis.y)scale.z=value;else scale.y=value;part.localScale=Vector3.Scale(rest,scale);}
 void LateUpdate(){
  if(!owner)return;float t=Time.unscaledTime;
  if(head&&viewer){var local=transform.InverseTransformDirection(viewer.position-head.position);float yaw=Mathf.Clamp(Mathf.Atan2(local.x,local.z)*Mathf.Rad2Deg,-40,40);float pitch=Mathf.Clamp(-Mathf.Atan2(local.y,new Vector2(local.x,local.z).magnitude)*Mathf.Rad2Deg,-20,20);head.rotation=transform.rotation*Quaternion.Euler(pitch,yaw,0)*headRest;}
  float lift=owner.Welcoming?30:owner.Speaking?8:0;float elbow=owner.Welcoming?100+Mathf.Sin(t*5)*12:owner.Speaking?15+Mathf.Sin(t*2)*5:0;
  if(upper)upper.rotation=Quaternion.AngleAxis(armSide*lift,transform.forward)*transform.rotation*upperRest;if(lower){lower.localRotation=lowerRest;lower.rotation=Quaternion.AngleAxis(armSide*elbow,transform.forward)*lower.rotation;}
  if(smile)ScaleVertical(smile,smileRest,owner.Speaking?1+Mathf.Sin(t*15)*.13f:1);
  float blink=Mathf.Repeat(t,4.7f);float openness=blink<.16f?Mathf.Max(.10f,Mathf.Abs(blink-.08f)/.08f):1;
  if(leftEye)ScaleVertical(leftEye,leftRest,openness);if(rightEye)ScaleVertical(rightEye,rightRest,openness);
 }
}
