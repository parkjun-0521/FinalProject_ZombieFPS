Shader "lwsoft/phantom" 
{
   Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        [perrenderdata]_MainTex ("Base (RGB)", 2D) = "white" {}
        _PhantomColor ("Phantom Color", Color) = (1,1,1,1)
        _PhantomPower ("Phantom Power", Float) = 1
        
    }
   
    SubShader
    {
        //Pass: character 
        stencil
        {
          ref 20
          comp Always
          
          pass replace
          
        }
        Tags { "Queue" = "Geometry+1" "RenderType" = "Opaque" }
 
        CGPROGRAM
        #pragma surface surf BlinnPhong 
        
       
        uniform float4 _Color;
        uniform float4 _Indicator;
        uniform sampler2D _MainTex;
         
        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
        };
       
        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Albedo = tex2D ( _MainTex, IN.uv_MainTex).rgb * _Color;
        }
        ENDCG
 
        // Pass: Phantom
        stencil
        {
          ref 20
          comp notequal
          
          pass replace
          zfail incrsat
        }
 
      ZWrite On
      ZTest Always
      
      Blend SrcAlpha OneMinusSrcAlpha
     
      CGPROGRAM
      #include "UnityCG.cginc"
      #pragma surface surf BlinnPhong
      uniform float4 _Color;
      uniform fixed  _PhantomPower;
      uniform fixed4 _PhantomColor;
      uniform sampler2D _MainTex;
      
      
      struct Input
      {
          float2 uv_MainTex;
          float3 viewDir;
          float3 worldNormal;
      };
     
      void surf (Input IN, inout SurfaceOutput o)
      {
            
            half rim = 1.0 - saturate(dot (normalize(IN.viewDir), IN.worldNormal ));
              fixed3 rim_final = _PhantomColor.rgb * pow (rim, _PhantomPower);
         
              o.Emission = rim_final.rgb ; 
              o.Alpha = rim * _PhantomColor.a;
      }
      ENDCG  
 
 
       
    }
 
 
   
    Fallback "Diffuse", 0
 
}
