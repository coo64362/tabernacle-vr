using System.Collections.Generic;
using UnityEngine;

public sealed class RobotGuide : MonoBehaviour {
 Transform head,leftArm,rightArm,mouth;Transform viewer;RiggedGuideVisual rigged;
 readonly List<Material> materials=new List<Material>();
 public bool Speaking {get;set;}public bool Welcoming {get;set;}
 public bool Thinking {get;set;}public bool Listening {get;set;}
 public AudioSource SpeechAudio {get;private set;}public string Gesture {get;private set;}="beat";public float GestureUntil {get;private set;}
 public float MouthOpen {get;private set;}public bool Walking=>follower&&follower.Walking;
 GuideFollower follower;readonly float[] audioSamples=new float[256];
 public string LocationDescription {get{if(!viewer)return "courtyard";float z=viewer.position.z;return z<26?"courtyard":z<35.144f?"Holy Place":"Most Holy Place (educational access)";}}
 public void SetSpeechAudio(AudioSource audio){SpeechAudio=audio;Speaking=audio&&audio.isPlaying;}
 public void PlayGesture(string name,float duration){Gesture=name;GestureUntil=Time.unscaledTime+Mathf.Clamp(duration,.5f,12);}
 Vector3? pointTarget;
 public Vector3 PointTarget=>pointTarget??(transform.position+transform.forward*2+Vector3.up);
 public void PlaySpeechGesture(string text,float duration){var words=(text??"").ToLowerInvariant();pointTarget=null;string furnishing=words.Contains("incense")?"incense altar":words.Contains("lamp")?"seven lampstand":words.Contains("bread")||words.Contains("table")?"bread table":words.Contains("basin")||words.Contains("wash")?"basin":words.Contains("ark")||words.Contains("cherub")?"ark":words.Contains("altar")?"bronze altar":null;if(furnishing!=null&&TabernacleWorld.Instance){foreach(Transform child in TabernacleWorld.Instance.transform)if(child.name=="Furnishing — "+furnishing){pointTarget=child.position+Vector3.up*.9f;break;}}PlayGesture(GuideSpeechGesture.FromWords(text),duration);}

 void Update(){float target=0;if(SpeechAudio&&SpeechAudio.isPlaying&&!SpeechAudio.mute){SpeechAudio.GetOutputData(audioSamples,0);float sum=0;foreach(var sample in audioSamples)sum+=sample*sample;target=Mathf.Clamp01(Mathf.Sqrt(sum/audioSamples.Length)*5);}MouthOpen=Mathf.Lerp(MouthOpen,target,1-Mathf.Exp(-Time.unscaledDeltaTime*(target>MouthOpen?30:16)));}

