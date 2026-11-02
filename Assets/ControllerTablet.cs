using System;
using UnityEngine;

public sealed class ControllerTablet : MonoBehaviour {
 public Action<int> VisitStation;
 public Action<bool> SetRaisedView;
 public TabernacleExperience Experience;
 readonly Renderer[] settingsDividers=new Renderer[6];
 TextMesh settingsHeading;readonly TextMesh[] settingsRows=new TextMesh[5];int settingsSection;
 static readonly string[] SettingsSections={"Audio","Avatar","Comfort"};
 Transform board;TextMesh title,body,footer;Font uncial;
 readonly TextMesh[] tabs=new TextMesh[3];readonly Renderer[][] borders=new Renderer[3][];
 int page,station;bool held,raised,pendingEntry;bool fitPending;
 Material stone,letterMaterial,borderMaterial,selectedMaterial;
 public static readonly string[] TourButtons={"Courtyard entrance","Bronze altar","Washing basin","Holy Place","Inner veil","Most Holy Place"};
 public void Initialize(Transform hand){
  transform.SetParent(hand,false);transform.localPosition=new Vector3(0,.14f,.12f);transform.localScale=Vector3.one*.75f;transform.localRotation=Quaternion.Euler(58,0,0);
  board=transform;uncial=Resources.Load<Font>("Fonts/UncialAntiqua")??Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
  stone=new Material(Resources.Load<Shader>("TabernacleSurface")){color=new Color(1,1,1),mainTexture=Resources.Load<Texture2D>("Art/TabletStone")};
  var slab=GameObject.CreatePrimitive(PrimitiveType.Cube);slab.name="Stone instruction tablet";slab.transform.SetParent(board,false);slab.transform.localScale=new Vector3(.44f,.64f,.025f);slab.GetComponent<Renderer>().sharedMaterial=stone;slab.layer=10;
  title=Label("Tablet title",new Vector3(0,.266f,-.014f),.006f);
  borderMaterial=new Material(Resources.Load<Shader>("TabernacleSurface")){color=new Color(.20f,.14f,.08f)};
  selectedMaterial=new Material(Resources.Load<Shader>("TabernacleSurface")){color=new Color(.65f,.37f,.08f)};
  string[] names={"Instructions","Tour","Settings"};
  for(int i=0;i<3;i++){
   float x=(i-1)*.134f;
   tabs[i]=Label(names[i]+" tab",new Vector3(x,.205f,-.018f),.0031f);tabs[i].text=names[i];
   borders[i]=new Renderer[]{Border(x,.239f,.132f,.003f),Border(x,.171f,.132f,.003f),Border(x-.066f,.205f,.003f,.068f),Border(x+.066f,.205f,.003f,.068f)};
  }
  body=Label("Tablet content",new Vector3(0,-.015f,-.015f),.0035f);
  footer=Label("Tablet footer",new Vector3(0,-.281f,-.015f),.0026f);
  settingsHeading=Label("Settings section",new Vector3(0,.123f,-.018f),.0031f);
  for(int i=0;i<5;i++)settingsRows[i]=Label("Settings row "+i,new Vector3(0,.055f-i*.065f,-.018f),.0031f);
  for(int i=0;i<6;i++)settingsDividers[i]=Border(0,.0875f-i*.065f,.395f,.001f);
  Refresh();
 }
 Renderer Border(float x,float y,float width,float height){
  var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name="Tab border";g.transform.SetParent(board,false);g.transform.localPosition=new Vector3(x,y,-.016f);g.transform.localScale=new Vector3(width,height,.002f);
  var collider=g.GetComponent<Collider>();if(Application.isPlaying)Destroy(collider);else DestroyImmediate(collider);
  var renderer=g.GetComponent<Renderer>();renderer.sharedMaterial=borderMaterial;return renderer;
 }
 TextMesh Label(string name,Vector3 p,float size){
  var g=new GameObject(name);g.transform.SetParent(board,false);g.transform.localPosition=p;
  var t=g.AddComponent<TextMesh>();t.font=uncial;t.fontSize=64;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=new Color(.085f,.065f,.045f);
  if(!letterMaterial)letterMaterial=new Material(uncial.material);
  g.GetComponent<Renderer>().sharedMaterial=letterMaterial;return t;
 }
 void LateUpdate(){if(fitPending){Fit(title,.39f,.07f);foreach(var tab in tabs)Fit(tab,.119f,.038f);Fit(body,.39f,.40f);Fit(footer,.39f,.04f);Fit(settingsHeading,.38f,.04f);foreach(var row in settingsRows)Fit(row,.38f,.047f);fitPending=false;}}
 void Fit(TextMesh text,float width,float height){
  text.transform.localScale=Vector3.one;
  var b=text.GetComponent<Renderer>().localBounds.size;
  float scale=Mathf.Min(1,width/Mathf.Max(.001f,b.x),height/Mathf.Max(.001f,b.y));text.transform.localScale=Vector3.one*scale;
 }
 public bool Point(Transform pointer,bool pressed,out Vector3 hitPoint){
  hitPoint=Vector3.zero;RaycastHit ray=default;bool hit=gameObject.activeInHierarchy&&Physics.Raycast(pointer.position,pointer.forward,out ray,2.5f,1<<10,QueryTriggerInteraction.Ignore);
  if(hit){hitPoint=ray.point;if(pressed&&!held)Select(transform.InverseTransformPoint(hitPoint));}
  held=pressed;return hit;
 }
 void Select(Vector3 p){
  if(p.y>.17f&&p.y<.24f){page=p.x<-.067f?0:p.x>.067f?2:1;pendingEntry=false;Refresh();return;}
  if(page==1){
   if(Experience&&Experience.IntroductionPlaying)return;
   if(pendingEntry){if(p.y>-.16f&&p.y<.08f){pendingEntry=false;VisitStation?.Invoke(5);}Refresh();return;}
   int index=Mathf.RoundToInt((.125f-p.y)/.057f);
   if(index>=0&&index<6){if(index==5)pendingEntry=true;else VisitStation?.Invoke(index);Refresh();}
  }else if(page==2){
   if(p.y>.095f&&p.y<.15f){settingsSection=(settingsSection+(p.x<0?2:1))%3;Refresh();return;}
   int row=Mathf.RoundToInt((.055f-p.y)/.065f);if(row<0||row>=5||Mathf.Abs(p.y-(.055f-row*.065f))>.028f)return;
   if(settingsSection==0){
    if(row==0)TabernacleMusic.Instance?.SetMuted(!(TabernacleMusic.Instance?.Muted??false));
    if(row==1)TabernacleMusic.Instance?.SetVolume((TabernacleMusic.Instance?.Volume??.16f)+(p.x<0?-.05f:.05f));
    if(Experience){if(row==2)Experience.SetGuideMuted(!Experience.GuideMuted);if(row==3)Experience.SetCaptions(!Experience.Captions);if(row==4)Experience.Replay();}
   }else if(settingsSection==1&&Experience){
    if(row==0)Experience.Avatar.SetFemale(!Experience.Avatar.Female);
    if(row==1)Experience.Avatar.SetChild(!Experience.Avatar.ChildFace);
    if(row==2)Experience.SetReflection(!Experience.Reflection.Enabled);
    if(row==3)Experience.Avatar.Calibrate();
   }else if(settingsSection==2){
    if(row==0){raised=!raised;SetRaisedView?.Invoke(raised);}
    if(Experience){if(row==1)Experience.CycleTurn();if(row==2)Experience.SetGuideVisible(!Experience.GuideVisible);if(row==3)Experience.Skip();}
    if(row==4&&!(Experience&&Experience.IntroductionPlaying))VisitStation?.Invoke(0);
   }
   Refresh();
  }
 }
 public void RequestStation(int value){if(Experience&&Experience.IntroductionPlaying)return;if(value==5){page=1;pendingEntry=true;gameObject.SetActive(true);Refresh();}else VisitStation?.Invoke(value);}
 public void UpdateContent(){Refresh();}
 public void SyncRaisedView(bool value){raised=value;Refresh();}

