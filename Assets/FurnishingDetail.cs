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
  // Drop-in slot for a properly sculpted cherub: put a prefab at Assets/Resources/Models/Cherub.prefab, authored facing +X with its feet at the origin,
  // about 0.45 m tall, gold material, under roughly 6,000 triangles. One copy is placed per end, turned to face the other (Ex 25:18-20).
  var sculpted=Resources.Load<GameObject>("Models/Cherub");
  if(sculpted!=null){var copy=Instantiate(sculpted,t);copy.name="Cherub — sculpted model";copy.transform.localPosition=origin;copy.transform.localRotation=Quaternion.Euler(0,side>0?180f:0f,0);return;}
  // Restrained, kneeling winged figures. No Assyrian crowns, curled beards or animal bodies.
  // Anatomy is deliberately schematic: Exodus specifies posture and wings, not facial appearance.
  Ball("Cherub kneeling robe — interpretive",t,origin+new Vector3(side*.025f,.075f,0),new Vector3(.19f,.15f,.15f),gold);
  Ball("Cherub inclined torso",t,origin+new Vector3(-side*.025f,.19f,0),new Vector3(.10f,.18f,.10f),gold);
  var head=Ball("Cherub lowered featureless face",t,origin+new Vector3(-side*.075f,.30f,0),new Vector3(.065f,.09f,.063f),gold);
  head.transform.localRotation=Quaternion.Euler(0,0,-side*28);
  foreach(int z in new[]{-1,1}){
   Rod("Cherub upper arm",t,origin+new Vector3(-side*.02f,.24f,z*.06f),origin+new Vector3(-side*.09f,.18f,z*.07f),.019f,gold);
   Rod("Cherub lowered hand",t,origin+new Vector3(-side*.09f,.18f,z*.07f),origin+new Vector3(-side*.13f,.12f,z*.06f),.015f,gold);
   for(int feather=0;feather<13;feather++){
    float spread=(float)feather/12;
    Vector3 from=origin+new Vector3(side*.025f,.20f,z*.045f);
    Vector3 tip=origin+new Vector3(-side*(.19f+.22f*spread),.36f+.19f*Mathf.Sin(spread*Mathf.PI*.72f),z*(.065f+.11f*(1-spread)));
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
