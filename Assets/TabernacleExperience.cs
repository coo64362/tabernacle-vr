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
 public TabernacleQuestions Questions {get;private set;}
 public string Caption {get;private set;}="";
 public string References {get;private set;}="";
 public void SetReferences(string text){if(References==text)return;References=text;tablet?.UpdateContent();}
 public static readonly string[] StationReferences={"Exodus 25–30; 40","Exodus 27:1–8; Leviticus 1:3–9; 6:8–13","Exodus 30:17–21; 38:8","Exodus 25:23–40; 30:1–9; Leviticus 24:1–9","Exodus 26:31–33; Leviticus 16:2–16","Exodus 25:10–22; Leviticus 16:11–16"};
 ControllerTablet tablet;AudioSource voice;Coroutine speech;int currentStation;
 readonly string[] clips={"welcome","altar","laver","holy-place","veil","ark"};
 public static readonly string[] Narration={
  "Hello, enquirer! Welcome to the tour of the Tabernacle of Moses. I will be your guide, as you discover the sacred furnishings and the holy vessels. I hope you're as excited as I am! Raise your left hand to read your tablet. When you are ready, choose Tour to visit a furnishing, or press A to continue.",
  "This is the bronze altar, made of acacia wood covered with bronze, with horns at its corners. Israelites brought offerings here; Aaron's priestly family handled the blood and the altar service. For a burnt offering, the worshipper laid a hand on the animal, and the priests presented its blood and burned the offering. The fire was kept burning, and ashes were carried to a clean place outside the camp. You can see bronze ash pots, shovels, basins, forks and firepans. Read Exodus 27:1–8, Leviticus 1:3–9 and Leviticus 6:8–13. This tour does not reenact sacrifices.",
  "This is the bronze washing basin and its bronze stand, between the altar and the tent. They were made from the mirrors of women who served at the entrance. Aaron and his sons washed their hands and feet before entering the tent or serving at the altar. Exodus does not give the basin's measurements or exact shape. Read Exodus 30:17–21 and Exodus 38:8. Our clear water reflection lets you see your chosen avatar; that is an educational feature.",
  "This is the Holy Place, where priests served. The pure gold lampstand burned olive oil; Aaron tended its lamps. The bread table was acacia wood covered with gold. Twelve loaves were arranged before the LORD with frankincense, replaced each Sabbath, and the removed bread belonged to Aaron and his sons to eat in a holy place. The incense altar was also acacia wood covered with gold, with horns at its corners. Aaron burned fragrant incense morning and evening as he tended the lamps. Read Exodus 25:23–40, Exodus 30:1–9 and Leviticus 24:1–9. You can ask me about any of these furnishings.",
  "This veil separates the Holy Place from the Most Holy Place. It was made of blue, purple and scarlet yarn and fine twisted linen, with cherubim worked into it. Their detailed appearance is not given, so our artwork is interpretive. Ordinary priestly service did not permit going beyond it whenever one wished. Leviticus 16 describes the high priest's prescribed preparation, incense and blood offerings for atonement. Read Exodus 26:31–33 and Leviticus 16:2–16. Choose Most Holy Place on your tablet to enter our educational view.",
  "This is the ark of the covenant: an acacia wood chest covered inside and outside with gold. It held the testimony given to Moses. Its cover was pure gold, with two cherubim spreading their wings above it; their faces turned toward the cover. The high priest entered this chamber under the prescribed conditions for atonement, bringing incense and sprinkling blood before and on the cover. Read Exodus 25:10–22 and Leviticus 16:11–16. Our visitor access is educational. Viewing assistance on your tablet can help you see higher without changing the ark's dimensions."
 };
 public void Initialize(Camera eye,Transform left,Transform right,ControllerTablet controllerTablet){
  tablet=controllerTablet;tablet.Experience=this;
  GuideMuted=PlayerPrefs.GetInt("GuideMuted",0)!=0;Captions=PlayerPrefs.GetInt("GuideCaptions",1)!=0;GuideVisible=PlayerPrefs.GetInt("GuideVisible",1)!=0;TurnAngle=PlayerPrefs.GetInt("TurnAngle",30);
  Avatar=new GameObject("Pilgrim avatar").AddComponent<PilgrimAvatar>();Avatar.transform.SetParent(transform,false);Avatar.Initialize(eye.transform,left,right);
  Guide=new GameObject("Sandy robot guide").AddComponent<RobotGuide>();Guide.transform.SetParent(transform,false);Guide.Initialize(eye.transform);SetGuideVisible(GuideVisible);
  voice=Guide.gameObject.AddComponent<AudioSource>();voice.playOnAwake=false;voice.spatialBlend=.35f;voice.minDistance=1.5f;voice.maxDistance=12;voice.rolloffMode=AudioRolloffMode.Linear;voice.priority=32;voice.volume=.85f;
  Reflection=new GameObject("Laver reflection system").AddComponent<LaverReflection>();Reflection.transform.SetParent(transform,false);Reflection.Initialize(eye);Reflection.Enabled=PlayerPrefs.GetInt("AvatarReflection",1)!=0;
  Questions=gameObject.AddComponent<TabernacleQuestions>();Questions.Initialize(this,tablet);
  IntroductionPlaying=true;speech=StartCoroutine(Speak(0,true,true));
 }
 public void SetGuideMuted(bool muted){GuideMuted=muted;PlayerPrefs.SetInt("GuideMuted",muted?1:0);PlayerPrefs.Save();if(voice){voice.mute=muted;TabernacleMusic.Instance?.SetGuideSpeaking(!muted&&voice.isPlaying);}}
 public void SetCaptions(bool enabled){Captions=enabled;PlayerPrefs.SetInt("GuideCaptions",enabled?1:0);PlayerPrefs.Save();tablet?.UpdateContent();}
 public void SetGuideVisible(bool visible){GuideVisible=visible;PlayerPrefs.SetInt("GuideVisible",visible?1:0);PlayerPrefs.Save();foreach(var renderer in Guide.GetComponentsInChildren<Renderer>())renderer.enabled=visible;}
 public void SetReflection(bool enabled){Reflection.Enabled=enabled;PlayerPrefs.SetInt("AvatarReflection",enabled?1:0);PlayerPrefs.Save();}
 public void CycleTurn(){TurnAngle=TurnAngle==30?45:30;PlayerPrefs.SetInt("TurnAngle",(int)TurnAngle);PlayerPrefs.Save();}
 public void StationChanged(int station){currentStation=station;Guide.SetStation(station);BeginSpeech(station,false);}
 public void Replay(){BeginSpeech(currentStation,currentStation==0);}
 public void Skip(){Questions?.Cancel();EndSpeech();Debug.Log("TABERNACLE_INTRO unlocked by skip");}
 void BeginSpeech(int station,bool introduction){Questions?.Cancel();EndSpeech();IntroductionPlaying=introduction;speech=StartCoroutine(Speak(station,introduction,false));}
 IEnumerator Speak(int station,bool introduction,bool delay){
  if(delay){float waited=0;while(waited<1.5f){if(Application.isFocused||Application.isBatchMode)waited+=Time.unscaledDeltaTime;yield return null;}}
  if(delay)Avatar.Calibrate();
  SetReferences(StationReferences[station]);Guide.Speaking=true;Guide.Welcoming=introduction;voice.clip=Resources.Load<AudioClip>("Audio/Guide/"+clips[station]);voice.mute=GuideMuted;
  if(voice.clip){voice.clip.LoadAudioData();float deadline=Time.realtimeSinceStartup+8;while(voice.clip.loadState==AudioDataLoadState.Loading&&Time.realtimeSinceStartup<deadline)yield return null;}
  bool hasAudio=voice.clip&&voice.clip.loadState==AudioDataLoadState.Loaded;
  float duration=hasAudio?voice.clip.length:Mathf.Max(8,Narration[station].Split(' ').Length/2.4f);
  if(hasAudio){voice.Play();Guide.SetSpeechAudio(voice);}else Debug.LogWarning("TABERNACLE_GUIDE missing narration; using timed tablet captions: "+clips[station]);
  TabernacleMusic.Instance?.SetGuideSpeaking(!GuideMuted&&hasAudio);
  string[] sentences=Narration[station].Split(new[]{". ","! "},StringSplitOptions.RemoveEmptyEntries);int length=0;foreach(var sentence in sentences)length+=sentence.Length;
  float elapsed=0;int oldIndex=-1;
  Debug.Log("TABERNACLE_GUIDE station="+station+" duration="+duration+" audio="+hasAudio+" movementLocked="+IntroductionPlaying);
  while(elapsed<duration){
   if(Application.isFocused||Application.isBatchMode){elapsed+=Time.unscaledDeltaTime;float position=Mathf.Clamp01(elapsed/duration)*length;int index=0,sum=0;for(int i=0;i<sentences.Length;i++){sum+=sentences[i].Length;index=i;if(position<sum)break;}
    if(index!=oldIndex){Guide.PlaySpeechGesture(sentences[index],Mathf.Max(1,duration/sentences.Length));Caption=sentences[index].TrimEnd('.','!')+(index==0&&introduction?"!":".");tablet.UpdateContent();oldIndex=index;}}
   yield return null;
  }
  voice.Stop();Guide.SetSpeechAudio(null);Guide.Speaking=false;Guide.Welcoming=false;IntroductionPlaying=false;Caption="";speech=null;TabernacleMusic.Instance?.SetGuideSpeaking(false);tablet.UpdateContent();Debug.Log("TABERNACLE_GUIDE complete movementUnlocked=true");
 }
 void EndSpeech(){if(speech!=null){StopCoroutine(speech);speech=null;}if(voice)voice.Stop();if(Guide){Guide.SetSpeechAudio(null);Guide.Speaking=false;Guide.Welcoming=false;}IntroductionPlaying=false;Caption="";TabernacleMusic.Instance?.SetGuideSpeaking(false);tablet?.UpdateContent();}
 public void StopForQuestion(){EndSpeech();}
 public void SetQuestionCaption(string text){Caption=text;tablet?.UpdateContent();}
 void LateUpdate(){Avatar?.UpdatePose();Reflection?.RenderIfNeeded();}
 void OnApplicationPause(bool paused){if(voice){if(paused)voice.Pause();else voice.UnPause();}}
 void OnDestroy(){EndSpeech();}
}
