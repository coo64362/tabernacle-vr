using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class TabernacleWorld {
 public bool prebuilt;
 public Material veilFabric;
 Material PhysicalMaterial(string name,Color tint,float metal,float glow){
  if(name=="Oil flame")return new Material(Resources.Load<Shader>("OilFlame")){name=name};
  if(glow>.1f){var unlit=new Material(Resources.Load<Shader>("TabernacleSurface")){name=name,color=tint};unlit.SetFloat("_Glow",1);return unlit;}
  var m=new Material(Resources.Load<Shader>("TabernaclePhysical")){name=name,enableInstancing=true,color=tint};
  m.SetFloat("_Metallic",metal>.1f?.72f:0);m.SetFloat("_Glossiness",metal>.1f?.67f:.20f);m.SetFloat("_Relief",metal>.1f?.6f:1.2f);
  m.mainTexture=Resources.Load<Texture2D>("Art/Linen");
  m.SetVector("_AtlasRect",new Vector4(1,1,0,0));
  if(name=="Gold"){m.mainTexture=Resources.Load<Texture2D>("Art/Gold");m.color=new Color(.95f,.93f,.88f);m.SetVector("_AtlasRect",new Vector4(1,1,0,0));m.mainTextureScale=Vector2.one*2;}
  else if(name=="Bronze"){m.mainTexture=Resources.Load<Texture2D>("Art/Bronze");m.color=new Color(.85f,.82f,.78f);m.SetVector("_AtlasRect",new Vector4(1,1,0,0));m.mainTextureScale=Vector2.one*2;m.SetFloat("_Glossiness",.44f);}
  else if(name=="Silver"){m.mainTexture=Texture2D.whiteTexture;m.color=new Color(.72f,.76f,.80f);m.SetVector("_AtlasRect",new Vector4(1,1,0,0));}
  else if(name=="Sand"){m.mainTexture=Resources.Load<Texture2D>("Art/Sand");m.color=new Color(.90f,.86f,.78f);m.SetVector("_AtlasRect",new Vector4(1,1,0,0));m.SetFloat("_WorldScale",.45f);m.SetFloat("_Glossiness",.05f);}
  else if(name=="Bread"){m.color=Color.white;m.mainTexture=Resources.Load<Texture2D>("Art/Bread");m.SetVector("_AtlasRect",new Vector4(1,1,0,0));m.SetFloat("_Glossiness",.08f);m.SetFloat("_Relief",.8f);}
  else {if(name=="Linen")m.color=new Color(.98f,.97f,.93f);m.mainTextureScale=Vector2.one*10;m.SetFloat("_Glossiness",.12f);}
  return m;
 }
 Texture2D BreadSurface(){var tex=new Texture2D(256,256,TextureFormat.RGB24,true){name="Baked bread crust"};var colors=new Color[256*256];for(int y=0;y<256;y++)for(int x=0;x<256;x++){
  float n=Mathf.PerlinNoise(x*.15f,y*.15f),fine=Mathf.PerlinNoise(x*.8f+13,y*.8f);float crack=Mathf.Pow(Mathf.Clamp01(1-Mathf.Abs(n-.5f)*15),8)*.13f;
  colors[y*256+x]=Color.Lerp(new Color(.33f,.16f,.058f),new Color(.76f,.57f,.32f),n*.65f+fine*.35f)-new Color(crack,crack*.6f,crack*.3f);
 }tex.SetPixels(colors);tex.Apply(true);tex.anisoLevel=8;return tex;}
 void SetRealisticEnvironment(){
  veilFabric=new Material(Resources.Load<Shader>("TabernaclePhysical")){name="Embroidered inner veil",mainTexture=Resources.Load<Texture2D>("Art/InnerVeil"),color=Color.white};veilFabric.SetFloat("_Glossiness",.16f);veilFabric.SetFloat("_Relief",1.2f);
  RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.19f,.22f,.26f);RenderSettings.ambientEquatorColor=new Color(.11f,.10f,.08f);RenderSettings.ambientGroundColor=new Color(.07f,.05f,.035f);RenderSettings.ambientIntensity=1;
  var sky=new Material(Shader.Find("Skybox/Procedural")){name="Desert daylight"};sky.SetFloat("_SunSize",.025f);sky.SetFloat("_AtmosphereThickness",.85f);sky.SetColor("_SkyTint",new Color(.53f,.59f,.64f));sky.SetColor("_GroundColor",new Color(.34f,.28f,.20f));sky.SetFloat("_Exposure",.9f);RenderSettings.skybox=sky;
  RenderSettings.fog=true;RenderSettings.fogColor=new Color(.68f,.63f,.54f);RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=65;RenderSettings.fogEndDistance=135;
  var sun=new GameObject("Desert sun — baked").AddComponent<Light>();sun.transform.SetParent(transform);sun.type=LightType.Directional;sun.transform.rotation=Quaternion.Euler(37,-32,0);sun.color=new Color(1,.95f,.85f);sun.intensity=1.4f;sun.shadows=LightShadows.Soft;sun.shadowBias=.025f;BakedOnly(sun);RenderSettings.sun=sun;
 }
 void BakedOnly(Light light){
#if UNITY_EDITOR
  light.lightmapBakeType=LightmapBakeType.Baked;
#endif
 }
 void AddOilLight(Transform parent,Vector3 position){var g=new GameObject("Warm oil light — baked");g.transform.SetParent(parent,false);g.transform.localPosition=position;var l=g.AddComponent<Light>();l.type=LightType.Point;l.range=6;l.intensity=.55f;l.color=new Color(1,.60f,.28f);l.shadows=LightShadows.Soft;BakedOnly(l);}
 GameObject RoundedBox(string name,Transform t,Vector3 center,Vector3 size,Material m,bool collision){
  var g=new GameObject(name);g.transform.SetParent(t,false);g.transform.localPosition=center;
  float bevel=Mathf.Min(.018f,Mathf.Min(size.x,Mathf.Min(size.y,size.z))*.19f);Vector3 half=size*.5f;
  var vertices=new List<Vector3>();var normals=new List<Vector3>();var uvs=new List<Vector2>();var indices=new List<int>();
  Vector3[] faceNormals={Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back};
  foreach(var n in faceNormals){Vector3 u=Mathf.Abs(n.y)>.5f?Vector3.right:Vector3.Cross(Vector3.up,n);Vector3 v=Vector3.Cross(n,u);int start=vertices.Count;
   for(int y=0;y<4;y++)for(int x=0;x<4;x++){
    float hu=Vector3.Dot(half,new Vector3(Mathf.Abs(u.x),Mathf.Abs(u.y),Mathf.Abs(u.z))),hv=Vector3.Dot(half,new Vector3(Mathf.Abs(v.x),Mathf.Abs(v.y),Mathf.Abs(v.z)));
    float xx=x==0?-hu:x==1?-hu+bevel:x==2?hu-bevel:hu;float yy=y==0?-hv:y==1?-hv+bevel:y==2?hv-bevel:hv;
    var p=Vector3.Scale(n,half)+u*xx+v*yy;var core=new Vector3(Mathf.Clamp(p.x,-half.x+bevel,half.x-bevel),Mathf.Clamp(p.y,-half.y+bevel,half.y-bevel),Mathf.Clamp(p.z,-half.z+bevel,half.z-bevel));var normal=(p-core).normalized;
    vertices.Add(core+normal*bevel);normals.Add(normal);uvs.Add(new Vector2(xx/(2*hu)+.5f,yy/(2*hv)+.5f));
   }
   for(int y=0;y<3;y++)for(int x=0;x<3;x++){int a=start+y*4+x;indices.AddRange(new[]{a,a+1,a+4,a+1,a+5,a+4});}
  }
  var mesh=new Mesh{name="Softly worked edges — "+name};mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetUVs(0,uvs);mesh.SetTriangles(indices,0);mesh.RecalculateBounds();g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=m;
  if(collision){g.AddComponent<BoxCollider>().size=size;g.layer=8;}return g;
 }
 void Fabric(string name,Transform parent,Vector3 center,float width,float height,Material material,bool collision=true,float yaw=0){
  var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=center;g.transform.localRotation=Quaternion.Euler(0,yaw,0);
  int nx=Mathf.Clamp(Mathf.CeilToInt(width*24),24,512),ny=12;var verts=new Vector3[(nx+1)*(ny+1)];var uv=new Vector2[verts.Length];var tris=new List<int>();
  for(int y=0;y<=ny;y++)for(int x=0;x<=nx;x++){float u=(float)x/nx,v=(float)y/ny;float fold=Mathf.Sin(u*width*2*Mathf.PI/.32f)*.027f+Mathf.Sin(u*width*12.7f)*.010f;int k=y*(nx+1)+x;verts[k]=new Vector3((u-.5f)*width,(v-.5f)*height+Mathf.Sin(u*width*6)*.012f*(1-v),fold);uv[k]=material==veilFabric?new Vector2(u,v):new Vector2(u*width/1.8f,v*height/1.8f);if(x<nx&&y<ny){int b=k+nx+1;tris.AddRange(new[]{k,b,k+1,k+1,b,b+1});}}
  var mesh=new Mesh{name=name+" cloth folds",indexFormat=IndexFormat.UInt32};mesh.vertices=verts;mesh.uv=uv;mesh.triangles=tris.ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();g.AddComponent<MeshFilter>().sharedMesh=mesh;var r=g.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.TwoSided;
  if(collision){g.AddComponent<BoxCollider>().size=new Vector3(width,height,.12f);g.layer=8;}
 }
 void DrapedScreen(Transform t,Vector3 p,float w,float h,bool cherub){
  Fabric(cherub?"Inner veil":"Entrance screen",t,p,w,h,cherub?veilFabric:purple);
  if(!cherub)foreach(int s in new[]{-1,1}){Box("Scarlet woven border",t,p+new Vector3(s*(w/2-.045f),0,-.04f),new Vector3(.045f,h,.009f),scarlet);Box("Linen woven border",t,p+new Vector3(s*(w/2-.075f),0,-.041f),new Vector3(.012f,h,.008f),linen);}
 }
 void DressTent(){float w=10*Cubit,h=10*Cubit,front=26,length=30*Cubit;
  foreach(int s in new[]{-1,1})Fabric("Outer layered skin covering",architecture,new Vector3(s*(w/2+.14f),h/2,front+length/2),length+.2f,h+.08f,dark,false,90);
  for(int i=0;i<3;i++){var before=architecture.childCount;Fabric("Inner linen cherubim covering",architecture,new Vector3(0,h-.045f,front+(i+.5f)*10*Cubit),w,w,veilFabric,false);architecture.GetChild(before).localRotation=Quaternion.Euler(90,0,0);}
 }
 void BuildDesert(){
  var floor=Box("Desert collision ground",architecture,new Vector3(0,-.15f,20),new Vector3(240,.3f,240),sand,true);floor.layer=9;floor.GetComponent<Renderer>().enabled=false;
  var g=new GameObject("Natural desert terrain");g.transform.SetParent(transform,false);int n=128;var verts=new Vector3[(n+1)*(n+1)];var uvs=new Vector2[verts.Length];var tris=new List<int>();
  for(int z=0;z<=n;z++)for(int x=0;x<=n;x++){float xx=-120+x*240f/n,zz=-100+z*240f/n;float distance=Mathf.Max(Mathf.Abs(xx)-14,Mathf.Max(-zz-8,zz-50));float blend=Mathf.SmoothStep(0,1,Mathf.Clamp01(distance/30));float height=(Mathf.PerlinNoise(xx*.032f+30,zz*.032f+23)*11+Mathf.PerlinNoise(xx*.08f+60,zz*.08f)*2)*blend-.018f;int k=z*(n+1)+x;verts[k]=new Vector3(xx,height,zz);uvs[k]=new Vector2(xx,zz);if(x<n&&z<n){int b=k+n+1;tris.AddRange(new[]{k,b,k+1,k+1,b,b+1});}}
  var mesh=new Mesh{name="Wind-shaped desert",indexFormat=IndexFormat.UInt32};mesh.vertices=verts;mesh.uv=uvs;mesh.triangles=tris.ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();g.AddComponent<MeshFilter>().sharedMesh=mesh;var outerSand=new Material(Resources.Load<Shader>("TabernacleSurface")){name="Distant sand",color=new Color(.95f,.91f,.84f),mainTexture=Resources.Load<Texture2D>("Art/Sand")};g.AddComponent<MeshRenderer>().sharedMaterial=outerSand;
  Box("Courtyard packed sand",architecture,new Vector3(0,-.035f,Length/2),new Vector3(Width,.07f,Length),sand);
 }
 void AlmondFlower(Transform t,Vector3 p){
  Bowl("Almond blossom cup",t,p,.032f,.035f,gold,16);Ball("Almond bud",t,p+Vector3.down*.025f,new Vector3(.035f,.055f,.035f),gold);
  for(int j=0;j<5;j++){float a=j*2*Mathf.PI/5;var petal=Ball("Worked almond petal",t,p+new Vector3(Mathf.Cos(a)*.027f,.035f,Mathf.Sin(a)*.027f),new Vector3(.019f,.065f,.012f),gold);petal.transform.localRotation=Quaternion.Euler(Mathf.Sin(a)*-32,a*Mathf.Rad2Deg,Mathf.Cos(a)*32);}
 }
}
