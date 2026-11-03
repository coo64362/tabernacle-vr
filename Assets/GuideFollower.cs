using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Collision-aware companion. A grid path stays within the same walkable space as the visitor.
public sealed class GuideFollower : MonoBehaviour {
 Transform viewer;RobotGuide guide;readonly List<Vector3> path=new List<Vector3>();int waypoint;float nextPlan;Vector3 lastViewer;Coroutine planning;
 const float Step=.5f;const int NX=43,NZ=91;const float MinX=-10.5f,MinZ=0;
 public bool Walking {get;private set;}
 public void Initialize(RobotGuide owner,Transform user){guide=owner;viewer=user;lastViewer=user.position;}
 public void ResetPath(){path.Clear();waypoint=0;nextPlan=Time.unscaledTime+1;if(planning!=null){StopCoroutine(planning);planning=null;}}
 public static int Room(Vector3 p){if(p.z<26||p.z>39.716f||Mathf.Abs(p.x)>2.286f)return 0;return p.z<35.144f?1:2;}
 public static bool Clear(Vector3 a,Vector3 b){if(Room(a)!=Room(b))return false;var delta=b-a;float length=delta.magnitude;return length<.01f||!Physics.CapsuleCast(a+Vector3.up*.35f,a+Vector3.up*1.45f,.28f,delta/length,length,1<<8,QueryTriggerInteraction.Ignore);}
 public static bool Stand(Vector3 p)=>TabernacleWorld.Instance&&TabernacleWorld.Instance.CanStand(p)&&!Physics.CheckCapsule(p+Vector3.up*.35f,p+Vector3.up*1.45f,.28f,1<<8,QueryTriggerInteraction.Ignore);
 Vector3 Point(int id)=>new Vector3(MinX+(id%NX)*Step,0,MinZ+(id/NX)*Step);
 int Cell(Vector3 p)=>Mathf.Clamp(Mathf.RoundToInt((p.x-MinX)/Step),0,NX-1)+NX*Mathf.Clamp(Mathf.RoundToInt((p.z-MinZ)/Step),0,NZ-1);
 void Update(){
  Walking=false;if(!viewer||guide.Welcoming||!Application.isFocused&&!Application.isBatchMode)return;
  var user=viewer.position;user.y=0;var here=transform.position;here.y=0;
  // A visitor can teleport between rooms. Catch up outside their forward view, never slide through walls.
  bool teleported=Vector3.Distance(lastViewer,user)>4;lastViewer=user;
  if((teleported&&Vector3.Distance(here,user)>7)||Room(here)!=Room(user)){var back=Vector3.ProjectOnPlane(-viewer.forward,Vector3.up).normalized;if(back.sqrMagnitude<.1f)back=Vector3.back;for(int i=0;i<12;i++){var candidate=user+Quaternion.Euler(0,(i%2==0?1:-1)*(i/2)*20,0)*back*1.8f;if(Room(candidate)==Room(user)&&Stand(candidate)&&Clear(candidate,user)){transform.position=candidate;ResetPath();break;}}}
  here=transform.position;here.y=0;float distance=Vector3.Distance(here,user);
  if(distance<2.1f){path.Clear();waypoint=0;return;}
  if(distance>3.1f&&Time.unscaledTime>nextPlan&&planning==null){nextPlan=Time.unscaledTime+1.5f;var side=Vector3.ProjectOnPlane(viewer.right,Vector3.up).normalized;var back=Vector3.ProjectOnPlane(-viewer.forward,Vector3.up).normalized;var choices=new[]{user+side*1.6f+back*1.2f,user-side*1.6f+back*1.2f,user+side*1.6f-back*1.2f,user-side*1.6f-back*1.2f,user+back*2,user-back*2,user+side*2,user-side*2};foreach(var goal in choices)if(Room(goal)==Room(user)&&Stand(goal)){planning=StartCoroutine(Plan(here,goal));break;}}
  if(waypoint>=path.Count)return;var target=path[waypoint];if(Vector3.Distance(here,target)<.09f){waypoint++;return;}if(!Clear(here,target)){ResetPath();return;}
  transform.position=Vector3.MoveTowards(here,target,Time.unscaledDeltaTime*.95f);Walking=true;
 }
 IEnumerator Plan(Vector3 from,Vector3 goal){
  // Yield before planning so the coroutine handle is assigned even for a direct path.
  yield return null;
  if(!Stand(goal)){planning=null;yield break;}
  if(Clear(from,goal)){path.Clear();path.Add(goal);waypoint=0;planning=null;yield break;}
  int start=Cell(from),end=Cell(goal);if(!Stand(Point(start))||!Clear(from,Point(start))){planning=null;yield break;}
  var open=new List<int>{start};var cost=new Dictionary<int,float>{{start,0}};var previous=new Dictionary<int,int>();var closed=new HashSet<int>();int expansions=0;
  while(open.Count>0&&closed.Count<NX*NZ){int best=0;float value=float.MaxValue;for(int j=0;j<open.Count;j++){int id=open[j];float f=cost[id]+Vector3.Distance(Point(id),Point(end));if(f<value){value=f;best=j;}}int current=open[best];open.RemoveAt(best);if(!closed.Add(current))continue;
   if(current==end){var reverse=new List<Vector3>{goal};int at=end;while(at!=start){reverse.Add(Point(at));at=previous[at];}reverse.Add(Point(start));reverse.Reverse();path.Clear();path.AddRange(reverse);waypoint=0;planning=null;yield break;}
   int x=current%NX,z=current/NX;for(int dz=-1;dz<=1;dz++)for(int dx=-1;dx<=1;dx++){if(dx==0&&dz==0||x+dx<0||x+dx>=NX||z+dz<0||z+dz>=NZ)continue;int id=current+dx+dz*NX;if(closed.Contains(id)||!Stand(Point(id))||!Clear(Point(current),Point(id)))continue;float next=cost[current]+(dx!=0&&dz!=0?.707107f:Step);if(cost.TryGetValue(id,out float old)&&next>=old)continue;cost[id]=next;previous[id]=current;open.Add(id);}
   if(++expansions%80==0)yield return null;
  }planning=null;
 }
}
