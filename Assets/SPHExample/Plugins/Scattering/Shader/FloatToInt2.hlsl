//
//
//
// example:
//     v = 10.25609895412
//     (int)10.25609890412 * 10000 -> 102560.989541 -> 102560
//     (int)0.989041 * 10000 -> 9895.41 -> 9895
//
//     102560 / 10000 -> 10.256
//     9895 / 100000000 -> 0.00009895
//     10.256 + 0.00009895 -> 10.25609895
//     
// BTW max value of int: 2147483647
//
int2 ConvertFloatToInt2(float v)
{
	float f1 = v * F2I_DIGIT;
    int i1 = (int)f1;
	float f2 = (f1 - i1) * F2I_DIGIT; 
    int i2 = (int)f2;
	return int2( i1, i2 );
}

float ConvertInt2ToFloat(int2 v) {
	float f1 = ((float)v.x) / F2I_DIGIT;
	float f2 = ((float)v.y) / (F2I_DIGIT * F2I_DIGIT);
	return f1 + f2;
}

float3 ConvertInt3x2ToFloat3(int3 v1, int3 v2) {
	return float3(
		ConvertInt2ToFloat(int2(v1.x, v2.x)),
		ConvertInt2ToFloat(int2(v1.y, v2.y)),
		ConvertInt2ToFloat(int2(v1.z, v2.z))
	);
}

