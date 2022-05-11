Shader "Hidden/Scattering"
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
			//"Queue" = "Transparent"
			"Queue" = "Overlay"
			"RenderType" = "Transparent"
		}

        // No culling or depth
		Cull Off ZWrite Off ZTest Always

		//col.xyz * col.w + backCol.xyz * (1 - col.w)
		Blend SrcAlpha OneMinusSrcAlpha

        //GrabPass {}

        Pass
        {
            CGPROGRAM

            #define UNIT_RP__BUILT_IN_RP
			#include "UnityCG.cginc"
			#include "./MpmRayMarchingCore.hlsl" 


			#pragma target 5.0
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

            V2FObjectSpace vert (VertexInput v) {
                V2FObjectSpace OUT;
                OUT.positionCS = UnityObjectToClipPos(v.positionOS);
                OUT.positionWS = mul(unity_ObjectToWorld, v.positionOS).xyz; //world space position

				//https://gamedev.stackexchange.com/questions/129139/how-do-i-calculate-uv-space-from-world-space-in-the-fragment-shader
				OUT.screenPos = OUT.positionCS.xyw;
				// Correct flip when rendering with a flipped projection matrix.
				// (I've observed this differing between the Unity scene & game views)
				OUT.screenPos.y *= _ProjectionParams.x; //For multi-platform like VR
                float2 uv = (OUT.screenPos.xy / OUT.screenPos.z) * 0.5f + 0.5f;

                OUT.uv = uv;

                // Camera space matches OpenGL convention where cam forward is -z. In unity forward is positive z.
                // (https://docs.unity3d.com/ScriptReference/Camera-cameraToWorldMatrix.html)
                float3 viewVector = mul(unity_CameraInvProjection, float4(uv * 2 - 1, 0, -1));
                OUT.viewVector = mul(unity_CameraToWorld, float4(viewVector,0));

                return OUT;
            }
            
            float4 frag (V2FObjectSpace IN) : SV_Target
            {
                //float2 screenUV = (IN.screenPos.xy / IN.screenPos.z) * 0.5f + 0.5f;

                //return float4(IN.uv, 0, 1);
                //fixed4 col = tex2D(_MainTex, IN.uv);
                //return col;

				#if defined(UNIT_RP__BUILT_IN_RP)
					float3 mainLightPosition = _WorldSpaceLightPos0;
					float3 mainLightColor = _LightColor0;
					float3 mainCameraPos = _WorldSpaceCameraPos; 
				#elif defined(UNIT_RP__HDRP)
					DirectionalLightData light = _DirectionalLightDatas[0];
					float3 mainLightPosition = -light.forward.xyz;
					float3 mainLightColor = light.color;
					float3 mainCameraPos = _WorldSpaceCameraPos; 
				#elif defined(UNIT_RP__URP)
					float3 mainLightPosition = _MainLightPosition;
					float3 mainLightColor = _MainLightColor;
					float3 mainCameraPos = _WorldSpaceCameraPos; 
				#endif

				return volumetricRayMarching(
					IN.positionWS,
					IN.uv,
                    //screenUV,
                    IN.viewVector,
					mainCameraPos,
					mainLightPosition,
					mainLightColor
				);
            }

            ENDCG
        }
    }
}
