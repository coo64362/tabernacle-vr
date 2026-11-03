using UnityEngine;

public sealed class RiggedPilgrimPose : MonoBehaviour {
 Transform eye,left,right,head;Transform[] arms=new Transform[6];Quaternion headBind,leftHandBind,rightHandBind;float eyeHeight=1.30f;
 public void Initialize(Transform camera,Transform leftController,Transform rightController){eye=camera;left=leftController;right=rightController;head=RiggedGuideVisual.Find(transform,"Head");if(head)headBind=Quaternion.Inverse(transform.rotation)*head.rotation;string[] names={"LeftUpperArm","LeftLowerArm","LeftHand","RightUpperArm","RightLowerArm","RightHand"};for(int i=0;i<6;i++)arms[i]=RiggedGuideVisual.Find(transform,names[i]);if(arms[2])leftHandBind=Quaternion.Inverse(transform.rotation)*arms[2].rotation;if(arms[5])rightHandBind=Quaternion.Inverse(transform.rotation)*arms[5].rotation;}
 public void Pose(float standingHeight){
  if(!eye)return;float scale=standingHeight/eyeHeight;transform.localScale=Vector3.one*scale;transform.rotation=Quaternion.Euler(0,eye.eulerAngles.y,0);transform.position=eye.position-Vector3.up*eye.localPosition.y-transform.forward*.06f;
  if(head){head.rotation=eye.rotation*headBind;head.position=eye.position-eye.up*(.12f*scale)-eye.forward*(.07f*scale);}
  Solve(arms[0],arms[1],arms[2],left,transform.right*-1,leftHandBind);Solve(arms[3],arms[4],arms[5],right,transform.right,rightHandBind);
 }
 static void Solve(Transform upper,Transform lower,Transform hand,Transform controller,Vector3 outward,Quaternion gripBind){
  if(!upper||!lower||!hand||!controller)return;Vector3 shoulder=upper.position;float a=Vector3.Distance(shoulder,lower.position),b=Vector3.Distance(lower.position,hand.position);Vector3 delta=controller.position-shoulder;float distance=Mathf.Clamp(delta.magnitude,.03f,a+b-.002f);Vector3 dir=delta.normalized;Vector3 bend=Vector3.ProjectOnPlane(Vector3.down+outward*.45f,dir).normalized;float along=(a*a-b*b+distance*distance)/(2*distance);float side=Mathf.Sqrt(Mathf.Max(0,a*a-along*along));Vector3 elbow=shoulder+dir*along+bend*side;Vector3 wrist=shoulder+dir*distance;
  upper.rotation=Quaternion.FromToRotation(lower.position-upper.position,elbow-shoulder)*upper.rotation;lower.rotation=Quaternion.FromToRotation(hand.position-lower.position,wrist-lower.position)*lower.rotation;hand.rotation=controller.rotation*gripBind;
 }
}
