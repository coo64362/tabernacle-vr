using System.Collections.Generic;
using UnityEngine;

public partial class TabernacleWorld {
 void DetailedArk(){
  var t=Group("Furnishing — ark",new Vector3(0,0,38));float w=2.5f*Cubit,d=1.5f*Cubit,h=1.5f*Cubit;
  Box("Gold-overlaid acacia ark",t,new Vector3(0,h/2,0),new Vector3(w,h,d),gold,true);
  Box("Mercy seat",t,new Vector3(0,h+.025f,0),new Vector3(w+.025f,.05f,d+.025f),gold);
  foreach(int side in new[]{-1,1}){
   Box("Ark upper molding",t,new Vector3(0,h-.025f,side*d/2),new Vector3(w+.035f,.035f,.035f),gold);
   Box("Ark lower molding",t,new Vector3(0,.035f,side*d/2),new Vector3(w+.025f,.025f,.025f),gold);
   Box("Ark end molding",t,new Vector3(side*w/2,h-.025f,0),new Vector3(.035f,.035f,d+.035f),gold);
   Rod("Ark poles remain in rings",t,new Vector3(-1.1f,.44f,side*(d/2+.08f)),new Vector3(1.1f,.44f,side*(d/2+.08f)),.025f,gold);
   foreach(int end in new[]{-1,1}){
    int index=t.childCount;Torus("Ark gold carrying ring",t,new Vector3(end*w*.35f,.44f,side*(d/2+.08f)),.048f,.010f,gold,32);t.GetChild(index).localRotation=Quaternion.Euler(0,0,90);
    Box("Ring mounting boss",t,new Vector3(end*w*.35f,.44f,side*(d/2+.012f)),new Vector3(.075f,.075f,.035f),gold);
   }
   Guardian(t,new Vector3(side*w*.34f,h+.055f,0),side);
  }
 }
 // A clearly interpretive winged guardian. Counts, facing and sheltering wings follow Exodus 25.
 void Guardian(Transform t,Vector3 origin,int side){
  Ball("Cherub body — interpretive sculpture",t,origin+new Vector3(0,.12f,0),new Vector3(.27f,.15f,.14f),gold);
  Ball("Sculpted cherub haunch",t,origin+new Vector3(side*.10f,.105f,0),new Vector3(.12f,.16f,.14f),gold);
  Ball("Cherub face toward cover",t,origin+new Vector3(-side*.105f,.235f,0),new Vector3(.080f,.105f,.075f),gold);
  Ball("Face profile",t,origin+new Vector3(-side*.143f,.224f,0),new Vector3(.034f,.035f,.047f),gold);
  foreach(int z in new[]{-1,1}){
   Rod("Guardian foreleg",t,origin+new Vector3(-side*.075f,.14f,z*.045f),origin+new Vector3(-side*.095f,.025f,z*.045f),.015f,gold);
   Ball("Guardian paw",t,origin+new Vector3(-side*.115f,.02f,z*.045f),new Vector3(.075f,.032f,.035f),gold);
   for(int feather=0;feather<13;feather++){
    float spread=(float)feather/12;
    Vector3 from=origin+new Vector3(side*.025f,.15f,z*.045f);
    Vector3 tip=origin+new Vector3(-side*(.17f+.22f*spread),.28f+.19f*Mathf.Sin(spread*Mathf.PI*.72f),z*(.065f+.11f*(1-spread)));
    SculptedFeather(t,from,tip,.016f+.006f*(1-spread));
   }
  }
 }
 void SculptedFeather(Transform t,Vector3 a,Vector3 b,float width){
  var verts=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();Vector3 direction=(b-a).normalized;
  Vector3 across=Vector3.Cross(Vector3.forward,direction).normalized;if(across.sqrMagnitude<.1f)across=Vector3.forward;Vector3 normal=Vector3.Cross(direction,across).normalized;
  int rings=10,sides=6;
  for(int i=0;i<=rings;i++){
   float f=(float)i/rings;Vector3 center=Vector3.Lerp(a,b,f)+Vector3.up*(Mathf.Sin(f*Mathf.PI)*.035f);float w=Mathf.Max(.0008f,Mathf.Pow(Mathf.Max(0,Mathf.Sin(f*Mathf.PI)),.65f)*width);
   for(int j=0;j<sides;j++){float angle=j*2*Mathf.PI/sides;verts.Add(center+across*(Mathf.Cos(angle)*w)+normal*(Mathf.Sin(angle)*w*.20f));uv.Add(new Vector2((float)j/sides,f));}
  }
  for(int i=0;i<rings;i++)for(int j=0;j<sides;j++){int x=i*sides+j,y=i*sides+(j+1)%sides,z=x+sides,w=y+sides;triangles.AddRange(new[]{x,z,y,y,z,w});}
  var mesh=new Mesh{name="Individually worked gold feather"};mesh.SetVertices(verts);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
  var g=new GameObject("Chased wing feather");g.transform.SetParent(t,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=gold;
 }
}
