#include "./RayMarchingUtil.hlsl"

//
// Fire & smoke settings
//
//sampler2D _FireGradient;
//sampler2D _SmokeGradient;

Texture2D<float4> FireGradient;
Texture2D<float4> SmokeGradient;
SamplerState samplerFireGradient;
SamplerState samplerSmokeGradient;

float _SmokeAbsorption, _FireAbsorption; //absorption = 吸収
//float4 _SmokeColor;
//float4 _FireColor;
float _FireIntensity;


//Camera background texture
#if defined(UNIT_RP__URP)
    uniform sampler2D _CameraOpaqueTexture;
#elif defined(UNIT_RP__BUILT_IN_RP)
    uniform sampler2D _MainTex;
    //uniform sampler2D _GrabPassTexture;
#elif defined(UNIT_RP__HDRP)
    //Nothing
#endif 

sampler2D _CameraDepthTexture;

// Marching settings

// Shape settings
float _DensityOffset;
float _ReactionOffset;

// Light settings
float2 _FireTemperatureRange;
float2 _SmokeDensityRange;


float4 volumetricRayMarching(
    float3 positionWS,
    float2 screenUV,
    float3 viewVector,
    float3 mainCameraPos,
    float3 mainLightPosition,
    float3 mainLightColor) {

    //return float4(screenUV, 0, 1);

    //float nonlin_depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);
    //float depth = LinearEyeDepth(nonlin_depth)/150.0;
    //return depth;

    Ray ray;
    ray.origin = mainCameraPos;
    ray.dir = normalize(viewVector);
    //return float4(positionWS, 1);
    //return float4(ray.dir, 1);
    //return float4(normalize(viewVector), 1);
    //return distance(ray.dir, normalize(viewVector)) ;

    //return float4(mainCameraPos, 1);

    //TODO(Tasuku): 効率悪いので後で修正
    BoundingBox boundingBox;
    boundingBox.Min = float3(-0.5,-0.5,-0.5)*_BoundingScale + _BoundingPosition;
    boundingBox.Max = float3(0.5,0.5,0.5)*_BoundingScale + _BoundingPosition;
    //return float4(boundingBox.Max, 1);

    //figure out where ray from eye hit front of cube
    float2 rayToContainerInfo = rayBoundsDistance(ray, boundingBox);
    float dstToBox = rayToContainerInfo.x;
    float dstInsideBox = rayToContainerInfo.y;
    //return float4(dstToBox/51.2, dstInsideBox/51.2, 0, 1);

    //if eye is in cube then start ray at eye
    if (dstToBox < 0.0) dstToBox = 0.0;

    float3 entryPoint = ray.origin + ray.dir * dstToBox;
    float3 exitPoint = ray.origin + ray.dir * dstInsideBox;
    //return float4(entryPoint, 1);
    //return float4(exitPoint, 1);

    // Phase function makes clouds brighter around sun
    //float cosAngle = dot(ray.dir, _WorldSpaceLightPos0.xyz);
    float cosAngle = dot(ray.dir, mainLightPosition.xyz);
    float phaseVal = phase(cosAngle);

    float3 rayPos = entryPoint;
    float stepSize = distance(exitPoint, entryPoint)/float(RAY_STEPS_TO_FLUID); //This is problem of while loop
    //float stepSize = 0.05;
    //float3 ds = normalize(exitPoint-entryPoint) * stepSize;
    //float dstLimit = min(depth-dstToBox, dstInsideBox);

    float sunLightEneryOnSmoke = 0.0;
    float sunLightEnergyOnFire = 0.0;
    float fireLightEnergyOnSmoke = 0.0;
    float fireTransmittance = 1.0, smokeTransmittance = 1.0;
    float dstLimit = dstInsideBox - dstToBox;
    float dstTravelled = 0.0;
    //float dstTravelled = randomOffset;

    float normalizedSmokeTemperature = 1.0;

    //while (dstTravelled < dstLimit) {
    //for(int i=0; i < RAY_STEPS_TO_FLUID; i++, rayPos += ds) {
    for(int i=0; i < RAY_STEPS_TO_FLUID; i++, dstTravelled += stepSize) {
        rayPos = entryPoint + ray.dir * dstTravelled;

        //return float4(rayPos, 1);
        float density = samplePhysicalQuantity(_GridBuffer, rayPos, boundingBox, _DensityOffset);
        //return density;

        if (density > 0) {
            Ray rayTowardsSunLight;
            rayTowardsSunLight.origin = rayPos;
            rayTowardsSunLight.dir = mainLightPosition;

            float sunLightTransmittanceOnSmoke = lightmarch(_GridBuffer, rayTowardsSunLight, boundingBox, _DensityOffset);
            sunLightEneryOnSmoke += density * stepSize * smokeTransmittance * sunLightTransmittanceOnSmoke * phaseVal;
            smokeTransmittance *= exp(-density * stepSize * _LightAbsorptionThroughCloud);
            //if not very dense, most light makes it through
            //if very dense, not much light makes it through
        }

        // Temperature(0 ~ 1) for binding temperature with color
        // Ref. https://github.com/Scrawk/GPU-GEMS-3D-Fluid-Simulation/blob/master/Assets/FluidSim3D/Shaders/FireRayCast.shader#L154-L156
        //normalizedSmokeTemperature *= 1.0-saturate(density * stepSize * 1.0); //TODO(Tasuku): 0.5 to variable
        //if (normalizedSmokeTemperature <= 0.01) break;

    }
    //return 1 - smokeTransmittance;
    //return sunLightEneryOnSmoke;

    //Debug
    //float fireMap = 1.0-fireTransmittance;
    //return float4(fireMap, fireMap, fireMap, 1.0);
    //float smokeMap = 1.0-smokeTransmittance;
    //return float4(smokeMap, smokeMap, smokeMap, 1.0);


    //float4 fireCol = _FireColor;
    //float4 fireCol = FireGradient.SampleLevel(samplerFireGradient, float2(normalizedFireTemperature, 0), 0);

    //Multiplying emission by density(1 - smokeTransmittance) makes better fire.
    //Ref. https://youtu.be/Hy4R5Vf-dVM?t=1079
    //fireCol = fireCol * _FireIntensity * sunLightEnergyOnFire * (1 - smokeTransmittance); 

    //float4 smoke = SmokeGradient.SampleLevel(samplerSmokeGradient, float2(normalizedSmokeTemperature, 0), 0);
    //float4 smokeCol = float4(sunLightEneryOnSmoke * blendLikePaint(mainLightColor.rgb, smoke.rgb), 1.0 - smokeTransmittance);

    // Load scene color
    #if defined(UNIT_RP__BUILT_IN_RP)
        float4 sceneColor = tex2D(_MainTex, screenUV);
        //float4 sceneColor = tex2D(_GrabPassTexture, screenUV);
    #elif defined(UNIT_RP__URP)
        float4 sceneColor = tex2D(_CameraOpaqueTexture, screenUV);
    #elif defined(UNIT_RP__HDRP)
        float4 sceneColor = float4(SampleCameraColor(screenUV), 1);

    #endif

#if DEBUG_SCATTERING
    float4 debugColor = sunLightEneryOnSmoke;
    debugColor.a = 1.0 - smokeTransmittance;
    return debugColor;
    //return sunLightEneryOnSmoke;
#else
    float4 sandColor = float4(sunLightEneryOnSmoke * sceneColor.rgb, 1.0 - smokeTransmittance);
    return BlendAlpha(sceneColor, sandColor);
#endif

    //return lerp(smokeCol, sceneColor, smokeTransmittance);

}

