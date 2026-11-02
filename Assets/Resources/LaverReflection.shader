Shader "Tabernacle/Laver reflection" {
 Properties { _ReflectionTex("Avatar reflection",2D)="black"{} _HasReflection("Active",Float)=0 }
 SubShader {
  Tags { "RenderType"="Opaque" "Queue"="Geometry+10" }
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_instancing
   #include "UnityCG.cginc"
   struct appdata {float4 vertex:POSITION;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
   struct v2f {float4 pos:SV_POSITION;float4 reflection:TEXCOORD0;float2 uv:TEXCOORD1;UNITY_VERTEX_OUTPUT_STEREO};
   sampler2D _ReflectionTex;float4x4 _ReflectionVP;float _HasReflection;
   v2f vert(appdata v){v2f o;UNITY_SETUP_INSTANCE_ID(v);UNITY_INITIALIZE_OUTPUT(v2f,o);UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);o.pos=UnityObjectToClipPos(v.vertex);o.reflection=mul(_ReflectionVP,mul(unity_ObjectToWorld,v.vertex));o.uv=v.uv;return o;}
   fixed4 frag(v2f i):SV_Target{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    float2 uv=i.reflection.xy/max(.0001,i.reflection.w)*.5+.5;
    #if UNITY_UV_STARTS_AT_TOP
    uv.y=1-uv.y;
    #endif
    float valid=step(0,uv.x)*step(uv.x,1)*step(0,uv.y)*step(uv.y,1)*step(0,i.reflection.w)*_HasReflection;
    fixed3 reflection=tex2D(_ReflectionTex,saturate(uv)).rgb;
    float edge=1-smoothstep(.40,.49,length(i.uv-.5));
    return fixed4(lerp(fixed3(.055,.12,.13),reflection*.80+fixed3(.025,.05,.05),valid*edge),1);
   }
   ENDCG
  }
 }
}
