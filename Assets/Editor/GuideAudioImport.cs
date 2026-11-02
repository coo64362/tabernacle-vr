using UnityEditor;
using UnityEngine;
public sealed class GuideAudioImport:AssetPostprocessor {
 void OnPreprocessAudio(){if(!assetPath.StartsWith("Assets/Resources/Audio/Guide/"))return;var importer=(AudioImporter)assetImporter;var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.CompressedInMemory;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.65f;settings.preloadAudioData=true;importer.defaultSampleSettings=settings;importer.forceToMono=true;}
}
