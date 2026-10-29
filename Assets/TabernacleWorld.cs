using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Coordinates: +Z west, +X north. All prescribed dimensions use a 0.4572 m cubit.
// Profiles of lamps, vessels, bread and cherubim are reconstruction choices.
public partial class TabernacleWorld : MonoBehaviour {
 public const float Cubit=.4572f, Width=50*Cubit, Length=100*Cubit;
 public static TabernacleWorld Instance;
 public Material gold, bronze, sand, linen, blue, purple, scarlet, silver, dark, bread, flame, ink, water;
 public List<Transform> closeDetails=new List<Transform>();
 public readonly Vector3[] stops={new Vector3(0,0,-3),new Vector3(3.6f,0,7.5f),new Vector3(2.5f,0,16),new Vector3(0,0,27.5f),new Vector3(0,0,33.1f),new Vector3(0,0,36.2f)};
 public readonly string[] titles={"THE TABERNACLE", "THE BRONZE ALTAR", "THE WASHING BASIN", "THE HOLY PLACE", "THE INNER VEIL", "THE HOLY OF HOLIES"};
 public readonly string[] notes={"An educational reconstruction\nExodus 25–30; 40\nOne cubit represented as 0.4572 metres", "Five cubits square; three cubits high.\nBronze ash pots, shovels, basins, forks and firepans.\nExodus 27:1–8", "Bronze basin and bronze stand, between altar and tent.\nIts size and profile are not specified.\nExodus 30:18–21; 38:8; 40:30", "Seven oil lamps to the south; twelve loaves to the north.\nGold incense altar before the veil.\nExodus 25:23–40; 26:35; 30:1–6", "Cherubim woven into blue, purple, scarlet and linen.\nFour gold-covered posts with silver bases.\nExodus 26:31–33", "Educational access beyond the veil.\nHistorically, access was restricted: Leviticus 16.\nArk dimensions: Exodus 25:10–22.\nCherubim appearance is interpretive."};
 Transform architecture;
 Font font;
 int breadCount, lampCount, toolCount;
 void Awake() {
  Instance=this; font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
  if(prebuilt){if(!GetComponent<QuestExplorer>())gameObject.AddComponent<QuestExplorer>();return;}
  gold=Mat("Gold",new Color(.77f,.49f,.13f),.65f); bronze=Mat("Bronze",new Color(.40f,.23f,.105f),.45f);
  silver=Mat("Silver",new Color(.63f,.65f,.66f),.5f); sand=Mat("Sand",new Color(.68f,.54f,.35f));
  linen=Mat("Linen",new Color(.86f,.81f,.67f)); blue=Mat("Blue",new Color(.075f,.15f,.29f));
  purple=Mat("Purple",new Color(.22f,.08f,.25f)); scarlet=Mat("Scarlet",new Color(.48f,.07f,.065f));
  dark=Mat("Dark leather",new Color(.15f,.105f,.07f)); bread=Mat("Bread",new Color(.69f,.43f,.20f));
  flame=Mat("Oil flame",new Color(1,.66f,.14f),0,1); ink=Mat("Ink",new Color(.055f,.075f,.075f),0,1);
  water=Mat("Water",new Color(.12f,.29f,.31f),.6f);
  SetRealisticEnvironment();
  architecture=new GameObject("Architecture — batched by material").transform; architecture.SetParent(transform);
  BuildLandscape(); BuildCourtyard(); BuildTent();
  Batch(architecture);
  BuildAltar(); BuildBasin(); BuildTable(); BuildLampstand(); BuildIncenseAltar(); BuildArk();
  // Batch each furnishing separately so close-range decoration can still disappear at distance.
  for(int i=1;i<transform.childCount;i++) {var t=transform.GetChild(i);if(t.name.StartsWith("Furnishing"))Batch(t);}
  BuildWayfinding();
  if(lampCount!=7||breadCount!=12||toolCount!=5)throw new Exception("Biblical inventory validation failed");
  Debug.Log($"TABERNACLE_INVENTORY lamps={lampCount} loaves={breadCount} altarToolTypes={toolCount} cubit={Cubit}");
  gameObject.AddComponent<QuestExplorer>();
 }
 Material Mat(string n,Color c,float metal=0,float glow=0)=>PhysicalMaterial(n,c,metal,glow);
 Transform Group(string n,Vector3 p){var t=new GameObject(n).transform;t.SetParent(transform);t.position=p;return t;}
 public GameObject Shape(PrimitiveType type,string n,Transform parent,Vector3 p,Vector3 scale,Material m,bool collision=false){
  var g=GameObject.CreatePrimitive(type);g.name=n;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=scale;
  var r=g.GetComponent<MeshRenderer>();r.sharedMaterial=m;r.shadowCastingMode=ShadowCastingMode.On;r.receiveShadows=true;
  var c=g.GetComponent<Collider>();if(!collision)Release(c);else g.layer=8;
  return g;
 }
 GameObject Box(string n,Transform t,Vector3 p,Vector3 s,Material m,bool c=false)=>(m==gold||m==bronze||m==silver)&&Mathf.Min(s.x,Mathf.Min(s.y,s.z))>.035f?RoundedBox(n,t,p,s,m,c):Shape(PrimitiveType.Cube,n,t,p,s,m,c);
 GameObject Ball(string n,Transform t,Vector3 p,Vector3 s,Material m)=>Shape(PrimitiveType.Sphere,n,t,p,s,m);
 GameObject Cyl(string n,Transform t,Vector3 p,float radius,float height,Material m)=>Shape(PrimitiveType.Cylinder,n,t,p,new Vector3(radius*2,height/2,radius*2),m);
 void Rod(string n,Transform t,Vector3 a,Vector3 b,float r,Material m){var g=Cyl(n,t,(a+b)/2,r,Vector3.Distance(a,b),m);g.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);}
 void Torus(string name,Transform t,Vector3 p,float radius,float tube,Material m,int segments=24){
  var g=new GameObject(name);g.transform.SetParent(t,false);g.transform.localPosition=p;var mesh=new Mesh();int sides=6;var verts=new Vector3[(segments+1)*(sides+1)];var norms=new Vector3[verts.Length];var tris=new List<int>();
  for(int i=0;i<=segments;i++)for(int j=0;j<=sides;j++){float a=i*2*Mathf.PI/segments,b=j*2*Mathf.PI/sides;var n=new Vector3(Mathf.Cos(a)*Mathf.Cos(b),Mathf.Sin(b),Mathf.Sin(a)*Mathf.Cos(b));int k=i*(sides+1)+j;verts[k]=new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius)+tube*n;norms[k]=n;if(i<segments&&j<sides){int q=k+sides+1;tris.AddRange(new[]{k,k+1,q,k+1,q+1,q});}}
  mesh.vertices=verts;mesh.normals=norms;mesh.triangles=tris.ToArray();mesh.RecalculateBounds();g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=m;
 }
 // Open bowls have a visible inside and rim, rather than an opaque sphere pretending to be a basin.
 void Bowl(string name,Transform t,Vector3 p,float r,float h,Material m,int seg=32){
  var g=new GameObject(name);g.transform.SetParent(t,false);g.transform.localPosition=p;var mesh=new Mesh();var vs=new List<Vector3>();var uv=new List<Vector2>();var tri=new List<int>();
  float[] rr={.35f*r,.62f*r,r,r-.025f,.62f*r-.015f,.35f*r};float[] yy={0,.22f*h,h,h-.025f,.22f*h+.025f,.035f};
  for(int k=0;k<rr.Length;k++)for(int j=0;j<=seg;j++){float a=j*2*Mathf.PI/seg;vs.Add(new Vector3(Mathf.Cos(a)*rr[k],yy[k],Mathf.Sin(a)*rr[k]));uv.Add(new Vector2((float)j/seg,(float)k/5));}
  for(int k=0;k<5;k++)for(int j=0;j<seg;j++){int a=k*(seg+1)+j,b=a+seg+1;tri.AddRange(new[]{a,b,a+1,a+1,b,b+1});}
  mesh.SetVertices(vs);mesh.SetUVs(0,uv);mesh.SetTriangles(tri,0);mesh.RecalculateNormals();mesh.RecalculateBounds();g.AddComponent<MeshFilter>().sharedMesh=mesh;g.AddComponent<MeshRenderer>().sharedMaterial=m;
  Cyl(name+" floor",t,p+new Vector3(0,.025f,0),.35f*r,.035f,m);Torus(name+" rim",t,p+Vector3.up*h,r-.012f,.018f,m);
 }
 void BuildLandscape(){
  BuildDesert();
 }
 void Post(Transform t,Vector3 p,float h,Material shaft,Material foot){Cyl("Post",t,p+Vector3.up*h/2,.045f,h,shaft);Cyl("Socket",t,p+Vector3.up*.055f,.105f,.11f,foot);Ball("Post hook",t,p+Vector3.up*(h-.03f),Vector3.one*.10f,shaft==gold?gold:silver);}
 void BuildCourtyard(){
  float w=Width/2,h=5*Cubit;
  Fabric("South linen enclosure",architecture,new Vector3(-w,h/2,Length/2),Length,h,linen,true,90);
  Fabric("North linen enclosure",architecture,new Vector3(w,h/2,Length/2),Length,h,linen,true,90);
  Fabric("West linen enclosure",architecture,new Vector3(0,h/2,Length),Width,h,linen,true);
  float gate=20*Cubit;
  foreach(int sign in new[]{-1,1})Fabric("East linen enclosure",architecture,new Vector3(sign*(w+gate/2)/2,h/2,0),w-gate/2,h,linen,true);
  // Twenty longitudinal posts per side, ten across each end. Shared corners counted once.
  for(int i=0;i<20;i++){Post(architecture,new Vector3(-w,0,i*Length/20),h,bronze,bronze);Post(architecture,new Vector3(w,0,(i+1)*Length/20),h,bronze,bronze);}
  for(int i=0;i<10;i++){Post(architecture,new Vector3(-w+(i+1)*Width/10,0,0),h,bronze,bronze);Post(architecture,new Vector3(-w+i*Width/10,0,Length),h,bronze,bronze);}
  // Parted entrance screen leaves a safe, visible walking route.
  foreach(int s in new[]{-1,1})Screen(architecture,new Vector3(s*3.6f,h/2,-.025f),1.7f,h,false);
  for(int i=0;i<16;i++){float z=i*Length/15;Rod("Guy rope",architecture,new Vector3(-w,h-.1f,z),new Vector3(-w-.8f,.02f,z+.2f),.009f,dark);Rod("Guy rope",architecture,new Vector3(w,h-.1f,z),new Vector3(w+.8f,.02f,z+.2f),.009f,dark);}
 }
 void Screen(Transform t,Vector3 p,float w,float h,bool cherub){
  DrapedScreen(t,p,w,h,cherub);
 }
 void BuildTent(){
  float w=10*Cubit,h=10*Cubit,front=26,back=front+30*Cubit,veil=front+20*Cubit;
  for(int i=0;i<20;i++)foreach(int s in new[]{-1,1}){
   float z=front+(i+.5f)*1.5f*Cubit;Box("Gold-covered acacia frame",architecture,new Vector3(s*w/2,h/2,z),new Vector3(.12f,h,1.5f*Cubit-.012f),gold,true);
   for(int j=0;j<2;j++)Box("Silver frame socket",architecture,new Vector3(s*w/2,.06f,z+(j==0?-.19f:.19f)),new Vector3(.20f,.12f,.20f),silver);
  }
  Box("Rear gold wall",architecture,new Vector3(0,h/2,back),new Vector3(w,h,.14f),gold,true);
  Box("Inner linen roof",architecture,new Vector3(0,h,front+15*Cubit),new Vector3(w,.025f,30*Cubit),purple);
  Box("Goat hair and skin roof covering",architecture,new Vector3(0,h+.09f,front+15*Cubit),new Vector3(w+.5f,.12f,30*Cubit+.5f),dark);
  for(int i=0;i<9;i++){float z=front+.7f+i*1.5f;Box("Ceiling embroidered stripe",architecture,new Vector3(0,h-.024f,z),new Vector3(w,.008f,.045f),gold);}
  // Five entrance posts: gold over acacia, bronze sockets. Open central screen for exploration.
  for(int i=0;i<5;i++){float x=-w/2+i*w/4;Post(architecture,new Vector3(x,0,front),h,gold,bronze);}
  foreach(int s in new[]{-1,1})Screen(architecture,new Vector3(s*w*.375f,h/2,front-.02f),w*.22f,h,false);
  DressTent();
  Screen(architecture,new Vector3(0,h/2,veil),w,h,true);
  for(int i=0;i<4;i++){float x=-w/2+i*w/3;Cyl("Veil gold post",architecture,new Vector3(x,h/2,veil-.10f),.055f,h,gold);Box("Veil silver socket",architecture,new Vector3(x,.07f,veil-.10f),new Vector3(.19f,.14f,.19f),silver);}
 }
 void BuildAltar(){
  var t=Group("Furnishing — bronze altar",new Vector3(0,0,8));float w=5*Cubit,h=3*Cubit;
  foreach(int s in new[]{-1,1}){Box("Hollow altar side",t,new Vector3(s*w/2,h/2,0),new Vector3(.065f,h,w),bronze,true);Box("Hollow altar side",t,new Vector3(0,h/2,s*w/2),new Vector3(w,h,.065f),bronze,true);}
  Box("Altar exclusion volume",t,new Vector3(0,h/2,0),new Vector3(w,h,w),bronze,true).GetComponent<Renderer>().enabled=false;
  foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1}){var g=Box("Altar horn",t,new Vector3(x*(w/2-.08f),h+.12f,z*(w/2-.08f)),new Vector3(.11f,.28f,.11f),bronze);g.transform.localRotation=Quaternion.Euler(z*12,0,-x*12);}
  // Network at mid-height; precise network interpretation is documented.
  for(int i=0;i<12;i++){float v=-w/2+i*w/11;Rod("Bronze network",t,new Vector3(v,h/2,-w/2),new Vector3(v,h/2,w/2),.012f,bronze);Rod("Bronze network",t,new Vector3(-w/2,h/2,v),new Vector3(w/2,h/2,v),.012f,bronze);}
  for(int s=-1;s<=1;s+=2){Rod("Altar carrying pole",t,new Vector3(s*(w/2+.12f),.9f,-1.7f),new Vector3(s*(w/2+.12f),.9f,1.7f),.035f,bronze);}
  BuildTools(new Vector3(-2.2f,0,8));
 }
 void BuildTools(Vector3 p){
  var t=Group("Furnishing — five altar tool types",p);
  Bowl("Ash pot",t,new Vector3(0,0,0),.22f,.38f,bronze);Rod("Ash pot handle",t,new Vector3(-.22f,.35f,0),new Vector3(0,.65f,0),.018f,bronze);Rod("Ash pot handle",t,new Vector3(0,.65f,0),new Vector3(.22f,.35f,0),.018f,bronze);
  Box("Shovel blade",t,new Vector3(-.6f,.025f,0),new Vector3(.22f,.04f,.30f),bronze);Rod("Shovel shaft",t,new Vector3(-.6f,.05f,.1f),new Vector3(-.6f,.10f,.95f),.018f,bronze);
  Bowl("Blood basin",t,new Vector3(0,0,.8f),.27f,.13f,bronze);
  Rod("Meat fork shaft",t,new Vector3(-1,.04f,0),new Vector3(-1,.04f,1),.018f,bronze);for(int j=0;j<3;j++)Rod("Meat fork prong",t,new Vector3(-1+(j-1)*.065f,.04f,1),new Vector3(-1+(j-1)*.065f,.04f,1.22f),.012f,bronze);Rod("Fork crossbar",t,new Vector3(-1.065f,.04f,1),new Vector3(-.935f,.04f,1),.012f,bronze);
  Bowl("Firepan",t,new Vector3(.15f,0,-.65f),.20f,.07f,bronze);Rod("Firepan handle",t,new Vector3(.15f,.04f,-.8f),new Vector3(.15f,.04f,-1.2f),.018f,bronze);toolCount=5;
 }
 void BuildBasin(){var t=Group("Furnishing — basin",new Vector3(0,0,16.5f));Cyl("Bronze stand foot",t,new Vector3(0,.06f,0),.43f,.12f,bronze);Cyl("Bronze basin stand",t,new Vector3(0,.40f,0),.15f,.65f,bronze);Bowl("Bronze washing basin",t,new Vector3(0,.7f,0),.65f,.35f,bronze);Cyl("Water surface",t,new Vector3(0,1.01f,0),.57f,.008f,water);Box("Basin collision",t,new Vector3(0,.55f,0),new Vector3(1.3f,1.1f,1.3f),bronze,true).GetComponent<Renderer>().enabled=false;}
 void BuildTable(){
  var t=Group("Furnishing — bread table",new Vector3(1.50f,0,30));float length=2*Cubit,width=Cubit,height=1.5f*Cubit;
  Box("Table top",t,new Vector3(0,height,0),new Vector3(width,.05f,length),gold,true);
  foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1}){Box("Table leg",t,new Vector3(x*(width/2-.045f),height/2,z*(length/2-.045f)),new Vector3(.055f,height,.055f),gold);Torus("Table carrying ring",t,new Vector3(x*(width/2+.035f),height-.13f,z*(length/2-.04f)),.043f,.009f,gold);}
  foreach(int s in new[]{-1,1}){Box("Table rim",t,new Vector3(s*width/2,height+.045f,0),new Vector3(.025f,.06f,length),gold);Box("Table rim",t,new Vector3(0,height+.045f,s*length/2),new Vector3(width,.06f,.025f),gold);Rod("Table carrying pole",t,new Vector3(s*(width/2+.045f),height-.13f,-.75f),new Vector3(s*(width/2+.045f),height-.13f,.75f),.018f,gold);}
  for(int stack=0;stack<2;stack++)for(int loaf=0;loaf<6;loaf++){Ball("Bread loaf "+(++breadCount)+" of twelve",t,new Vector3(0,height+.085f+loaf*.066f,(stack==0?-.21f:.21f)),new Vector3(.33f,.063f,.31f),bread);}
  // Small vessels represented as types, without asserting a biblical quantity for each type.
  var v=Group("Furnishing — gold table vessels",t.position+Vector3.up*(height+.025f));
  Bowl("Gold plate beneath bread",v,new Vector3(0,-.015f,-.21f),.17f,.025f,gold);
  Bowl("Gold cup",v,new Vector3(.16f,.005f,.38f),.035f,.045f,gold);Rod("Gold cup handle",v,new Vector3(.17f,.04f,.38f),new Vector3(.21f,.04f,.38f),.007f,gold);
  var jug=new GameObject("Gold pitcher").transform;jug.SetParent(v,false);jug.localPosition=new Vector3(.12f,0,0);jug.localScale=Vector3.one*.55f;Pitcher(jug,Vector3.zero,gold);
  Bowl("Gold pouring bowl",v,new Vector3(-.13f,0,0),.065f,.055f,gold);
  foreach(int s in new[]{-1,1}){Bowl("Frankincense cup — profile interpreted",t,new Vector3(-.145f,height+.05f,s*.38f),.045f,.055f,gold);for(int k=0;k<4;k++)Ball("Pure frankincense",t,new Vector3(-.145f+(k%2)*.015f,height+.105f,s*.38f+(k/2)*.015f),Vector3.one*.017f,linen);}
 }
 void Pitcher(Transform t,Vector3 p,Material m){Ball("Oil or libation pitcher body",t,p+Vector3.up*.13f,new Vector3(.16f,.23f,.16f),m);Cyl("Pitcher neck",t,p+Vector3.up*.26f,.045f,.10f,m);Torus("Pitcher lip",t,p+Vector3.up*.315f,.05f,.008f,m);Rod("Pitcher handle",t,p+new Vector3(.05f,.28f,0),p+new Vector3(.13f,.22f,0),.012f,m);Rod("Pitcher handle",t,p+new Vector3(.13f,.22f,0),p+new Vector3(.08f,.1f,0),.012f,m);}
 void BuildLampstand(){
  var t=Group("Furnishing — seven lampstand",new Vector3(-1.40f,0,30));
  var detail=Group("Lampstand close detail",t.position);closeDetails.Add(detail);
  Cyl("Lampstand base — interpreted",t,new Vector3(0,.04f,0),.28f,.08f,gold);Cyl("Central gold shaft",t,new Vector3(0,.82f,0),.035f,1.5f,gold);
  for(int pair=1;pair<=3;pair++)foreach(int s in new[]{-1,1}){
   float start=.40f+(3-pair)*.23f;Vector3 prev=new Vector3(0,start,0);
   for(int j=1;j<=12;j++){float a=(float)j/12*Mathf.PI/2;var next=new Vector3(s*pair*.22f*Mathf.Sin(a),start+(1.58f-start)*(1-Mathf.Cos(a)),0);Rod("Beaten gold branch",t,prev,next,.025f,gold);prev=next;}
   for(int j=1;j<=3;j++){float a=(float)j/4*Mathf.PI/2;Blossom(detail,new Vector3(s*pair*.22f*Mathf.Sin(a),start+(1.58f-start)*(1-Mathf.Cos(a)),0));}
   Ball("Knob under branch pair",t,new Vector3(0,start,0),new Vector3(.10f,.08f,.10f),gold);
  }
  for(int i=0;i<4;i++)Blossom(detail,new Vector3(0,.48f+i*.32f,0));
  for(int i=-3;i<=3;i++){
   Vector3 p=new Vector3(i*.22f,1.60f,0);Bowl("Oil lamp "+(++lampCount)+" of seven",t,p,.085f,.065f,gold,20);
   Cyl("Visible olive oil",detail,p+new Vector3(0,.052f,0),.06f,.005f,bronze);
   Box("Short wick",detail,p+new Vector3(0,.07f,-.064f),new Vector3(.009f,.025f,.025f),dark);
   Ball("Small oil flame",t,p+new Vector3(0,.10f,-.065f),new Vector3(.018f,.055f,.018f),flame);
   AddOilLight(t,p+new Vector3(0,.13f,-.065f));
  }
  Rod("Gold wick tongs",t,new Vector3(.35f,.025f,-.3f),new Vector3(.53f,.025f,-.6f),.010f,gold);Rod("Gold wick tongs",t,new Vector3(.38f,.025f,-.28f),new Vector3(.53f,.025f,-.6f),.010f,gold);
  Bowl("Gold wick tray",t,new Vector3(.2f,0,-.4f),.10f,.025f,gold);Pitcher(t,new Vector3(-.38f,0,-.35f),bronze);
  Box("Lampstand collision",t,new Vector3(0,.85f,0),new Vector3(1.5f,1.8f,.25f),gold,true).GetComponent<Renderer>().enabled=false;
  Batch(detail);
 }
 void Blossom(Transform t,Vector3 p){AlmondFlower(t,p);}
 void BuildIncenseAltar(){
  var t=Group("Furnishing — incense altar",new Vector3(0,0,34));float w=Cubit,h=2*Cubit;
  Box("Gold-covered incense altar",t,new Vector3(0,h/2,0),new Vector3(w,h,w),gold,true);Box("Incense altar gold molding",t,new Vector3(0,h,0),new Vector3(w+.035f,.035f,w+.035f),gold);
  foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1})Box("Incense altar horn",t,new Vector3(x*(w/2-.03f),h+.045f,z*(w/2-.03f)),new Vector3(.045f,.10f,.045f),gold);
  Cyl("Incense on altar",t,new Vector3(0,h+.025f,0),.12f,.015f,dark);
  foreach(int s in new[]{-1,1})Rod("Incense altar pole",t,new Vector3(s*(w/2+.045f),h-.18f,-.55f),new Vector3(s*(w/2+.045f),h-.18f,.55f),.018f,gold);
 }
 void BuildArk(){DetailedArk();}
 public TextMesh Text(string n,Transform parent,Vector3 p,string content,float size,Color color,TextAnchor anchor=TextAnchor.MiddleCenter){
  var g=new GameObject(n);g.transform.SetParent(parent,false);g.transform.localPosition=p;var tm=g.AddComponent<TextMesh>();tm.text=content;tm.font=font;tm.fontSize=64;tm.characterSize=size;tm.color=color;tm.anchor=anchor;tm.alignment=TextAlignment.Center;g.GetComponent<MeshRenderer>().sharedMaterial=font.material;return tm;
 }
 void BuildWayfinding(){
  var t=Group("Welcome information",new Vector3(-2.7f,0,-1.5f));
  Box("Information panel",t,new Vector3(0,1.5f,.02f),new Vector3(2.8f,1.6f,.07f),ink);
  Text("Title",t,new Vector3(0,2.10f,-.025f),"THE TABERNACLE",.045f,new Color(.94f,.78f,.43f));
  Text("Welcome",t,new Vector3(0,1.40f,-.025f),"A walk through the wilderness sanctuary\n\nRight trigger: aim, release to teleport\nRight stick: snap turn     A: next tour stop\nB: performance display\n\nText-based layout • Interpretive forms\nExodus 25–30; 40",.020f,new Color(.9f,.87f,.76f));
  foreach(var p in stops){Torus("Tour stop marker",transform,p+Vector3.up*.012f,.36f,.018f,gold);}
 }
 public bool CanStand(Vector3 p){
  if(p.x<-Width/2+.35f||p.x>Width/2-.35f||p.z<-6||p.z>Length-.35f)return false;
  if(p.z>26&&p.z<26+30*Cubit&&(Mathf.Abs(p.x)>10*Cubit/2-.35f&&Mathf.Abs(p.x)<10*Cubit/2+.4f))return false;
  return !Physics.CheckCapsule(p+Vector3.up*.30f,p+Vector3.up*1.65f,.22f,1<<8,QueryTriggerInteraction.Ignore);
 }
 public void UpdateDetails(Vector3 eye){foreach(var d in closeDetails){bool active=Vector3.Distance(eye,d.position)<7; if(d.gameObject.activeSelf!=active)d.gameObject.SetActive(active);}}
 void Batch(Transform root){
  var all=root.GetComponentsInChildren<MeshFilter>();var byMaterial=new Dictionary<Material,List<CombineInstance>>();var remove=new List<GameObject>();
  foreach(var mf in all){var r=mf.GetComponent<MeshRenderer>();if(!r||!r.enabled||!mf.sharedMesh)continue;var m=r.sharedMaterial;if(!byMaterial.ContainsKey(m))byMaterial[m]=new List<CombineInstance>();byMaterial[m].Add(new CombineInstance{mesh=mf.sharedMesh,transform=root.worldToLocalMatrix*mf.transform.localToWorldMatrix});remove.Add(mf.gameObject);}
  foreach(var entry in byMaterial){var mesh=new Mesh{name=root.name+" / "+entry.Key.name,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(entry.Value.ToArray(),true,true);var g=new GameObject(entry.Key.name+" batch");g.transform.SetParent(root,false);g.AddComponent<MeshFilter>().sharedMesh=mesh;var r=g.AddComponent<MeshRenderer>();r.sharedMaterial=entry.Key;r.shadowCastingMode=ShadowCastingMode.On;r.receiveShadows=true;}
  foreach(var g in remove){Release(g.GetComponent<MeshRenderer>());Release(g.GetComponent<MeshFilter>());}
 }
 void Release(UnityEngine.Object o){if(Application.isPlaying)Destroy(o);else DestroyImmediate(o);}
}