 Material New(string name,Color color){var m=CharacterGeometry.Material(name,color);materials.Add(m);return m;}
 Transform Part(string name,Transform parent,Vector3 p,Vector3 size,Material m)=>CharacterGeometry.Oval(name,parent,p,size,m);
 public void Initialize(Transform user){
  viewer=user;follower=gameObject.AddComponent<GuideFollower>();follower.Initialize(this,user);var accepted=Resources.Load<GameObject>("Characters/SandstoneGuideReady");if(accepted){var visual=Instantiate(accepted,transform,false);rigged=visual.AddComponent<RiggedGuideVisual>();rigged.Initialize(this,user);SetStation(0);return;}var shell=New("Sandy ceramic shell",new Color(.78f,.67f,.48f));var trim=New("Warm ivory trim",new Color(.96f,.88f,.69f));var joint=New("Graphite joints",new Color(.07f,.09f,.095f));var glow=New("Soft teal display",new Color(.25f,.82f,.77f));glow.SetFloat("_Glow",1);
  var body=CharacterGeometry.Group("Guide torso",transform,Vector3.zero);
  Part("Rounded torso",body,new Vector3(0,.90f,0),new Vector3(.44f,.55f,.29f),shell);
  Part("Chest inset",body,new Vector3(0,.97f,.125f),new Vector3(.24f,.25f,.055f),trim);
  Part("Status light",body,new Vector3(0,.99f,.159f),new Vector3(.055f,.055f,.012f),glow);
  Part("Neck joint",body,new Vector3(0,1.22f,0),new Vector3(.14f,.17f,.14f),joint);
  for(int s=-1;s<=1;s+=2){
   Part("Hip joint",body,new Vector3(s*.12f,.63f,0),new Vector3(.14f,.16f,.15f),joint);
   Part("Leg shell",body,new Vector3(s*.13f,.39f,0),new Vector3(.16f,.43f,.18f),shell);
   Part("Ankle joint",body,new Vector3(s*.13f,.16f,0),new Vector3(.10f,.13f,.11f),joint);
   Part("Stable rounded foot",body,new Vector3(s*.13f,.075f,.045f),new Vector3(.20f,.15f,.32f),trim);
  }
  CharacterGeometry.Combine(body);
  head=CharacterGeometry.Group("Expressive robot head",transform,new Vector3(0,1.43f,0));
  Part("Ceramic head",head,Vector3.zero,new Vector3(.49f,.39f,.36f),shell);
  Part("Soft black visor",head,new Vector3(0,.005f,.146f),new Vector3(.405f,.25f,.105f),joint);
  for(int s=-1;s<=1;s+=2){Part("Friendly illuminated eye",head,new Vector3(s*.10f,.04f,.197f),new Vector3(.069f,.066f,.018f),glow);Part("Side hinge",head,new Vector3(s*.245f,0,0),new Vector3(.055f,.12f,.12f),trim);}
  CharacterGeometry.Combine(head);
  mouth=CharacterGeometry.Curve("Smiling animated mouth",head,new[]{new Vector3(-.075f,-.04f,.199f),new Vector3(-.04f,-.065f,.203f),new Vector3(0,-.073f,.205f),new Vector3(.04f,-.065f,.203f),new Vector3(.075f,-.04f,.199f)},.012f,glow);
  leftArm=Arm(-1,shell,trim,joint);rightArm=Arm(1,shell,trim,joint);SetStation(0);
 }
 Transform Arm(int side,Material shell,Material trim,Material joint){
  var arm=CharacterGeometry.Group(side<0?"Left arm gesture":"Right arm wave",transform,new Vector3(side*.27f,1.10f,0));
  Part("Shoulder",arm,Vector3.zero,new Vector3(.15f,.15f,.15f),joint);
  Part("Upper arm",arm,new Vector3(side*.025f,-.15f,0),new Vector3(.13f,.24f,.15f),shell);
  Part("Elbow",arm,new Vector3(side*.03f,-.28f,0),new Vector3(.11f,.11f,.11f),joint);
  Part("Forearm",arm,new Vector3(side*.035f,-.40f,.015f),new Vector3(.12f,.20f,.14f),shell);
  Part("Rounded hand",arm,new Vector3(side*.04f,-.53f,.025f),new Vector3(.12f,.14f,.075f),trim);
  CharacterGeometry.Combine(arm);return arm;
 }
 public void SetStation(int station){
  var p=TabernacleWorld.Instance.stops[Mathf.Clamp(station,0,5)];
  Vector3[] offsets={new Vector3(1.05f,0,1.5f),new Vector3(-.2f,0,1.5f),new Vector3(-1.05f,0,1.5f),new Vector3(.95f,0,1.4f),new Vector3(.95f,0,.8f),new Vector3(-1.05f,0,.65f)};
  transform.position=p+offsets[Mathf.Clamp(station,0,5)];follower?.ResetPath();
 }
 void LateUpdate(){
  if(!viewer)return;
  var to=viewer.position-transform.position;to.y=0;if(to.sqrMagnitude>.01f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(to),Time.unscaledDeltaTime*4);
  if(rigged)return;
  var look=viewer.position-head.position;if(look.sqrMagnitude>.01f)head.rotation=Quaternion.Slerp(head.rotation,Quaternion.LookRotation(look),Time.unscaledDeltaTime*5);
  rightArm.localRotation=Quaternion.Euler(Welcoming?-12:Speaking?-18:0,0,Welcoming?155+Mathf.Sin(Time.unscaledTime*5)*15:Speaking?25+Mathf.Sin(Time.unscaledTime*2)*7:5);
  leftArm.localRotation=Quaternion.Euler(Speaking?-20:0,0,Speaking?-20:-5);
  mouth.localScale=new Vector3(1,1+MouthOpen*.55f,1);
 }
 void OnDestroy(){foreach(var m in materials)if(m)Destroy(m);}
}
