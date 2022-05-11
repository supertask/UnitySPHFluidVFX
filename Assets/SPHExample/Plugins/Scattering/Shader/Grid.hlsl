#ifndef GRID_INCLUDED
#define GRID_INCLUDED


float3 _CellStartPos;
uint _GridResolutionWidth;
uint _GridResolutionHeight;
uint _GridResolutionDepth;
float _GridSpacingH;


inline uint3 ParticlePositionToCellIndex3D(float3 pos)
{
	// return uint3(pos-_CellStartPos); // by Yuan
	 return uint3( (pos-_CellStartPos) / _GridSpacingH ); // by Tasuku
}

inline uint CellIndex3DTo1D(uint3 idx)
{
	return idx.x + idx.y * _GridResolutionWidth + idx.z * _GridResolutionWidth * _GridResolutionHeight;
}

inline uint3 CellIndex1DTo3D(uint idx)
{
	uint z = idx/(_GridResolutionWidth * _GridResolutionHeight);
	uint xy = idx%(_GridResolutionWidth * _GridResolutionHeight);

	return uint3(xy%_GridResolutionWidth, xy/_GridResolutionWidth, z);
}


// 動かない原因はここっぽい
inline float3 CellIndex3DToPositionWS(uint3 idx)
{
	//return _CellStartPos + (idx + 0.5f) * ; //by Yuan
	float halfH = _GridSpacingH / 2;
	return _CellStartPos + (idx + halfH) * _GridSpacingH;
	//return _CellStartPos + (idx + 0.5);
}

inline bool InGrid(uint3 idx)
{
	uint cdid = CellIndex3DTo1D(idx);
	return 0<= cdid && cdid < _GridResolutionWidth * _GridResolutionHeight *_GridResolutionDepth;
}


#endif