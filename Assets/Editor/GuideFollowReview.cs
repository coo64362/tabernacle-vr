using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
[InitializeOnLoad]
public static class GuideFollowReview {
 static Transform user;static RobotGuide guide;static int stage;static double started;static float closest;
 static GuideFollowReview(){if(SessionState.GetBool("GuideFollowReviewRunning",false))EditorApplication.update+=Tick;}
 public static void Run(){EditorSceneManager.OpenScene("Assets/TabernacleRealistic.unity");var explorer=UnityEngine.Object.FindFirstObjectByType<QuestExplorer>();if(explorer)explorer.enabled=false;SessionState.SetBool("GuideFollowReviewRunning",true);EditorApplication.EnterPlaymode();}
 static void Check(bool c,string m){if(!c)throw new Exception(m);}
 static void Tick(){if(!EditorApplication.isPlaying)return;try{
  if(stage==0){user=new GameObject("Follow review visitor").transform;user.position=new Vector3(3,1.65f,8);guide=new GameObject("Follow review guide").AddComponent<RobotGuide>();guide.Initialize(user);foreach(var r in guide.GetComponentsInChildren<Renderer>())if(r.name!="geometry_0")Debug.Log("FACE_BOUNDS "+r.name+" "+r.bounds+" local="+r.transform.localPosition);guide.transform.position=new Vector3(-3,0,8);Physics.SyncTransforms();Check(!GuideFollower.Clear(guide.transform.position,new Vector3(4.6f,0,6.8f)),"Obstacle route did not cross the altar");started=EditorApplication.timeSinceStartup;closest=10;stage=1;return;}
  Check(GuideFollower.Stand(guide.transform.position),"Guide overlapped a wall or furnishing: "+guide.transform.position);
  if(stage==1){float distance=Vector3.Distance(guide.transform.position,new Vector3(3,0,8));closest=Mathf.Min(closest,distance);if(distance>2.8f){Check(EditorApplication.timeSinceStartup-started<22,"Guide did not walk around altar; closest="+closest);return;}Debug.Log("GUIDE_FOLLOW_ALTAR_PATH_PASS");user.position=new Vector3(0,1.65f,27.5f);started=EditorApplication.timeSinceStartup;stage=2;}
  else if(stage==2&&EditorApplication.timeSinceStartup-started>.75f){Check(guide.transform.position.z>26,"Guide did not catch up into Holy Place: "+guide.transform.position);Check(Vector3.Distance(guide.transform.position,new Vector3(0,0,27.5f))<3,"Guide too far from visitor");Debug.Log("GUIDE_FOLLOW_HOLY_PLACE_PASS");user.position=new Vector3(0,1.65f,36.2f);started=EditorApplication.timeSinceStartup;stage=3;}
  else if(stage==3&&EditorApplication.timeSinceStartup-started>.75f){Check(guide.transform.position.z>35.144f,"Guide did not catch up beyond veil: "+guide.transform.position);Debug.Log("GUIDE_FOLLOW_MOST_HOLY_PASS");Directory.CreateDirectory("Documentation/Review-v0.6");File.WriteAllText("Documentation/Review-v0.6/following.txt","Courtyard path around bronze altar: pass\nNo collision overlap while walking: pass\nCatch up into Holy Place after visitor teleport: pass\nCatch up into Most Holy Place after visitor teleport: pass\nHeadset comfort and appearance: not tested\n");Debug.Log("GUIDE_FOLLOW_REVIEW_PASS");Finish(0);}
 }catch(Exception e){Debug.LogException(e);Finish(1);}}
 static void Finish(int result){SessionState.SetBool("GuideFollowReviewRunning",false);EditorApplication.update-=Tick;EditorApplication.Exit(result);}
}
