Shader "Tabernacle/Surface" {
 Properties {
  _Color ("Color", Color) = (1,1,1,1)
  _MainTex ("Surface", 2D) = "white" {}
  _Metal ("Metal sheen", Range(0,1)) = 0
  _Glow ("Glow", Range(0,1)) = 0
 }
 SubShader {
  Tags { "RenderType"="Opaque" "Queue"="Geometry" }
  Pass {
   Tags { "LightMode"="ForwardBase" }
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fog
   #pragma multi_compile_instancing
   #include "UnityCG.cginc"
   struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; float2 uv:TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
   struct v2f { float4 pos:SV_POSITION; float3 normal:TEXCOORD0; float3 world:TEXCOORD1; float2 uv:TEXCOORD2; UNITY_FOG_COORDS(3) UNITY_VERTEX_OUTPUT_STEREO };
   sampler2D _MainTex; float4 _MainTex_ST; fixed4 _Color; half _Metal, _Glow;
   v2f vert(appdata v) { v2f o; UNITY_SETUP_INSTANCE_ID(v); UNITY_INITIALIZE_OUTPUT(v2f,o); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); o.pos=UnityObjectToClipPos(v.vertex); o.normal=UnityObjectToWorldNormal(v.normal); o.world=mul(unity_ObjectToWorld,v.vertex).xyz; o.uv=TRANSFORM_TEX(v.uv,_MainTex); UNITY_TRANSFER_FOG(o,o.pos); return o; }
   fixed4 frag(v2f i):SV_Target {
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    half3 n=normalize(i.normal); half3 l=normalize(half3(-.45,.85,-.3));
    half shade=.48+.48*saturate(dot(n,l));
    half3 base=tex2D(_MainTex,i.uv).rgb*_Color.rgb;
    // A restrained mobile material: no realtime shadows, reflection probes or transparency.
    half spec=pow(saturate(dot(n,normalize(l+normalize(_WorldSpaceCameraPos-i.world)))),32)*_Metal;
    fixed4 c=fixed4(base*lerp(shade,1,_Glow)+spec*half3(.65,.48,.24),1);
    UNITY_APPLY_FOG(i.fogCoord,c); return c;
   }
   ENDCG
  }
 }
}
