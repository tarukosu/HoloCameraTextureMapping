Shader "Custom/ImageMapping" {
	Properties {
_TextureArray("TextureArray", 2DArray) = "" {}
	}
		SubShader{
            Tags { "RenderType" = "Opaque" }
			Cull Off ZWrite Off ZTest Always

			//		LOD 200
					Pass
					{
						CGPROGRAM
#pragma vertex vert
									#pragma fragment frag

						//			#pragma target 3.5
									#include "UnityCG.cginc"
#pragma target 5.0
uniform StructuredBuffer<float2> _UVArray;

	        //float4x4 _MyObjectToWorld;
            //float4x4 _WorldToCameraMatrixArray[MAX_SIZE];
 	//float4x4 _CameraProjectionMatrixArray[MAX_SIZE];
	//float4 _vertexInProjectionSpaceArray[MAX_SIZE];

	//float4 vertexPositionInCameraSpaceArray[MAX_SIZE];
	         struct appdata
			 {
				 float4 vertex : SV_POSITION;
				 //float4 pos : POSITION;
				 uint id : SV_VertexID;
			 };

	        struct v2f {
				//float4 pos : SV_POSITION;
				float4 vertex : POSITION;
				//float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			

			v2f vert(appdata v)
			{
				v2f o;
				o.uv = _UVArray[v.id];
//o.uv = float2(0.5, 0.5);
				//o.pos = float4(0, 0, 0, 0);
				//o.pos = v.pos;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			UNITY_DECLARE_TEX2DARRAY(_TextureArray);

			fixed4 frag(v2f i) : SV_Target
	    	{
                float3 uvz = float3(i.uv.x, i.uv.y, 10);
//float3 uvz = float3(0.2, 0.9, 0);
				//return float4(1, 1, 1, 1);
				return UNITY_SAMPLE_TEX2DARRAY(_TextureArray, uvz);
    		}
			ENDCG
        }
	}
}
