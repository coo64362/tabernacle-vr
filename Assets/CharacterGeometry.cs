using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Original, small meshes shared by the guide and pilgrim avatars. No downloaded character assets.
public static class CharacterGeometry {
 static readonly Dictionary<string,Mesh> meshes=new Dictionary<string,Mesh>();
 public static Material Material(string name,Color color,bool cloth=false){
  var m=new Material(Resources.Load<Shader>("CharacterSurface")){name=name,color=color};
  if(cloth)m.mainTexture=Resources.Load<Texture2D>("Art/Linen");return m;
 }
 public static Transform Group(string name,Transform parent,Vector3 position){var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;return t;}
 public static Transform Oval(string name,Transform parent,Vector3 position,Vector3 size,Material material,int layer=0){
  var t=Group(name,parent,position);t.localScale=size;t.gameObject.layer=layer;
  if(!meshes.TryGetValue("oval",out var mesh)){
   var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();const int rings=12,segments=20;
   for(int y=0;y<=rings;y++)for(int x=0;x<=segments;x++){float a=y*Mathf.PI/rings,b=x*2*Mathf.PI/segments;vertices.Add(new Vector3(Mathf.Sin(a)*Mathf.Cos(b),Mathf.Cos(a),Mathf.Sin(a)*Mathf.Sin(b))*.5f);uv.Add(new Vector2((float)x/segments,(float)y/rings));if(y<rings&&x<segments){int i=y*(segments+1)+x;triangles.AddRange(new[]{i,i+1,i+segments+1,i+1,i+segments+2,i+segments+1});}}
   mesh=new Mesh{name="Smooth character oval"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();meshes["oval"]=mesh;
  }
  t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;var r=t.gameObject.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;return t;
 }
 public static Transform Garment(string name,Transform parent,float bottom,float top,float hem,float shoulder,Material material,int layer=0){
  var t=Group(name,parent,Vector3.zero);t.gameObject.layer=layer;const int rings=10,segments=32;
  var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
  for(int y=0;y<=rings;y++)for(int x=0;x<=segments;x++){
   float f=(float)y/rings,a=x*2*Mathf.PI/segments,r=Mathf.Lerp(hem,shoulder,f)*(1+.035f*Mathf.Cos(8*a)*(1-f));
   vertices.Add(new Vector3(Mathf.Cos(a)*r,Mathf.Lerp(bottom,top,f),Mathf.Sin(a)*r*.65f));uv.Add(new Vector2((float)x/segments*2,f*3));
   if(y<rings&&x<segments){int i=y*(segments+1)+x;triangles.AddRange(new[]{i,i+segments+1,i+1,i+1,i+segments+1,i+segments+2});}
  }
  var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;t.gameObject.AddComponent<CharacterMeshOwner>().Mesh=mesh;t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;return t;
 }
 public static Transform Drape(Transform parent,Material material,int layer=0){
  var t=Group("Draped russet shoulder sash",parent,Vector3.zero);t.gameObject.layer=layer;var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
  for(int side=0;side<2;side++)for(int y=0;y<=12;y++)for(int x=0;x<=4;x++){
   float f=y/12f,w=x/4f;float center=Mathf.Lerp(-.145f,.14f,f);float z=(side==0?1:-1)*(.15f+Mathf.Sin(f*Mathf.PI)*.015f+Mathf.Cos(w*Mathf.PI*4)*.005f);
   vertices.Add(new Vector3(center+(w-.5f)*.18f,Mathf.Lerp(1.30f,.70f,f),z));uv.Add(new Vector2(w,f*2));
   if(y<12&&x<4){int i=side*65+y*5+x;if(side==0)triangles.AddRange(new[]{i,i+5,i+1,i+1,i+5,i+6});else triangles.AddRange(new[]{i,i+1,i+5,i+1,i+6,i+5});}
  }
  var mesh=new Mesh{name=t.name};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;t.gameObject.AddComponent<CharacterMeshOwner>().Mesh=mesh;t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;return t;
 }
 public static void Segment(Transform part,Vector3 a,Vector3 b,float width){part.position=(a+b)*.5f;part.rotation=Quaternion.FromToRotation(Vector3.up,b-a);var parentScale=part.parent?part.parent.lossyScale:Vector3.one;part.localScale=new Vector3(width/parentScale.x,(Vector3.Distance(a,b)+width*.3f)/parentScale.y,width/parentScale.z);}
 public static Transform Curve(string name,Transform parent,Vector3[] points,float width,Material material,int layer=0){
  var t=Group(name,parent,Vector3.zero);t.gameObject.layer=layer;var line=t.gameObject.AddComponent<LineRenderer>();line.useWorldSpace=false;line.sharedMaterial=material;line.widthMultiplier=width;line.positionCount=points.Length;line.SetPositions(points);line.numCapVertices=3;line.numCornerVertices=2;line.shadowCastingMode=ShadowCastingMode.Off;return t;
 }
 public static void Combine(Transform root){
  var batches=new Dictionary<Material,List<CombineInstance>>();var remove=new List<GameObject>();
  foreach(var filter in root.GetComponentsInChildren<MeshFilter>()){
   var renderer=filter.GetComponent<MeshRenderer>();if(!renderer||!renderer.enabled)continue;
   if(!batches.ContainsKey(renderer.sharedMaterial))batches[renderer.sharedMaterial]=new List<CombineInstance>();
   batches[renderer.sharedMaterial].Add(new CombineInstance{mesh=filter.sharedMesh,transform=root.worldToLocalMatrix*filter.transform.localToWorldMatrix});remove.Add(filter.gameObject);
  }
  foreach(var pair in batches){var t=Group(pair.Key.name+" character batch",root,Vector3.zero);t.gameObject.layer=root.gameObject.layer;var mesh=new Mesh{name=t.name,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(pair.Value.ToArray());t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;t.gameObject.AddComponent<CharacterMeshOwner>().Mesh=mesh;t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=pair.Key;}
  foreach(var g in remove){g.SetActive(false);if(Application.isPlaying)Object.Destroy(g);else Object.DestroyImmediate(g);}
 }
}
