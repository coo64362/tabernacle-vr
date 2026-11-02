using System;
using UnityEditor;
using UnityEngine;
public static class HymnAudioReview {
 public static void Run(){
  foreach(var name in new[]{"ItIsWellInstrumental","GraceGreaterInstrumental"}){
   var clip=Resources.Load<AudioClip>("Audio/"+name);
   if(!clip||clip.length<70||clip.channels!=2)throw new Exception("Hymn clip import failed: "+name);
   var importer=(AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(clip));
   if(importer.defaultSampleSettings.loadType!=AudioClipLoadType.Streaming)throw new Exception("Expected streaming audio");
   Debug.Log("HYMN_CHECK_PASS "+name+" duration="+clip.length+" channels="+clip.channels+" streaming=true");
  }
 }
}
