using System;
using System.Collections.Generic;
using System.IO;
using Concentus.Structs;
using UnityEditor;
using UnityEngine;
public static class ConversationAudioImport {
 [Serializable] class Packet {public string frame;public int number,total;}
 [Serializable] class Recording {public string name,text;public Packet[] packets;}
 public static void Run(){
  foreach(var file in Directory.GetFiles("/private/tmp/tabernacle-conversation-audio","*.json")){
   var recording=JsonUtility.FromJson<Recording>(File.ReadAllText(file));var decoder=new OpusDecoder(24000,1);var samples=new List<short>();var block=new short[480];int expected=0;
   foreach(var p in recording.packets){if(p.number==0){decoder.ResetState();expected=0;}if(p.number!=expected++)throw new Exception("Incomplete recording: "+recording.name);var data=Convert.FromBase64String(p.frame);int n=decoder.Decode(data,0,data.Length,block,0,480,false);for(int i=0;i<n;i++)samples.Add(block[i]);}
   string path="Assets/Resources/Audio/"+recording.name+".wav";Directory.CreateDirectory(Path.GetDirectoryName(path));using(var w=new BinaryWriter(File.Create(path))){int bytes=samples.Count*2;w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(bytes+36);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(24000);w.Write(48000);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(bytes);foreach(var sample in samples)w.Write(sample);}AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);var importer=(AudioImporter)AssetImporter.GetAtPath(path);importer.forceToMono=true;var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.75f;importer.defaultSampleSettings=settings;importer.SaveAndReimport();Debug.Log("CONVERSATION_AUDIO "+recording.name+" seconds="+samples.Count/24000f);
  }Debug.Log("CONVERSATION_AUDIO_IMPORT_PASS");
 }
}
