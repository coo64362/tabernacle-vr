Shader "Tabernacle/OilFlame" {
 Properties {_MainTex ("Unused compatibility texture",2D)="white"{} _Color ("Color",Color)=(1,1,1,1)}
 SubShader {Tags {"RenderType"="Opaque"} Pass {
  CGPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  #pragma multi_compile_instancing
  #include "UnityCG.cginc"
  struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
  struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_OUTPUT_STEREO};
  v2f vert(appdata v){v2f o;UNITY_SETUP_INSTANCE_ID(v);UNITY_INITIALIZE_OUTPUT(v2f,o);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);v.vertex.x+=sin(_Time.y*4+v.vertex.x*23)*.003*v.uv.y;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
  fixed4 frag(v2f i):SV_Target{UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);half heat=pow(saturate(sin(i.uv.y*3.14159)),3);return fixed4(lerp(half3(1,.28,.02),half3(1,.97,.70),heat),1);}
  ENDCG
 }}
}
