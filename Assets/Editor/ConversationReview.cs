using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class ConversationReview {
 static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
 public static void ValidateAndBuild(){Run();QuestBuild.Build();}
 public static void RunFinal(){GuideIntegrationReview.ImportAndRun();ExperiencePlayReview.Run();}
 public static void ImportAndRun(){ConversationAudioImport.Run();Run();}
 public static void Run(){
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var corpus=JsonUtility.FromJson<ScriptureSearch.Corpus>(Resources.Load<TextAsset>("Guide/Scripture").text);Check(corpus.verses.Length>31000,"Incomplete Scripture corpus");var search=new ScriptureSearch(corpus);
  var bread=search.Retrieve("Has anyone who was not a priest eaten the holy bread?");Debug.Log("BREAD_RETRIEVAL "+string.Join("; ",bread.Select(p=>p.Reference)));Check(bread.Any(p=>p.book=="1 Samuel"&&p.chapter==21&&p.first<=6),"David story missing from retrieved evidence");Check(bread.Any(p=>p.book=="Leviticus"&&p.chapter==24&&p.first<=5&&p.last>=9),"Normal bread rule missing");
  var ashes=search.Retrieve("What did they do with the ashes from the altar?");Check(ashes.Any(p=>p.book=="Leviticus"&&p.chapter==6),"Ash process missing");
  Check(search.ValidReferences("1 Samuel 21:1–6; Leviticus 24:9").Length==2,"Valid citations rejected");Check(search.ValidReferences("Exodus 9999999999:9999999999; Exodus 99:999; Leviticus 24:100").Length==0,"Impossible citation accepted");
  Check(GuideSpeechGesture.FromWords("Yes, David received bread.")=="nod","Affirming gesture missing");Check(GuideSpeechGesture.FromWords("It was made of acacia wood.")=="explain","Explanatory gesture missing");
  Check(!File.ReadAllText("Assets/TabernacleQuestions.cs").Contains("command_mode"),"Stored answer selector remains");Check(!File.ReadAllText("Assets/ControllerTablet.cs").Contains("Thinking about"),"Processing text on tablet");
  Check(TabernacleExperience.Narration[1].Contains("Leviticus 1:3"),"Sacrifice process narration missing");Check(TabernacleExperience.Narration[3].Contains("acacia"),"Holy Place material narration missing");
  foreach(string name in new[]{"hmm","umm","well","let-me-think","give-me-a-second","connection"})Check(Resources.Load<AudioClip>("Audio/Reactions/"+name),"Missing reaction: "+name);
  Directory.CreateDirectory("Documentation/Review-v0.6");File.WriteAllText("Documentation/Review-v0.6/verification.txt","Scripture verses: "+corpus.verses.Length+"\nDavid and altar-ash evidence retrieval: pass\nReference validity checks: pass\nSentence gesture selection: pass\nGenerated answer pipeline (no answer selector): pass\nRevised tour process/material narration: pass\nVaried recorded thinking reactions: pass\nHeadset microphone, visual gesture quality, following comfort and sustained performance: not tested\n");Debug.Log("CONVERSATION_REVIEW_PASS");
 }
}
