#include "./RgbToCmyk.hlsl"
#include "Assets/MLS-MPM-Core/Shaders/MpmStruct.hlsl"
#include "Assets/MLS-MPM-Core/Shaders/Grid.hlsl"
//#include "Assets/Common/Shaders/PhotoShopMath.hlsl"

#define BUFFER_T LockMpmCell
#define PHYSICAL_QUANTITY(element) ConvertInt2ToFloat(int2(element.mass, element.mass2))
StructuredBuffer<BUFFER_T> _GridBuffer;


#define RAY_STEPS_TO_FLUID 64
#define RAY_STEPS_TO_LIGHT 6

struct Ray {
    float3 origin;
    float3 dir;
};

struct BoundingBox {
    float3 Min;
    float3 Max;
};

uniform float3 _BoundingPosition, _BoundingScale, _GridDimension;

// Marching settings

// Shape settings
float4 _PhaseParams;

// Light settings
float _LightAbsorptionTowardSun;
float _LightAbsorptionThroughCloud;
float _DarknessThreshold;
float4 _LightColor0;


// Henyey-Greenstein(散乱位相関数モデル)
// https://www.astro.umd.edu/~jph/HG_note.pdf
float hg(float a, float g) {
    float g2 = g*g;
    return (1-g2) / (4*3.1415*pow(1+g2-2*g*(a), 1.5));
}

//https://www.cps-jp.org/~mosir/pub/2013/2013-11-28/01_satou/pub-web/20131128_satou_01.pdf 
float phase(float a) {
    float blend = .5;
    float hgBlend = hg(a,_PhaseParams.x) * (1-blend) + hg(a,-_PhaseParams.y) * blend;
    return _PhaseParams.z + hgBlend*_PhaseParams.w;
}

float remap(float v, float minOld, float maxOld, float minNew, float maxNew) {
    return minNew + (v-minOld) * (maxNew - minNew) / (maxOld-minOld);
}

float2 squareUV(float2 uv) {
    float width = _ScreenParams.x;
    float height =_ScreenParams.y;
    //float minDim = min(width, height);
    float scale = 1000;
    float x = uv.x * width;
    float y = uv.y * height;
    return float2 (x/scale, y/scale);
}

float3 blendLikePaint(float3 rbgL, float3 rgbR)     {
    return CMYKtoRGB(RGBtoCMYK(rbgL) + RGBtoCMYK(rgbR));
}

float2 rayBoundsDistance(Ray ray, BoundingBox boundingBox)
{
    float3 inverseRayDir = 1.0 / ray.dir;
    float3 tbot = inverseRayDir * (boundingBox.Min-ray.origin);
    float3 ttop = inverseRayDir * (boundingBox.Max-ray.origin);
    float3 tmin = min(ttop, tbot);
    float3 tmax = max(ttop, tbot);
    float2 t = max(tmin.xx, tmin.yz);
    float distanceIntersectedToNearBounds = max(t.x, t.y);
    t = min(tmax.xx, tmax.yz);
    float distanceIntersectedToFarBounds = min(t.x, t.y);

    return float2(distanceIntersectedToNearBounds, distanceIntersectedToFarBounds);
}

//find intersection points of a ray with a box
bool intersectBox(Ray ray, BoundingBox boundingBox, out float t0, out float t1)
{
    float3 invR = 1.0 / ray.dir;
    float3 tbot = invR * (boundingBox.Min-ray.origin);
    float3 ttop = invR * (boundingBox.Max-ray.origin);
    float3 tmin = min(ttop, tbot);
    float3 tmax = max(ttop, tbot);
    float2 t = max(tmin.xx, tmin.yz);
    t0 = max(t.x, t.y);
    t = min(tmax.xx, tmax.yz);
    t1 = min(t.x, t.y);
    return t0 <= t1;
}


float SampleBilinear(StructuredBuffer<BUFFER_T> buffer, float3 uvw, float3 size)
{
    uvw = saturate(uvw);
    uvw = uvw * (size-1.0);

    int x = uvw.x;
    int y = uvw.y;
    int z = uvw.z;
    
    int X = size.x;
    int XY = size.x*size.y;
    
    float fx = uvw.x-x;
    float fy = uvw.y-y;
    float fz = uvw.z-z;
    
    int xp1 = min(_GridDimension.x-1, x+1);
    int yp1 = min(_GridDimension.y-1, y+1);
    int zp1 = min(_GridDimension.z-1, z+1);
    
    float x0 = PHYSICAL_QUANTITY(buffer[x+y*X+z*XY]) * (1.0f-fx)
        + PHYSICAL_QUANTITY(buffer[xp1+y*X+z*XY]) * fx;
    float x1 = PHYSICAL_QUANTITY(buffer[x+y*X+zp1*XY]) * (1.0f-fx)
        + PHYSICAL_QUANTITY(buffer[xp1+y*X+zp1*XY]) * fx;
    
    float x2 = PHYSICAL_QUANTITY(buffer[x+yp1*X+z*XY]) * (1.0f-fx) +
        PHYSICAL_QUANTITY(buffer[xp1+yp1*X+z*XY]) * fx;
    float x3 = PHYSICAL_QUANTITY(buffer[x+yp1*X+zp1*XY]) * (1.0f-fx) +
        PHYSICAL_QUANTITY(buffer[xp1+yp1*X+zp1*XY]) * fx;
    
    float z0 = x0 * (1.0f-fz) + x1 * fz;
    float z1 = x2 * (1.0f-fz) + x3 * fz;
    
    return z0 * (1.0f-fy) + z1 * fy;
    
}

// Convert World position to UVW position
float3 convertFromWorldPosToUVW(float3 rayWorldPos) {
    float3 rayUVWPos = (rayWorldPos - _BoundingPosition + 0.5*_BoundingScale)/_BoundingScale;
    return rayUVWPos;
}

float samplePhysicalQuantity(StructuredBuffer<BUFFER_T> buffer,
        float3 rayWorldPos, BoundingBox boundingBox, float physicalQuantityOffset) {
    float3 rayUVWPos = convertFromWorldPosToUVW(rayWorldPos);
    float sampledPhysicalQuantity = SampleBilinear(buffer, rayUVWPos, _GridDimension);
    float physicalQuantity = physicalQuantityOffset * sampledPhysicalQuantity;
    return physicalQuantity;
}

// 
// Ref. Sebastian Lague, Clouds, https://github.com/SebLague/Clouds
float lightmarch(StructuredBuffer<BUFFER_T> buffer, Ray rayTowardsLight,
        BoundingBox boundingBox, float physicalQuantityOffset) {

    float dstInsideBox = rayBoundsDistance(rayTowardsLight, boundingBox).y; //Confirmed!!!
    
    float stepSize = dstInsideBox/RAY_STEPS_TO_LIGHT;
    float totalPhysicalQuantity = 0;

    float3 lightRayPos = rayTowardsLight.origin;
    for (int step = 0; step < RAY_STEPS_TO_LIGHT; step ++) {
        lightRayPos += rayTowardsLight.dir * stepSize;
        float physicalQuantity = samplePhysicalQuantity(buffer, lightRayPos, boundingBox, physicalQuantityOffset);
        totalPhysicalQuantity += max(0, physicalQuantity * stepSize);
    }
    float transmittance = exp(-totalPhysicalQuantity * _LightAbsorptionTowardSun);
    return _DarknessThreshold + transmittance * (1-_DarknessThreshold);
}