 public void SetStation(int value){station=value;Refresh();}
 public void Toggle(){gameObject.SetActive(!gameObject.activeSelf);held=true;}
 void Refresh(){
  if(!title)return;title.text="The Tabernacle";
  for(int i=0;i<3;i++)foreach(var border in borders[i])border.sharedMaterial=i==page?selectedMaterial:borderMaterial;
  foreach(var divider in settingsDividers)divider.gameObject.SetActive(page==2);
  body.gameObject.SetActive(page!=2);settingsHeading.gameObject.SetActive(page==2);foreach(var row in settingsRows)row.gameObject.SetActive(page==2);
  if(page==0&&Experience&&Experience.Captions&&!string.IsNullOrEmpty(Experience.Caption))body.text="Guide says\n\n"+Wrap(Experience.Caption,30)+"\n\n"+(Experience.IntroductionPlaying?"Welcome in progress\nSettings > Comfort: skip welcome":"A: next tour stop");
  else if(page==0)body.text="Right trigger: point and select\nAim away: hold, release to teleport\nRight stick: turn\nA: next tour stop\nY: show or hide this tablet\n\n"+TourButtons[station]+"\n"+(station==5?"Educational access beyond the veil.\nHistorical access was restricted.\nLeviticus 16":"Choose Tour to visit any room.\nMost Holy Place is behind the veil.");
  else if(page==1){body.text=pendingEntry?"Beyond the veil\n\nHistorically, access was restricted.\nLeviticus 16\n\nEnter educational view\n\nPoint here and press trigger":string.Join("\n\n",TourButtons);}
  else if(page==2){
   settingsHeading.text="<    "+SettingsSections[settingsSection]+"    >";
   var e=Experience;
   string[] rows;
   if(settingsSection==0)rows=new[]{"Music: "+((TabernacleMusic.Instance?.Muted??false)?"Off":"On"),"Volume  -    "+Mathf.RoundToInt((TabernacleMusic.Instance?.Volume??.16f)*100)+"%    +","Guide voice: "+(e&&e.GuideMuted?"Off":"On"),"Tablet captions: "+(!e||e.Captions?"On":"Off"),"Replay guide"};
   else if(settingsSection==1)rows=new[]{"Character: "+(e&&e.Avatar.Female?"Female":"Male"),"Face: "+(!e||e.Avatar.ChildFace?"Child":"Adult"),"Laver reflection: "+(!e||e.Reflection.Enabled?"On":"Off"),"Match height"+(e?" ("+e.Avatar.Height.ToString("F2")+" m)":""),"Lean over water to see your face"};
   else rows=new[]{"Viewing assistance: "+(raised?"Back down":"Help me see higher"),"Snap turn: "+(e?e.TurnAngle:30)+" degrees","Robot guide: "+(!e||e.GuideVisible?"Shown":"Hidden"),e&&e.IntroductionPlaying?"Skip welcome":"Stop narration","Return to courtyard entrance"};
   for(int i=0;i<rows.Length;i++)settingsRows[i].text=rows[i];
  }
  footer.text=Experience&&Experience.IntroductionPlaying?"Welcome in progress | Settings to skip":"Point with your right controller";fitPending=true;
 }
 static string Wrap(string text,int width){
  var result=new System.Text.StringBuilder();int column=0;foreach(var word in text.Split(' ')){if(column>0&&column+word.Length+1>width){result.Append('\n');column=0;}if(column>0){result.Append(' ');column++;}result.Append(word);column+=word.Length;}return result.ToString();
 }
 void OnDestroy(){if(stone)Destroy(stone);if(letterMaterial)Destroy(letterMaterial);if(borderMaterial)Destroy(borderMaterial);if(selectedMaterial)Destroy(selectedMaterial);}
}
