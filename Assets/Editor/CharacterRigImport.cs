using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CharacterRigImport {
 public static void Run(){AssetDatabase.Refresh();foreach(string name in new[]{"SandstoneGuide","FemaleChild"})Build(name);AssetDatabase.SaveAssets();Debug.Log("CHARACTER_RIG_IMPORT_PASS skinned FBX prefabs created");}
 static Material Material(string path,Color color,Texture texture=null,float glow=0,float metal=0){var m=new Material(Resources.Load<Shader>("CharacterSurface")){color=color,mainTexture=texture};m.SetFloat("_Glow",glow);m.SetFloat("_Metal",metal);var old=AssetDatabase.LoadAssetAtPath<Material>(path);if(old){EditorUtility.CopySerialized(m,old);UnityEngine.Object.DestroyImmediate(m);return old;}AssetDatabase.CreateAsset(m,path);return m;}
 static void Build(string name){
  string folder="Assets/Resources/Characters/",path=folder+name+".fbx";var importer=AssetImporter.GetAtPath(path) as ModelImporter;if(!importer)throw new Exception("Missing character FBX "+name);importer.animationType=ModelImporterAnimationType.Generic;importer.importAnimation=false;importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.isReadable=true;importer.SaveAndReimport();
  var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);var model=UnityEngine.Object.Instantiate(source);var root=new GameObject(name);model.transform.SetParent(root.transform,true);if(name=="SandstoneGuide"){var visor=RiggedGuideVisual.Find(root.transform,"VisorScreen");var head=RiggedGuideVisual.Find(root.transform,"Head");if(visor&&head){var renderer=visor.GetComponent<Renderer>();float front=(renderer?renderer.bounds.center:visor.position).z-head.position.z;Debug.Log("RIG_FRONT "+front);if(front<0)model.transform.Rotate(0,180,0,Space.World);}}
  var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(folder+name+"_BaseColor.png");var textureImporter=AssetImporter.GetAtPath(folder+name+"_BaseColor.png") as TextureImporter;if(textureImporter){textureImporter.maxTextureSize=2048;textureImporter.mipmapEnabled=true;textureImporter.SaveAndReimport();}
  string art=folder+name+"Materials/";Directory.CreateDirectory(art);AssetDatabase.Refresh();
  var body=Material(art+"Body.mat",Color.white,texture,.18f);var glass=Material(art+"Visor.mat",new Color(.038f,.020f,.009f),null,.1f,.4f);var amber=Material(art+"Amber.mat",new Color(1,.60f,.16f),null,1);var bronze=Material(art+"Bronze.mat",new Color(.66f,.35f,.1f),null,0,.7f);var stone=Material(art+"Stone.mat",new Color(.74f,.59f,.38f));
  foreach(var r in root.GetComponentsInChildren<Renderer>()){
   var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++){string n=r.name;mats[i]=n.Contains("Eye_")||n.Contains("Brow_")||n=="Smile"?amber:n=="VisorScreen"?glass:n=="BronzeBezel"?bronze:n=="SandstoneBezel"?stone:body;}r.sharedMaterials=mats;
   if(r is SkinnedMeshRenderer skin){skin.updateWhenOffscreen=true;skin.quality=SkinQuality.Bone4;if(!skin.sharedMesh||skin.bones.Length<1)throw new Exception("Incomplete skin "+name);}
  }
  var bounds=new Bounds();bool first=true;foreach(var r in root.GetComponentsInChildren<Renderer>()){if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds);}if(bounds.size.y<.8||bounds.size.y>2.5)throw new Exception("Unexpected character height "+name+" "+bounds.size.y);
  PrefabUtility.SaveAsPrefabAsset(root,folder+name+"Ready.prefab");
  if(name=="FemaleChild"){
   int meshIndex=0;foreach(var skin in root.GetComponentsInChildren<SkinnedMeshRenderer>()){
    var mesh=UnityEngine.Object.Instantiate(skin.sharedMesh);var weights=mesh.boneWeights;int[] hidden=skin.bones.Select((b,i)=>new{b,i}).Where(x=>x.b.name=="Head"||x.b.name=="Neck").Select(x=>x.i).ToArray();bool Hide(int index){var w=weights[index];return hidden.Contains(w.boneIndex0)&&w.weight0>.1f||hidden.Contains(w.boneIndex1)&&w.weight1>.1f||hidden.Contains(w.boneIndex2)&&w.weight2>.1f||hidden.Contains(w.boneIndex3)&&w.weight3>.1f;}
    for(int s=0;s<mesh.subMeshCount;s++){var t=mesh.GetTriangles(s);var keep=new System.Collections.Generic.List<int>();for(int i=0;i<t.Length;i+=3)if(!Hide(t[i])&&!Hide(t[i+1])&&!Hide(t[i+2])){keep.Add(t[i]);keep.Add(t[i+1]);keep.Add(t[i+2]);}mesh.SetTriangles(keep,s);}if(mesh.triangles.Length==0){UnityEngine.Object.DestroyImmediate(mesh);UnityEngine.Object.DestroyImmediate(skin);continue;}string meshPath=folder+"FemaleChildFirstPersonMesh"+(meshIndex++)+".asset";if(AssetDatabase.LoadAssetAtPath<Mesh>(meshPath))AssetDatabase.DeleteAsset(meshPath);AssetDatabase.CreateAsset(mesh,meshPath);skin.sharedMesh=mesh;
   }
   PrefabUtility.SaveAsPrefabAsset(root,folder+"FemaleChildFirstPerson.prefab");
  }
  Debug.Log("RIG_IMPORT "+name+" height="+bounds.size.y+" skinned="+root.GetComponentsInChildren<SkinnedMeshRenderer>().Length);UnityEngine.Object.DestroyImmediate(root);
 }
}
