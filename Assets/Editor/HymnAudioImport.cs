using UnityEngine;
using UnityEditor;
public sealed class HymnAudioImport : AssetPostprocessor {
 void OnPreprocessAudio(){
  if(!assetPath.EndsWith("/Audio/ItIsWellInstrumental.wav") && !assetPath.EndsWith("/Audio/GraceGreaterInstrumental.wav"))return;
  var importer=(AudioImporter)assetImporter;
  importer.forceToMono=false;importer.loadInBackground=true;
  var settings=importer.defaultSampleSettings;
  settings.preloadAudioData=false;
  settings.loadType=AudioClipLoadType.Streaming;
  settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.65f;
  settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;
  importer.defaultSampleSettings=settings;
 }
}
