using System;
using System.Collections;
using UnityEngine;

public sealed class TabernacleExperience : MonoBehaviour {
 public bool IntroductionPlaying {get;private set;}
 public bool GuideMuted {get;private set;}
 public bool Captions {get;private set;}=true;
 public bool GuideVisible {get;private set;}=true;
 public float TurnAngle {get;private set;}=30;
 public PilgrimAvatar Avatar {get;private set;}
 public LaverReflection Reflection {get;private set;}
 public RobotGuide Guide {get;private set;}
 public string Caption {get;private set;}="";
 ControllerTablet tablet;AudioSource voice;Coroutine speech;int currentStation;
 readonly string[] clips={"welcome","altar","laver","holy-place","veil","ark"};
 public static readonly string[] Narration={
  "Hello, enquirer! Welcome to the tour of the Tabernacle of Moses. I will be your guide, as you discover the sacred furnishings and the holy vessels. I hope you're as excited as I am! Raise your left hand to read your tablet. When you are ready, choose Tour to visit a furnishing, or press A to continue.",
  "This is the bronze altar in the courtyard. Exodus describes it as five cubits square and three cubits high, with a horn at each corner. Its bronze utensils include pots for ashes, shovels, basins, forks and firepans. You can move around the altar to examine these details. This reconstruction shows the furnishings without reenacting the sacrifices.",
  "This is the bronze washing basin, between the altar and the tent. The priests washed their hands and feet here. Exodus says that the basin and its stand were made from the bronze mirrors of the women who served at the entrance. Its exact size and shape are not specified. Look into the water to see your chosen avatar. The clear reflection is an educational feature of our reconstruction.",
  "Welcome to the Holy Place. To the south is the golden lampstand, with seven oil lamps. To the north, twelve loaves are arranged on the table, in two stacks of six. Before the inner veil stands the golden incense altar. It is one cubit square and two cubits high, with horns at its corners. Incense was burned here morning and evening. The lamps, bread and incense each had prescribed duties and utensils.",
  "This veil divides the Holy Place from the Most Holy Place. Exodus describes blue, purple and scarlet yarn, fine linen, and cherubim worked into the fabric. The exact appearance of these figures is not described, so the artwork here is interpretive. Historically, access beyond the veil was restricted. For this educational tour, choose Most Holy Place on your tablet, read the explanation, and confirm that you wish to enter.",
  "You are now in an educational view of the Most Holy Place. The ark was two and a half cubits long, one and a half cubits wide, and one and a half cubits high. Its gold cover had two cherubim, with wings spread above it and faces turned toward the cover. The precise appearance of their faces is not given. Our figures are interpretive. If you need a better view, choose Viewing assistance in the tablet's Comfort settings. Historically, entry to this chamber was restricted, as described in Leviticus sixteen."
 };
 public void Initialize(Camera eye,Transform left,Transform right,ControllerTablet controllerTablet){
  tablet=controllerTablet;tablet.Experience=this;
  GuideMuted=PlayerPrefs.GetInt("GuideMuted",0)!=0;Captions=PlayerPrefs.GetInt("GuideCaptions",1)!=0;GuideVisible=PlayerPrefs.GetInt("GuideVisible",1)!=0;TurnAngle=PlayerPrefs.GetInt("TurnAngle",30);
  Avatar=new GameObject("Pilgrim avatar").AddComponent<PilgrimAvatar>();Avatar.transform.SetParent(transform,false);Avatar.Initialize(eye.transform,left,right);
  Guide=new GameObject("Sandy robot guide").AddComponent<RobotGuide>();Guide.transform.SetParent(transform,false);Guide.Initialize(eye.transform);SetGuideVisible(GuideVisible);
  voice=Guide.gameObject.AddComponent<AudioSource>();voice.playOnAwake=false;voice.spatialBlend=.35f;voice.minDistance=1.5f;voice.maxDistance=12;voice.rolloffMode=AudioRolloffMode.Linear;voice.priority=32;voice.volume=.85f;
  Reflection=new GameObject("Laver reflection system").AddComponent<LaverReflection>();Reflection.transform.SetParent(transform,false);Reflection.Initialize(eye);Reflection.Enabled=PlayerPrefs.GetInt("AvatarReflection",1)!=0;
  IntroductionPlaying=true;speech=StartCoroutine(Speak(0,true,true));
 }
 public void SetGuideMuted(bool muted){GuideMuted=muted;PlayerPrefs.SetInt("GuideMuted",muted?1:0);PlayerPrefs.Save();if(voice){voice.mute=muted;TabernacleMusic.Instance?.SetGuideSpeaking(!muted&&voice.isPlaying);}}
 public void SetCaptions(bool enabled){Captions=enabled;PlayerPrefs.SetInt("GuideCaptions",enabled?1:0);PlayerPrefs.Save();tablet?.UpdateContent();}
 public void SetGuideVisible(bool visible){GuideVisible=visible;PlayerPrefs.SetInt("GuideVisible",visible?1:0);PlayerPrefs.Save();foreach(var renderer in Guide.GetComponentsInChildren<Renderer>())renderer.enabled=visible;}
 public void SetReflection(bool enabled){Reflection.Enabled=enabled;PlayerPrefs.SetInt("AvatarReflection",enabled?1:0);PlayerPrefs.Save();}
 public void CycleTurn(){TurnAngle=TurnAngle==30?45:30;PlayerPrefs.SetInt("TurnAngle",(int)TurnAngle);PlayerPrefs.Save();}
 public void StationChanged(int station){currentStation=station;Guide.SetStation(station);BeginSpeech(station,false);}
 public void Replay(){BeginSpeech(currentStation,currentStation==0);}
 public void Skip(){EndSpeech();Debug.Log("TABERNACLE_INTRO unlocked by skip");}
 void BeginSpeech(int station,bool introduction){EndSpeech();IntroductionPlaying=introduction;speech=StartCoroutine(Speak(station,introduction,false));}
 IEnumerator Speak(int station,bool introduction,bool delay){
  if(delay){float waited=0;while(waited<1.5f){if(Application.isFocused||Application.isBatchMode)waited+=Time.unscaledDeltaTime;yield return null;}}
  if(delay)Avatar.Calibrate();
  Guide.Speaking=true;Guide.Welcoming=introduction;voice.clip=Resources.Load<AudioClip>("Audio/Guide/"+clips[station]);voice.mute=GuideMuted;
  if(voice.clip){voice.clip.LoadAudioData();float deadline=Time.realtimeSinceStartup+8;while(voice.clip.loadState==AudioDataLoadState.Loading&&Time.realtimeSinceStartup<deadline)yield return null;}
  bool hasAudio=voice.clip&&voice.clip.loadState==AudioDataLoadState.Loaded;
  float duration=hasAudio?voice.clip.length:Mathf.Max(8,Narration[station].Split(' ').Length/2.4f);
  if(hasAudio)voice.Play();else Debug.LogWarning("TABERNACLE_GUIDE missing narration; using timed tablet captions: "+clips[station]);
  TabernacleMusic.Instance?.SetGuideSpeaking(!GuideMuted&&hasAudio);
  string[] sentences=Narration[station].Split(new[]{". ","! "},StringSplitOptions.RemoveEmptyEntries);int length=0;foreach(var sentence in sentences)length+=sentence.Length;
  float elapsed=0;int oldIndex=-1;
  Debug.Log("TABERNACLE_GUIDE station="+station+" duration="+duration+" audio="+hasAudio+" movementLocked="+IntroductionPlaying);
  while(elapsed<duration){
   if(Application.isFocused||Application.isBatchMode){elapsed+=Time.unscaledDeltaTime;float position=Mathf.Clamp01(elapsed/duration)*length;int index=0,sum=0;for(int i=0;i<sentences.Length;i++){sum+=sentences[i].Length;index=i;if(position<sum)break;}
    if(index!=oldIndex){Caption=sentences[index].TrimEnd('.','!')+(index==0&&introduction?"!":".");tablet.UpdateContent();oldIndex=index;}}
   yield return null;
  }
  voice.Stop();Guide.Speaking=false;Guide.Welcoming=false;IntroductionPlaying=false;Caption="";speech=null;TabernacleMusic.Instance?.SetGuideSpeaking(false);tablet.UpdateContent();Debug.Log("TABERNACLE_GUIDE complete movementUnlocked=true");
 }
 void EndSpeech(){if(speech!=null){StopCoroutine(speech);speech=null;}if(voice)voice.Stop();if(Guide){Guide.Speaking=false;Guide.Welcoming=false;}IntroductionPlaying=false;Caption="";TabernacleMusic.Instance?.SetGuideSpeaking(false);tablet?.UpdateContent();}
 void LateUpdate(){Avatar?.UpdatePose();Reflection?.RenderIfNeeded();}
 void OnApplicationPause(bool paused){if(voice){if(paused)voice.Pause();else voice.UnPause();}}
 void OnDestroy(){EndSpeech();}
}
