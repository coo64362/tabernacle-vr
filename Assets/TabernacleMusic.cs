using System.Collections;
using UnityEngine;

// Non-spatial background music, separate from the guide's spatial voice.
public sealed class TabernacleMusic : MonoBehaviour {
 public static TabernacleMusic Instance { get; private set; }
 AudioSource source;
 readonly string[] tracks={"Audio/ItIsWellInstrumental","Audio/GraceGreaterInstrumental"};
 bool appPaused;
 public int CurrentTrack {get; private set;}
 float level; bool muted, guideSpeaking;
 public float Volume => level;
 public bool Muted => muted;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void Bootstrap(){
  if(FindFirstObjectByType<TabernacleWorld>() && !FindFirstObjectByType<TabernacleMusic>())
   new GameObject("Instrumental hymn background").AddComponent<TabernacleMusic>();
 }
 void Awake(){
  if(Instance && Instance!=this){Destroy(gameObject);return;}
  Instance=this;
  level=Mathf.Clamp01(PlayerPrefs.GetFloat("MusicVolume",.16f));
  muted=PlayerPrefs.GetInt("MusicMuted",0)!=0;
  source=gameObject.AddComponent<AudioSource>();
  source.loop=false;source.playOnAwake=false;source.spatialBlend=0;
  source.priority=192;source.volume=0;
  StartCoroutine(Playlist());
 }
 IEnumerator Playlist(){
  while(true){
   source.clip=Resources.Load<AudioClip>(tracks[CurrentTrack]);
   if(source.clip){
    source.clip.LoadAudioData();
    float deadline=Time.realtimeSinceStartup+10;
    while(source.clip.loadState==AudioDataLoadState.Loading && Time.realtimeSinceStartup<deadline)yield return null;
    if(source.clip.loadState==AudioDataLoadState.Loaded){
     source.Play();yield return null;
     while(source.isPlaying||appPaused||AudioListener.pause)yield return null;
    }else Debug.LogWarning("TABERNACLE_MUSIC could not load "+tracks[CurrentTrack]);
   }else Debug.LogWarning("TABERNACLE_MUSIC missing "+tracks[CurrentTrack]);
   CurrentTrack=(CurrentTrack+1)%tracks.Length;
   yield return new WaitForSecondsRealtime(1);
  }
 }
 void OnApplicationPause(bool paused){appPaused=paused;}
 void Update(){
  float target=muted?0:level*(guideSpeaking?.22f:1);
  source.volume=Mathf.MoveTowards(source.volume,target,Time.unscaledDeltaTime*.16f);
 }
 // Tablet settings and guide playback call these; no microphone data is needed.
 public void SetVolume(float value){level=Mathf.Clamp01(value);PlayerPrefs.SetFloat("MusicVolume",level);PlayerPrefs.Save();}
 public void SetMuted(bool value){muted=value;PlayerPrefs.SetInt("MusicMuted",value?1:0);PlayerPrefs.Save();}
 public void SetGuideSpeaking(bool speaking){guideSpeaking=speaking;}
 void OnDestroy(){if(Instance==this)Instance=null;}
}
