using System.Collections.Generic;
using UnityEngine;

public sealed class PilgrimAvatar : MonoBehaviour {
 public bool Female {get;private set;}public bool ChildFace {get;private set;}
 public float Height {get;private set;}=1.65f;
 Transform eye,left,right,root,face,leftUpper,leftLower,rightUpper,rightLower,leftHand,rightHand,selfRobe;
 Transform selfLeftUpper,selfLeftLower,selfRightUpper,selfRightLower,selfLeftHand,selfRightHand;
 readonly List<Material> materials=new List<Material>();
 Material skin,linen,mantle,hair,white,iris,dark,leather;
 public void Initialize(Transform head,Transform leftController,Transform rightController){
  eye=head;left=leftController;right=rightController;
  skin=New("Warm skin",new Color(.64f,.40f,.25f));linen=New("Woven cream robe",new Color(.94f,.87f,.69f),true);mantle=New("Russet mantle",new Color(.49f,.12f,.08f),true);hair=New("Chestnut hair",new Color(.13f,.065f,.035f));white=New("Eye whites",new Color(.96f,.94f,.88f));iris=New("Brown eyes",new Color(.27f,.13f,.05f));dark=New("Facial detail",new Color(.045f,.025f,.02f));leather=New("Leather sandals",new Color(.22f,.13f,.065f));
  Height=PlayerPrefs.GetFloat("AvatarStandingHeight",1.65f);Female=PlayerPrefs.GetInt("AvatarFemale",0)!=0;ChildFace=PlayerPrefs.GetInt("AvatarChild",1)!=0;Rebuild();
 }
 Material New(string name,Color color,bool cloth=false){var m=CharacterGeometry.Material(name,color,cloth);materials.Add(m);return m;}
 public void SetFemale(bool value){Female=value;PlayerPrefs.SetInt("AvatarFemale",value?1:0);PlayerPrefs.Save();Rebuild();}
 public void SetChild(bool value){ChildFace=value;PlayerPrefs.SetInt("AvatarChild",value?1:0);PlayerPrefs.Save();Rebuild();}
 public void Calibrate(){if(eye&&eye.localPosition.y>.6f){Height=Mathf.Clamp(eye.localPosition.y,.8f,2.15f);PlayerPrefs.SetFloat("AvatarStandingHeight",Height);PlayerPrefs.Save();}}
 Transform Part(string name,Transform parent,Vector3 p,Vector3 size,Material m)=>CharacterGeometry.Oval(name,parent,p,size,m,11);
 public void Rebuild(){
  if(root){root.gameObject.SetActive(false);Release(root.gameObject);}if(selfRobe){selfRobe.gameObject.SetActive(false);Release(selfRobe.gameObject);}
  root=CharacterGeometry.Group("Chosen pilgrim — reflection",transform,Vector3.zero);root.gameObject.layer=11;
  var body=CharacterGeometry.Group("Woven clothing",root,Vector3.zero);body.gameObject.layer=11;
  CharacterGeometry.Garment("Long linen robe",body,.08f,1.25f,.31f,.225f,linen,11);
  Part("Linen shoulders",body,new Vector3(0,1.23f,0),new Vector3(.48f,.21f,.31f),linen);
  CharacterGeometry.Drape(body,mantle,11);
  Part("Linen belt",body,new Vector3(0,.91f,0),new Vector3(.48f,.06f,.32f),linen);
  Part("Neck",body,new Vector3(0,1.35f,0),new Vector3(.13f,.20f,.14f),skin);
  for(int s=-1;s<=1;s+=2)Part("Sandaled foot",body,new Vector3(s*.12f,.055f,.07f),new Vector3(.15f,.10f,.29f),leather);
  CharacterGeometry.Combine(body);
  face=CharacterGeometry.Group("Original animated pilgrim face",root,new Vector3(0,1.52f,0));face.gameObject.layer=11;
  float h=ChildFace?.39f:.37f,w=ChildFace?.30f:.25f;
  Part("Face",face,Vector3.zero,new Vector3(w,h,.27f),skin);
  Part("Hair crown",face,new Vector3(0,.125f,-.025f),new Vector3(w*1.05f,.21f,.28f),hair);
  for(int s=-1;s<=1;s+=2){
   Part("Ear",face,new Vector3(s*w*.48f,-.01f,0),new Vector3(.055f,.10f,.065f),skin);
   Part("Swept hair",face,new Vector3(s*w*.34f,.09f,.065f),new Vector3(.12f,.14f,.16f),hair).localRotation=Quaternion.Euler(0,0,s*25);
   float eyeY=ChildFace?.015f:.025f,eyeX=w*.25f,eyeW=ChildFace?.083f:.066f;
   Part("Almond eye",face,new Vector3(s*eyeX,eyeY,.12f),new Vector3(eyeW,.045f,.026f),white);
   Part("Iris",face,new Vector3(s*eyeX,eyeY,.134f),new Vector3(.032f,.037f,.012f),iris);
   Part("Pupil",face,new Vector3(s*eyeX,eyeY,.141f),new Vector3(.017f,.025f,.008f),dark);
   Part("Eye catchlight",face,new Vector3(s*eyeX-.006f,eyeY+.009f,.145f),new Vector3(.009f,.010f,.004f),white);
   Part("Expressive brow",face,new Vector3(s*eyeX,.065f,.119f),new Vector3(eyeW,.015f,.026f),hair).localRotation=Quaternion.Euler(0,0,-s*8);
   if(Female)Part("Long hair",face,new Vector3(s*.125f,-.10f,-.045f),new Vector3(.105f,.39f,.15f),hair);
  }
  Part("Nose",face,new Vector3(0,-.033f,.132f),new Vector3(.042f,.069f,.052f),skin);
  CharacterGeometry.Curve("Gentle smile",face,new[]{new Vector3(-.045f,-.092f,.119f),new Vector3(-.025f,-.104f,.129f),new Vector3(0,-.108f,.135f),new Vector3(.025f,-.104f,.129f),new Vector3(.045f,-.092f,.119f)},.007f,dark,11);
  if(!ChildFace&&!Female)Part("Short beard",face,new Vector3(0,-.14f,.04f),new Vector3(.19f,.14f,.19f),hair);
  CharacterGeometry.Combine(face);
  leftUpper=Part("Left sleeve",root,Vector3.zero,Vector3.one,linen);rightUpper=Part("Right sleeve",root,Vector3.zero,Vector3.one,linen);
  leftLower=Part("Left forearm",root,Vector3.zero,Vector3.one,skin);rightLower=Part("Right forearm",root,Vector3.zero,Vector3.one,skin);
  leftHand=Part("Left hand",root,Vector3.zero,new Vector3(.085f,.12f,.045f),skin);rightHand=Part("Right hand",root,Vector3.zero,new Vector3(.085f,.12f,.045f),skin);
  selfRobe=CharacterGeometry.Group("Your visible robe and hands",transform,Vector3.zero);
  CharacterGeometry.Garment("First person linen robe",selfRobe,.06f,1.17f,.31f,.21f,linen);
  CharacterGeometry.Drape(selfRobe,mantle);
  CharacterGeometry.Combine(selfRobe);
  selfLeftUpper=SelfPart("Your left sleeve",linen);selfRightUpper=SelfPart("Your right sleeve",linen);selfLeftLower=SelfPart("Your left forearm",skin);selfRightLower=SelfPart("Your right forearm",skin);
  selfLeftHand=SelfPart("Your left hand",skin);selfRightHand=SelfPart("Your right hand",skin);selfLeftHand.localScale=selfRightHand.localScale=new Vector3(.085f,.12f,.045f);
 }
 Transform SelfPart(string name,Material material)=>CharacterGeometry.Oval(name,selfRobe,Vector3.zero,Vector3.one,material);
 public void UpdatePose(){
  if(!eye||!root)return;
  float height=eye.localPosition.y;float scale=Height/1.58f;
  root.rotation=Quaternion.Euler(0,eye.eulerAngles.y,0);float lean=Mathf.InverseLerp(25,75,Mathf.DeltaAngle(0,eye.eulerAngles.x))*.20f;var floor=eye.position-Vector3.up*height-root.forward*(.20f+lean);root.position=floor;root.localScale=Vector3.one*scale;
  // The selected face follows the user's real head; choosing a child face never shrinks their viewpoint.
  face.position=eye.position-eye.forward*(.141f*scale)-eye.up*((ChildFace?.015f:.025f)*scale);face.rotation=eye.rotation;
  selfRobe.position=floor-root.forward*.06f;selfRobe.rotation=root.rotation;selfRobe.localScale=Vector3.one*scale;
  Arm(-1,left,leftUpper,leftLower,leftHand,scale);Arm(1,right,rightUpper,rightLower,rightHand,scale);
  Arm(-1,left,selfLeftUpper,selfLeftLower,selfLeftHand,scale);Arm(1,right,selfRightUpper,selfRightLower,selfRightHand,scale);
 }
 void Arm(int side,Transform controller,Transform upper,Transform lower,Transform hand,float scale){
  var shoulder=root.TransformPoint(new Vector3(side*.23f,1.22f,0));var wrist=controller.position;var elbow=Vector3.Lerp(shoulder,wrist,.55f)-Vector3.up*.12f+root.right*side*.06f;
  CharacterGeometry.Segment(upper,shoulder,elbow,.14f);CharacterGeometry.Segment(lower,elbow,wrist,.075f);hand.position=wrist;hand.rotation=controller.rotation;
 }
 static void Release(Object item){if(Application.isPlaying)Destroy(item);else DestroyImmediate(item);}
 void OnDestroy(){foreach(var material in materials)if(material)Destroy(material);}
}
