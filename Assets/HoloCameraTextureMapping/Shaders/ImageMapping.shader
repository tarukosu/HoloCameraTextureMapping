Shader "Custom/ImageMapping" {
	Properties {
        _TextureArray("TextureArray", 2DArray) = "" {}
	}
	SubShader{
	    Tags { "RenderType" = "Opaque" }
//		LOD 200
		Pass
		{
			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers d3d11 gles
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
//#pragma exclude_renderers d3d11 gles
			#pragma vertex vert
			#pragma fragment frag

//			#pragma target 3.5
			#include "UnityCG.cginc"

			float2 _UVArray[];
	//float4x4 _MyObjectToWorld;
	//float4x4 _WorldToCameraMatrixArray[MAX_SIZE];
	//float4x4 _CameraProjectionMatrixArray[MAX_SIZE];
	//float4 _vertexInProjectionSpaceArray[MAX_SIZE];

	//float4 vertexPositionInCameraSpaceArray[MAX_SIZE];

	        struct v2f {
				float4 pos : SV_POSITION;
				float4 uv : TEXCOORD0;
			};

			v2f vert(vuint vertexID : SV_VertexID)
			{
				v2f o;
				o.uv = _UVArray[vertexID];
				o.pos = float4(0, 0, 0, 0);
				return o;
			}
			UNITY_DECLARE_TEX2DARRAY(_TextureArray);

			fixed4 frag(v2f i) : SV_Target
	    	{
				float3 uvz = float3(i.u, i.v, 0);
				return float4(1,1,1,1);
				//return UNITY_SAMPLE_TEX2DARRAY(_TextureArray, uvz);
    		}
			ENDCG
        }
	}
}
