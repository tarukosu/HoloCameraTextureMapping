Shader "Custom/ImageMapping" {
	Properties {
        _TextureArray("TextureArray", 2DArray) = "" {}
	}
	SubShader{
        Tags { "RenderType" = "Transparent" }
        Cull Off
		//ZWrite Off

			//		LOD 200
        Pass
        {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma geometry geom

		 //			#pragma target 3.5
        #include "UnityCG.cginc"
        #pragma target 5.0
		uniform StructuredBuffer<float2> _UVArray;
     	uniform StructuredBuffer<int> _TextureIndexArray;

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
				float2 uv2 : TEXCOORD1;
				//int textureIndex :
				//uint id : SV_VertexID;
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
                //o.uv = float2(0.5, 0.5);
				//o.pos = float4(0, 0, 0, 0);
				//o.pos = v.pos;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//o.textureIndex = _TextureIndexArray[v.id];
				//o.id = _TextureIndexArray[v.id];
				o.uv2.x = _TextureIndexArray[v.id];
                //o.uv2.x = v.id;
				return o;
			}
			UNITY_DECLARE_TEX2DARRAY(_TextureArray);

			[maxvertexcount(9)]
			void geom(triangle v2f IN[3], inout TriangleStream<g2f> tristream)
			{
				g2f o;

				// それ自身のポリゴンを登録
				for (int cnt = 0; cnt<3; ++cnt)
				{
					o.pos = IN[cnt].vertex;
					o.uv = IN[cnt].uv;
					o.uv2 = IN[0].uv2;
					//output.Pos = mul(input[cnt].Pos, View);
					//output.Pos = mul(output.Pos, Projection);
					//output.Normal = input[cnt].Normal.xyz;

					//output.Color = input[cnt].Color;

					// 頂点追加
					tristream.Append(o);
				}

				tristream.RestartStrip();
				for (int cnt = 0; cnt<3; ++cnt)
				{
					o.pos = IN[cnt].vertex;
					o.uv = IN[cnt].uv;
					o.uv2 = IN[1].uv2;
					tristream.Append(o);
				}

				tristream.RestartStrip();

				for (int cnt = 0; cnt<3; ++cnt)
				{
					o.pos = IN[cnt].vertex;
					o.uv = IN[cnt].uv;
					o.uv2 = IN[2].uv2;
					tristream.Append(o);
				}

				tristream.RestartStrip();

			}

			fixed4 frag(g2f i) : SV_Target
	    	{
                fixed4 color = fixed4(0, 0, 0, 0);
				//float3 uvz = float3(i.uv.x, i.uv.y, i.uv2.x);
				if (frac(i.uv2.x)) {
					return color;
				}
				uint index = floor(i.uv2.x);
                float3 uvz = float3(i.uv.x, i.uv.y, index);
                //float3 uvz = float3(0.2, 0.9, 0);
				//return float4(1, 1, 1, 1);
//				fixed4 color = fixed4(0, 0, 0, 1);
				if (!any(saturate(i.uv) - i.uv)) {
					color = UNITY_SAMPLE_TEX2DARRAY(_TextureArray, uvz);
				}
				return color;
    		}
			ENDCG
        }
	}
}
