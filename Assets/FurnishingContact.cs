using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public partial class TabernacleWorld {
 void VerifyFurnishingContacts(){
  var nodes=GetComponentsInChildren<Transform>();
  var loaves=nodes.Where(t=>t.name.StartsWith("Bread loaf ")).OrderBy(t=>int.Parse(t.name.Split(' ')[2])).ToArray();
  var plates=nodes.Where(t=>t.name=="Gold plate beneath bread").ToArray();
  if(loaves.Length!=12||plates.Length!=2)throw new Exception("CONTACT_CHECK bread support inventory");
  for(int stack=0;stack<2;stack++){
   var lower=plates.OrderBy(t=>Mathf.Abs(t.position.z-loaves[stack*6].position.z)).First().GetComponent<Renderer>().bounds;
   for(int j=0;j<6;j++){
    var current=loaves[stack*6+j].GetComponent<Renderer>().bounds;
    if(Mathf.Abs(current.min.y-lower.max.y)>.001f)throw new Exception("CONTACT_CHECK floating bread "+loaves[stack*6+j].name);
    lower=current;
   }
  }
  var lamps=nodes.Where(t=>t.name.StartsWith("Oil lamp ")&&t.name.EndsWith("of seven")).ToArray();
  var seats=nodes.Where(t=>t.name=="Gold lamp seat").ToArray();
  if(lamps.Length!=7||seats.Length!=7)throw new Exception("CONTACT_CHECK lamp seat inventory");
  foreach(var lamp in lamps){
   var seat=seats.OrderBy(t=>Vector3.Distance(t.position,lamp.position)).First().GetComponent<Renderer>().bounds;
   if(seat.max.y<lamp.GetComponent<Renderer>().bounds.min.y-.001f)throw new Exception("CONTACT_CHECK detached lamp");
  }
  Debug.Log("CONTACT_CHECK_PASS twelve supported loaves and seven seated oil lamps");
 }
 void ContactLoaf(string name,Transform parent,Vector3 basePoint){
  var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=basePoint;
  const int count=32;
  float[] radius={0,.74f,.96f,1,.94f,.68f,0};
  float[] height={0,0,.008f,.025f,.043f,.055f,.055f};
  var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
  for(int k=0;k<radius.Length;k++)for(int j=0;j<=count;j++){
   float a=j*2*Mathf.PI/count;float x=Mathf.Cos(a)*radius[k],z=Mathf.Sin(a)*radius[k];
   vertices.Add(new Vector3(x*.165f,height[k],z*.155f));uv.Add(new Vector2(.5f+x*.48f,.5f+z*.48f));
   if(k<radius.Length-1&&j<count){int q=k*(count+1)+j,b=q+count+1;triangles.AddRange(new[]{q,b,q+1,q+1,b,b+1});}
  }
  var mesh=new Mesh{name="Flat contact bread loaf"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
  g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=bread;
 }
 void CurvedHorn(Transform parent,Vector3 foot,int x,int z){
  Vector3 previous=foot;
  for(int i=1;i<=6;i++){
   float f=i/6f;var next=foot+new Vector3(x*.035f*f*f,.13f*f,z*.035f*f*f);
   Rod("Incense altar horn",parent,previous,next,Mathf.Lerp(.025f,.004f,f),gold);previous=next;
  }
 }
}
