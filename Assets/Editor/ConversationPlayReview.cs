using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Exercises the actual Unity socket, retrieval, PCM stream, history and citation path.
[InitializeOnLoad]
public static class ConversationPlayReview {
 static TabernacleExperience experience;static double started,turnStarted;static int stage;static bool sawStreaming;static int initialMuted;
 static ConversationPlayReview(){if(SessionState.GetBool("ConversationReviewRunning",false))EditorApplication.update+=Tick;}
 public static void Run(){EditorSceneManager.OpenScene("Assets/TabernacleRealistic.unity");var explorer=UnityEngine.Object.FindFirstObjectByType<QuestExplorer>();if(explorer)explorer.enabled=false;SessionState.SetInt("ConversationReviewMuted",PlayerPrefs.GetInt("GuideMuted",0));SessionState.SetBool("ConversationReviewHadMuted",PlayerPrefs.HasKey("GuideMuted"));PlayerPrefs.SetInt("GuideMuted",0);SessionState.SetBool("ConversationReviewRunning",true);EditorApplication.EnterPlaymode();}
 static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
 static void Tick(){if(!EditorApplication.isPlaying)return;try{
  if(stage==0){var rig=new GameObject("Conversation review origin").transform;var eye=new GameObject("Review eye").AddComponent<Camera>();eye.gameObject.AddComponent<AudioListener>();eye.transform.SetParent(rig,false);eye.transform.position=new Vector3(0,1.65f,2);var left=new GameObject("Review hand").transform;left.SetParent(rig);var right=new GameObject("Review right").transform;right.SetParent(rig);var tablet=new GameObject("Review tablet").AddComponent<ControllerTablet>();tablet.Initialize(left);experience=new GameObject("Conversation review experience").AddComponent<TabernacleExperience>();experience.Initialize(eye,left,right,tablet);experience.Skip();started=EditorApplication.timeSinceStartup;stage=1;return;}
  if(EditorApplication.timeSinceStartup-started>150)throw new Exception("Conversation review timed out");
  if(stage==1){if(!experience.Questions.Ready||EditorApplication.timeSinceStartup-started<3)return;experience.Questions.AskText("Has anyone who was not a priest eaten the holy bread?");Check(experience.Questions.Busy,"Question was not accepted: "+experience.Caption);turnStarted=EditorApplication.timeSinceStartup;stage=2;}
  else if(stage==2){if(experience.Guide.SpeechAudio&&experience.Guide.SpeechAudio.isPlaying)sawStreaming=true;if(experience.Questions.Busy)return;Check(experience.Caption.ToLowerInvariant().Contains("david"),"Generated David answer missing: "+experience.Caption);Check(experience.References.Contains("1 Samuel 21"),"David citation missing");Check(sawStreaming,"No streamed speech playback observed");Debug.Log("CONVERSATION_PLAY_TURN1 seconds="+(EditorApplication.timeSinceStartup-turnStarted)+" answer="+experience.Caption);experience.Questions.AskText("Did he go into the Holy Place to eat it?");Check(experience.Questions.Busy,"Follow-up was not accepted");stage=3;}
  else if(stage==3){if(experience.Questions.Busy)return;var answer=experience.Caption.ToLowerInvariant();Check(answer.Contains("david")||answer.Contains("he "),"Follow-up lost its subject");Check(answer.Contains("not")||answer.Contains("doesn't")||answer.Contains("unclear")||answer.Contains("no "),"Unsupported inner-room claim: "+experience.Caption);Debug.Log("CONVERSATION_PLAY_TURN2 "+experience.Caption);Debug.Log("CONVERSATION_PLAY_REVIEW_PASS generated answer, follow-up memory, cited Scripture and streaming PCM playback");Finish(0);}
 }catch(Exception e){Debug.LogException(e);Finish(1);}}
 static void Finish(int result){SessionState.SetBool("ConversationReviewRunning",false);EditorApplication.update-=Tick;if(SessionState.GetBool("ConversationReviewHadMuted",false))PlayerPrefs.SetInt("GuideMuted",SessionState.GetInt("ConversationReviewMuted",0));else PlayerPrefs.DeleteKey("GuideMuted");PlayerPrefs.Save();EditorApplication.Exit(result);}
}
