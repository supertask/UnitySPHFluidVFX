#ifndef CONSTANT_INCLUDED
#define CONSTANT_INCLUDED

//#define Gathering
#define LScattering
//#define LFScattering

#define PI 3.14159265359
#define GOLDEN_RATIO 1.61803398874989484820459
#define SQ2 1.41421356237309504880169 //Square Root of Two
#define EPSILON 0.0001

#define TYPE__INACTIVE 0
#define TYPE__ELASTIC 1
#define TYPE__SNOW 2
#define TYPE__FLUID 3

#define DEFAULT_MASS 1
#define DEFAULT_VOLUME 1

// For InterlockedAdd with float value
#define F2I_DIGIT 1024
#define F2I_DIGIT_2SQRT F2I_DIGIT * F2I_DIGIT 

#define FLOAT_TO_INT_DIGIT_1 100000
#define INT_TO_FLOAT_DIGIT_1 (1.0 / (float)FLOAT_TO_INT_DIGIT_1)

static const float4x4 Identity =
{
	{ 1, 0, 0, 0 },
	{ 0, 1, 0, 0 },
	{ 0, 0, 1, 0 },
	{ 0, 0, 0, 1 }
}; 

static const float3x3 Identity3x3 =
{
	{ 1, 0, 0},
	{ 0, 1, 0},
	{ 0, 0, 1},
};
static const float3x3 Identity3x32D =
{
	{ 1, 0, 0},
	{ 0, 1, 0},
	{ 0, 0, 0},
};
static const float2x2 Identity2x2 =
{
	{ 1, 0},
	{ 0, 1},
};

#endif