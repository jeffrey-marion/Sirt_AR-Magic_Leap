Shader "Unlit/360Image"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color ("Main Color", Color) = (1,1,1,1)
		[Enum(Equal,3,NotEqual,6)] _StencilTest ("Stencil Test", int) = 6
    }
    SubShader
    {
       Tags { "RenderType"="Opaque" }
		LOD 100

		//CHANGE HERE
		Cull Front

		Stencil{
		Ref 1
		Comp[_StencilTest]
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//CHANGE HERE (Flip image)
				float2 uv = float2(1. - i.uv.x,i.uv.y);
				// sample the texture
				fixed4 col = tex2D(_MainTex, uv);

				//CHANGE ENDS

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
