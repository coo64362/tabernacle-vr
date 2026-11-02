using UnityEngine;
using UnityEngine.Rendering;

// One avatar-only 512px reflection, updated near the basin at 12 Hz. No second rendering of the courtyard.
public sealed class LaverReflection : MonoBehaviour {
 Camera viewer,reflection;RenderTexture texture;Material material;float nextFrame;const float SurfaceY=1.017f;
 public RenderTexture ReflectionTexture=>texture;
 public bool Enabled {get;set;}=true;
 public void Initialize(Camera head){
  viewer=head;viewer.cullingMask&=~(1<<11);
  var go=new GameObject("Laver avatar reflection camera");go.transform.SetParent(transform,false);reflection=go.AddComponent<Camera>();reflection.enabled=false;reflection.stereoTargetEye=StereoTargetEyeMask.None;reflection.cullingMask=1<<11;reflection.clearFlags=CameraClearFlags.SolidColor;reflection.backgroundColor=new Color(.065f,.13f,.14f);reflection.nearClipPlane=.05f;reflection.farClipPlane=9;reflection.allowHDR=false;reflection.allowMSAA=false;reflection.useOcclusionCulling=false;
  texture=new RenderTexture(512,512,16,RenderTextureFormat.ARGB32){name="Laver avatar reflection",antiAliasing=1};texture.Create();reflection.targetTexture=texture;
  material=new Material(Resources.Load<Shader>("LaverReflection"));material.SetTexture("_ReflectionTex",texture);
  var disk=CharacterGeometry.Group("Reflective laver water",transform,new Vector3(0,SurfaceY,16.5f));
  var mesh=new Mesh{name="Circular water reflection"};const int n=48;var vertices=new Vector3[n+2];var uv=new Vector2[n+2];var triangles=new int[n*3];uv[0]=new Vector2(.5f,.5f);
  for(int i=0;i<=n;i++){float a=i*Mathf.PI*2/n;vertices[i+1]=new Vector3(Mathf.Cos(a)*.568f,0,Mathf.Sin(a)*.568f);uv[i+1]=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*.5f+Vector2.one*.5f;if(i<n){triangles[i*3]=0;triangles[i*3+1]=i+2;triangles[i*3+2]=i+1;}}
  mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();disk.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;disk.gameObject.AddComponent<CharacterMeshOwner>().Mesh=mesh;disk.gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;
 }
 public void RenderIfNeeded(){
  if(!viewer||!reflection)return;
  bool nearby=Enabled&&Vector3.Distance(viewer.transform.position,new Vector3(0,1.3f,16.5f))<4;
  material.SetFloat("_HasReflection",nearby?1:0);if(!nearby||Time.unscaledTime<nextFrame)return;nextFrame=Time.unscaledTime+1f/12;
  Matrix4x4 mirror=Matrix4x4.identity;mirror.m11=-1;mirror.m13=2*SurfaceY;
  reflection.transform.position=mirror.MultiplyPoint(viewer.transform.position);reflection.transform.rotation=Quaternion.LookRotation(Vector3.Reflect(viewer.transform.forward,Vector3.up),Vector3.Reflect(viewer.transform.up,Vector3.up));
  reflection.worldToCameraMatrix=viewer.worldToCameraMatrix*mirror;reflection.projectionMatrix=viewer.projectionMatrix;
  material.SetMatrix("_ReflectionVP",GL.GetGPUProjectionMatrix(reflection.projectionMatrix,true)*reflection.worldToCameraMatrix);
  bool inverted=GL.invertCulling;try{GL.invertCulling=!inverted;reflection.Render();}finally{GL.invertCulling=inverted;}
 }
 void OnDestroy(){if(texture){texture.Release();Destroy(texture);}if(material)Destroy(material);}
}
