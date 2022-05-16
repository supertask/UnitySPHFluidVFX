Shader "Hidden/SPHFluid/PostProcess/ScatteringHDRP"
{
    
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    { 
        // No culling or depth
        //Cull Off ZWrite Off ZTest Always

		Tags {
			//"Queue" = "Overlay"
			//"RenderType" = "Transparent"
		}
        

        // No culling or depth
		Cull Off ZWrite Off ZTest Always

		//col.xyz * col.w + backCol.xyz * (1 - col.w)
		//Blend SrcAlpha OneMinusSrcAlpha

        //GrabPass {}

        Pass
        {
            HLSLINCLUDE

            //#define UNIT_RP__BUILT_IN_RP
			//#include "UnityCG.cginc"
            #define UNIT_RP__HDRP

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			//#include "./FluidRayMarchingCore.hlsl" 

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile __ DEBUG_SCATTERING

            /*
            // vertex input: position, UV
            struct VertexInput {
                float4 positionOS: POSITION;
                float2 uv : TEXCOORD0;
            };

            struct V2FObjectSpace {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewVector : TEXCOORD1;
            };
            */

            struct VertexInput
            {
                float4 positionOS : POSITION; //vertex position in object space
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct V2FScreenSpace
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 viewVector : TEXCOORD2;
            };

            /*
            struct V2FObjectSpace
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 screenPos : TEXCOORD1;
                float3 viewVector : TEXCOORD2;
                float2 uv : TEXCOORD3;
            };
            */

            V2FScreenSpace vert (VertexInput v) {
                /*
                OUT.positionWS = mul(unity_ObjectToWorld, v.positionOS).xyz; //world space position
                
                // Camera space matches OpenGL convention where cam forward is -z. In unity forward is positive z.
                // (https://docs.unity3d.com/ScriptReference/Camera-cameraToWorldMatrix.html)
                float3 viewVector = mul(unity_CameraInvProjection, float4(uv * 2 - 1, 0, -1));
                OUT.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));
                */
                
                V2FScreenSpace output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                //output.positionWS = GetFullScreenTriangleVertexPosition(input.vertexID);
                
                float3 _Camera_Direction = -1 * mul(UNITY_MATRIX_M, transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V)) [2].xyz);
                output.viewVector = _Camera_Direction;

                output.uv = GetFullScreenTriangleTexCoord(input.vertexID);

                return OUT;
            }
            
            float4 frag (V2FScreenSpace IN) : SV_Target
            {
                return 1;
                //return float4(IN.uv, 0, 1);
                //fixed4 col = tex2D(_MainTex, IN.uv);
                //return col;

                /*
                //https://forum.unity.com/threads/hdrp-getting-shadow-attenuation.863620/
                DirectionalLightData light = _DirectionalLightDatas[0];
                float3 mainLightPosition = -light.forward.xyz;
                float3 mainLightColor = light.color;
                float3 mainCameraPos = _WorldSpaceCameraPos;
                */
                                
                /*
				return volumetricRayMarching(
					IN.positionWS,
					IN.uv,
                    //screenUV,
                    IN.viewVector,
					mainCameraPos,
					mainLightPosition,
					mainLightColor
				);
                */
            }

            ENDHLSL
        }
    }
}
