using System;
using UnityEngine;

public sealed class ControllerTablet : MonoBehaviour {
 public Action<int> VisitStation;
 public Action<bool> SetRaisedView;
 Transform board;TextMesh title,body,tabs,footer;Font uncial;
 int page,station;bool held,raised,pendingEntry;bool fitPending;
 Material stone,letterMaterial;
 public static readonly string[] TourButtons={"Courtyard entrance","Bronze altar","Washing basin","Holy Place","Inner veil","Most Holy Place"};
 public void Initialize(Transform hand){
  transform.SetParent(hand,false);transform.localPosition=new Vector3(0,.18f,.12f);transform.localRotation=Quaternion.Euler(58,0,0);
  board=transform;uncial=Resources.Load<Font>("Fonts/UncialAntiqua")??Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
  stone=new Material(Resources.Load<Shader>("TabernacleSurface")){color=new Color(1,1,1),mainTexture=Resources.Load<Texture2D>("Art/TabletStone")};
  var slab=GameObject.CreatePrimitive(PrimitiveType.Cube);slab.name="Stone instruction tablet";slab.transform.SetParent(board,false);slab.transform.localScale=new Vector3(.44f,.64f,.025f);slab.GetComponent<Renderer>().sharedMaterial=stone;slab.layer=10;
  title=Label("Tablet title",new Vector3(0,.266f,-.014f),.006f);
  tabs=Label("Tablet tabs",new Vector3(0,.205f,-.015f),.0031f);
  body=Label("Tablet content",new Vector3(0,-.015f,-.015f),.0035f);
  footer=Label("Tablet footer",new Vector3(0,-.281f,-.015f),.0026f);
  Refresh();
 }
 TextMesh Label(string name,Vector3 p,float size){
  var g=new GameObject(name);g.transform.SetParent(board,false);g.transform.localPosition=p;
  var t=g.AddComponent<TextMesh>();t.font=uncial;t.fontSize=64;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=new Color(.085f,.065f,.045f);
  if(!letterMaterial)letterMaterial=new Material(uncial.material);
  g.GetComponent<Renderer>().sharedMaterial=letterMaterial;return t;
 }
 void LateUpdate(){if(fitPending){Fit(title,.39f,.07f);Fit(tabs,.40f,.04f);Fit(body,.39f,.40f);Fit(footer,.39f,.04f);fitPending=false;}}
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
   if(pendingEntry){if(p.y>-.16f&&p.y<.08f){pendingEntry=false;VisitStation?.Invoke(5);}Refresh();return;}
   int index=Mathf.RoundToInt((.125f-p.y)/.057f);
   if(index>=0&&index<6){if(index==5)pendingEntry=true;else VisitStation?.Invoke(index);Refresh();}
  }else if(page==2){
   if(p.y>.07f&&p.y<.15f){TabernacleMusic.Instance?.SetMuted(!(TabernacleMusic.Instance?.Muted??false));}
   else if(p.y>-.01f&&p.y<.07f){TabernacleMusic.Instance?.SetVolume((TabernacleMusic.Instance?.Volume??.16f)+(p.x<0?-.05f:.05f));}
   else if(p.y>-.12f&&p.y<-.03f){raised=!raised;SetRaisedView?.Invoke(raised);}
   Refresh();
  }
 }
 public void SetStation(int value){station=value;Refresh();}
 public void Toggle(){gameObject.SetActive(!gameObject.activeSelf);held=true;}
 void Refresh(){
  if(!title)return;title.text="The Tabernacle";
  tabs.text=(page==0?"[Instructions]":"Instructions")+"   "+(page==1?"[Tour]":"Tour")+"   "+(page==2?"[Settings]":"Settings");
  if(page==0)body.text="Right trigger: point and select\nAim away: hold, release to teleport\nRight stick: turn\nA: next tour stop\nY: show or hide this tablet\n\n"+TourButtons[station]+"\n"+(station==5?"Educational access beyond the veil.\nHistorical access was restricted.\nLeviticus 16":"Choose Tour to visit any room.\nMost Holy Place is behind the veil.");
  else if(page==1){body.text=pendingEntry?"Beyond the veil\n\nHistorically, access was restricted.\nLeviticus 16\n\nEnter educational view\n\nPoint here and press trigger":string.Join("\n\n",TourButtons);}
  else body.text="Music: "+((TabernacleMusic.Instance?.Muted??false)?"Off":"On")+"\n\nVolume  -      "+Mathf.RoundToInt((TabernacleMusic.Instance?.Volume??.16f)*100)+"%      +\n\n"+(raised?"Back down":"Help me see higher")+"\n\nRaise your left hand to read.\nLook up to continue exploring.";
  footer.text="Point with your right controller";fitPending=true;
 }
 void OnDestroy(){if(stone)Destroy(stone);if(letterMaterial)Destroy(letterMaterial);}
}
