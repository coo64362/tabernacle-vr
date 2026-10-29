Shader "Tabernacle/Physical" {
 Properties {
  _Color ("Tint",Color)=(1,1,1,1)
  _MainTex ("Material atlas",2D)="white"{}
  _AtlasRect ("Atlas rectangle",Vector)=(1,1,0,0)
  _Metallic ("Metallic",Range(0,1))=0
  _Glossiness ("Smoothness",Range(0,1))=.4
  _Relief ("Surface relief",Range(0,3))=.25
  _WorldScale ("Ground world tiling",Float)=0
  _EmissionColor ("Emission",Color)=(0,0,0,0)
 }
 SubShader {
  Tags {"RenderType"="Opaque"} LOD 250
  Cull Off
  CGPROGRAM
  #pragma surface surf Standard fullforwardshadows addshadow finalcolor:FilmResponse
  #pragma target 3.0
  #pragma multi_compile_instancing
  sampler2D _MainTex; float4 _MainTex_TexelSize,_AtlasRect; fixed4 _Color,_EmissionColor;
  half _Metallic,_Glossiness,_Relief;float _WorldScale,_SceneExposure,_ReflectionCapture;
  struct Input {float2 uv_MainTex;float3 worldPos;float3 worldNormal;INTERNAL_DATA};
  void surf(Input IN,inout SurfaceOutputStandard o){
   float2 uv=_WorldScale>0?IN.worldPos.xz*_WorldScale:IN.uv_MainTex;
   float2 p=uv*_AtlasRect.xy+_AtlasRect.zw;
   fixed3 t=tex2D(_MainTex,p).rgb;half h=dot(t,half3(.3,.5,.2));
   half hx=dot(tex2D(_MainTex,p+float2(_MainTex_TexelSize.x,0)).rgb,half3(.3,.5,.2));
   half hy=dot(tex2D(_MainTex,p+float2(0,_MainTex_TexelSize.y)).rgb,half3(.3,.5,.2));
   o.Albedo=t*_Color.rgb;o.Metallic=_Metallic;
   o.Smoothness=saturate(_Glossiness+(h-.5)*.12);
   o.Normal=normalize(half3((h-hx)*_Relief,(h-hy)*_Relief,1));
   o.Occlusion=lerp(.82,1,h);o.Emission=_EmissionColor.rgb;o.Alpha=1;
  }
  void FilmResponse(Input IN,SurfaceOutputStandard o,inout fixed4 color){
   if(_Metallic>.1&&IN.worldPos.z>25&&abs(IN.worldPos.x)<3){
    half3 n=normalize(WorldNormalVector(IN,o.Normal));half3 view=normalize(_WorldSpaceCameraPos-IN.worldPos);
    float3 key=IN.worldPos.z>35?float3(-1,2.7,36.1):float3(-.6,3.6,29.5);
    half3 direction=normalize(key-IN.worldPos);half ndl=saturate(dot(n,direction));half h=saturate(dot(n,normalize(view+direction)));
    half highlight=(pow(h,36)*.7+pow(h,110)*.3)*ndl*_Metallic;
    color.rgb+=highlight*lerp(half3(1,1,1),o.Albedo,.45)*1.8;
   }
   if(_ReflectionCapture<.5){half3 radiance=max(0,color.rgb)*max(1,_SceneExposure);color.rgb=radiance/(1+radiance);}
  }
  ENDCG
 }
 FallBack "Diffuse"
}
