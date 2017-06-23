Shader "Custom/ImageMapping" {
	Properties {
        _TextureArray("TextureArray", 2DArray) = "" {}
	}
	SubShader{
        Tags { "RenderType" = "Transparent" }
        Cull Off
		//ZWrite Off
		//LOD 200
        Pass
        {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom

			#include "UnityCG.cginc"
			#pragma target 5.0
			uniform StructuredBuffer<float2> _UVArray;
			uniform StructuredBuffer<int> _TextureIndexArray;
			int _TextureCount;

	        struct appdata
			{
				 float4 vertex : POSITION;
				 uint id : SV_VertexID;
			};

	        struct v2f {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};

			struct g2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.uv = _UVArray[v.id];
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv2.x = _TextureIndexArray[v.id];
				o.uv2.y = v.id;
				return o;
			}
			UNITY_DECLARE_TEX2DARRAY(_TextureArray);

			[maxvertexcount(9)]
			void geom(triangle v2f IN[3], inout TriangleStream<g2f> tristream)
			{
				g2f o;

				for (int mainPoint = 0; mainPoint < 3; ++mainPoint) {
					float valid = 1;

					for (int cnt = 0; cnt < 3; ++cnt)
					{
						uint vertexId = floor(IN[cnt].uv2.y);
						uint textureId = floor(IN[mainPoint].uv2.x);
						float2 uv = _UVArray[_TextureCount * vertexId + textureId];

						if (uv.x < -0.5 || uv.y < -0.5) {
							valid = -1;
						}
					}

					for (int cnt = 0; cnt < 3; ++cnt)
					{
						uint vertexId = floor(IN[cnt].uv2.y);
						uint textureId = floor(IN[mainPoint].uv2.x);
						o.pos = IN[cnt].vertex;
						o.uv = _UVArray[_TextureCount * vertexId + textureId];
						o.uv2 = IN[mainPoint].uv2;

						if (valid > 0) {
							o.uv2.x = floor(IN[mainPoint].uv2.x) + 0.00001;
						}
						else {
							o.uv2.x = 0;
						}
						tristream.Append(o);
					}
					tristream.RestartStrip();
				}
				
			}

			fixed4 frag(g2f i) : SV_Target
	    	{
                fixed4 color = fixed4(0, 0, 0, 0);
				//fixed4 color = fixed4(1, 1, 1, 1);
				//float3 uvz = float3(i.uv.x, i.uv.y, i.uv2.x);

				/*
			    if (frac(i.uv2.x) > 0.001) {
					return color;
				}
				*/
			    if (i.uv.x < 0 || i.uv.x > 1 || i.uv.y < 0 || i.uv.y > 1) {
				    return color;
				}
				uint index = floor(i.uv2.x);
                float3 uvz = float3(i.uv.x, i.uv.y, index);
                if (!any(saturate(i.uv) - i.uv)) {
					color = UNITY_SAMPLE_TEX2DARRAY(_TextureArray, uvz);
				}
				//color = UNITY_SAMPLE_TEX2DARRAY(_TextureArray, uvz);
				return color;
    		}
			ENDCG
        }
	}
}
