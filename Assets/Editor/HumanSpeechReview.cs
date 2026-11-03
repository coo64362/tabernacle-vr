using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class HumanSpeechReview {
 static GameObject model;static HumanSpeechFace driver;static SkinnedMeshRenderer mouth;static double started,last;static int step;static float maximum;static float[] voiceSamples;static int voiceCursor;static readonly System.Collections.Generic.HashSet<int> usedShapes=new System.Collections.Generic.HashSet<int>();static readonly float[] samples=new float[320];
 static HumanSpeechReview(){if(SessionState.GetBool("SpeechReviewRunning",false))EditorApplication.update+=Tick;}
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 public static void Import(){CharacterRigImport.Run();Validate();}
 public static void Review(){CharacterRigImport.Run();Render();RunVoice();}
 public static void ValidateAndBuild(){Validate();ConversationReview.Run();QuestBuild.Build();}
 public static void Validate(){
  var prefab=Resources.Load<GameObject>("Characters/FemaleChildReady");Check(prefab,"Missing female prefab");var face=prefab.GetComponentsInChildren<SkinnedMeshRenderer>().FirstOrDefault(s=>s.sharedMesh.blendShapeCount>=6);Check(face,"Speech blend shapes missing");
  foreach(string sound in new[]{"A","I","U","E","O","N"})Check(Enumerable.Range(0,face.sharedMesh.blendShapeCount).Any(i=>face.sharedMesh.GetBlendShapeName(i).EndsWith("Viseme_"+sound)),"Missing shape "+sound);
  foreach(string name in new[]{"MouthInterior","MouthTeeth","MouthTongue"}){var renderer=prefab.GetComponentsInChildren<Renderer>().FirstOrDefault(r=>r.name==name);Check(renderer&&renderer.sharedMaterial&&renderer.sharedMaterial.name==name,"Mouth surface missing: "+name);}
  var first=Resources.Load<GameObject>("Characters/FemaleChildFirstPerson");Check(first&&first.GetComponentsInChildren<SkinnedMeshRenderer>().All(s=>s.sharedMesh.blendShapeCount==0),"Unused first-person mouth shapes retained");
  Check(Resources.Load<uLipSync.Profile>("Speech/VisitorProfile"),"Missing phoneme profile");
  // Reject missing, empty, or non-finite imported morph data.
  var mesh=face.sharedMesh;var delta=new Vector3[mesh.vertexCount];for(int i=0;i<mesh.blendShapeCount;i++){mesh.GetBlendShapeFrameVertices(i,0,delta,null,null);Check(delta.All(v=>float.IsFinite(v.x)&&float.IsFinite(v.y)&&float.IsFinite(v.z)),"Non-finite shape");Check(delta.Max(v=>v.magnitude)>0,"Empty shape");}
  Directory.CreateDirectory("Documentation/Review-v0.7");File.WriteAllText("Documentation/Review-v0.7/rig-check.txt","Six imported speech shapes; three skinned inner-mouth surfaces; material assignments; finite, nonempty deformations: PASS\n");Debug.Log("HUMAN_SPEECH_RIG_PASS");
 }
 public static void Render(){
  Validate();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var avatar=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Characters/FemaleChildReady"));
  foreach(var t in avatar.GetComponentsInChildren<Transform>())t.gameObject.layer=12;
  var camera=new GameObject("Speech face review").AddComponent<Camera>();camera.transform.position=new Vector3(0,1.31f,2);camera.transform.LookAt(new Vector3(0,1.31f,0));camera.orthographic=true;camera.orthographicSize=.205f;camera.cullingMask=1<<12;camera.backgroundColor=new Color(.85f,.81f,.76f);camera.clearFlags=CameraClearFlags.SolidColor;
  var light=new GameObject("Face review light").AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.1f;light.transform.rotation=Quaternion.Euler(30,-25,0);RenderSettings.ambientLight=new Color(.6f,.6f,.6f);
  foreach(string sound in new[]{"Rest","A","I","U","O"}){
   var skins=avatar.GetComponentsInChildren<SkinnedMeshRenderer>();foreach(var skin in skins)for(int i=0;i<skin.sharedMesh.blendShapeCount;i++)skin.SetBlendShapeWeight(i,skin.sharedMesh.GetBlendShapeName(i).EndsWith("Viseme_"+sound)?100:0);
   var previews=new System.Collections.Generic.List<GameObject>();foreach(var skin in skins){var mesh=new Mesh();skin.BakeMesh(mesh);var g=new GameObject("Baked mouth review");g.layer=12;g.transform.SetPositionAndRotation(skin.transform.position,skin.transform.rotation);g.transform.localScale=Vector3.one;g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterials=skin.sharedMaterials;skin.enabled=false;previews.Add(g);}
   ExperienceReview.Capture(camera,"Documentation/Review-v0.7/unity-"+sound+".png",700,700);foreach(var g in previews){UnityEngine.Object.DestroyImmediate(g.GetComponent<MeshFilter>().sharedMesh);UnityEngine.Object.DestroyImmediate(g);}foreach(var skin in skins)skin.enabled=true;
  }
  Debug.Log("HUMAN_SPEECH_VISUAL_CAPTURED");
 }
 public static void RunVoice(){SessionState.SetString("SpeechReviewVoice",Environment.GetEnvironmentVariable("TABERNACLE_SPEECH_REVIEW_WAV")??"");Run();}
 public static void Run(){Validate();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);SessionState.SetBool("SpeechReviewRunning",true);EditorApplication.update+=Tick;EditorApplication.EnterPlaymode();}
 static void Tick(){
  if(!EditorApplication.isPlaying||EditorApplication.isCompiling)return;
  try{
   if(!model){model=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Characters/FemaleChildReady"));driver=model.AddComponent<HumanSpeechFace>();driver.Initialize();mouth=model.GetComponentsInChildren<SkinnedMeshRenderer>().First(s=>s.sharedMesh.blendShapeCount>=6);Check(driver.HasSpeechShapes,"Driver did not find morphs");
    string path=SessionState.GetString("SpeechReviewVoice","");if(!string.IsNullOrEmpty(path)){using(var reader=new BinaryReader(File.OpenRead(path))){Check(new string(reader.ReadChars(4))=="RIFF","Review audio must be PCM WAV");reader.ReadInt32();Check(new string(reader.ReadChars(4))=="WAVE","WAV header missing");while(reader.BaseStream.Position+8<reader.BaseStream.Length){string id=new string(reader.ReadChars(4));int size=reader.ReadInt32();if(id=="data"){voiceSamples=new float[size/2];for(int i=0;i<voiceSamples.Length;i++)voiceSamples[i]=reader.ReadInt16()/32768f;break;}reader.BaseStream.Position+=size+(size%2);}Check(voiceSamples!=null,"No PCM data");}}
    driver.BeginCapture();started=last=EditorApplication.timeSinceStartup;}
   double now=EditorApplication.timeSinceStartup,elapsed=now-started;
   if(step==0){
    if(now-last>=.02){last=now;for(int i=0;i<320;i++){double t=elapsed+i/16000.0;samples[i]=voiceSamples!=null?(voiceCursor<voiceSamples.Length?voiceSamples[voiceCursor++]:0):.07f*(float)(Math.Sin(2*Math.PI*170*t)+.5*Math.Sin(2*Math.PI*850*t)+.3*Math.Sin(2*Math.PI*1700*t));}driver.Feed(samples,16000);}
    driver.ApplyPose();float sum=0;for(int i=0;i<mouth.sharedMesh.blendShapeCount;i++){float weight=mouth.GetBlendShapeWeight(i);Check(float.IsFinite(weight)&&weight>=0&&weight<=100.01f,"Invalid speech weight");sum+=weight;}Check(sum<=100.1f,"Blend weights exceed full expression");maximum=Mathf.Max(maximum,sum);if(sum>10)usedShapes.Add(Enumerable.Range(0,mouth.sharedMesh.blendShapeCount).OrderByDescending(i=>mouth.GetBlendShapeWeight(i)).First());
    if(voiceSamples!=null?voiceCursor>=voiceSamples.Length:elapsed>3){Check(maximum>5,"Audio did not move the mouth");driver.EndCapture();step=1;started=now;}
   }else if(step==1){driver.ApplyPose();Check(Enumerable.Range(0,mouth.sharedMesh.blendShapeCount).All(i=>mouth.GetBlendShapeWeight(i)<.001f),"Mouth did not close after capture");driver.BeginCapture();Array.Clear(samples,0,320);driver.Feed(samples,16000);step=2;started=now;
   }else{driver.ApplyPose();if(now-started>.4){Check(Enumerable.Range(0,mouth.sharedMesh.blendShapeCount).All(i=>mouth.GetBlendShapeWeight(i)<.001f),"Silent audio opened mouth");driver.EndCapture();File.WriteAllText("Documentation/Review-v0.7/"+(voiceSamples!=null?"voice-check.txt":"audio-check.txt"),(voiceSamples!=null?"16 kHz offline synthesized speech":"16 kHz synthetic input")+"; dominant shapes observed "+usedShapes.Count+". Input through real MFCC/Burst analysis: PASS; peak summed weight "+maximum.ToString("F2")+"\nFinite normalized blend weights: PASS\nCapture stop closes mouth immediately: PASS\nSilent capture remains closed: PASS\nHuman voice phoneme accuracy, headset microphone and sustained Quest performance: NOT TESTED\n");Debug.Log("HUMAN_SPEECH_AUDIO_PASS peak="+maximum);Finish(0);}}
  }catch(Exception e){Debug.LogException(e);Finish(1);}
 }
 static void Finish(int code){EditorApplication.update-=Tick;SessionState.SetBool("SpeechReviewRunning",false);SessionState.EraseString("SpeechReviewVoice");EditorApplication.Exit(code);}
}
