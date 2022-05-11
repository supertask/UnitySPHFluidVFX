using System;
using UnityEngine;
using Unity.Mathematics;

using ImageEffectUtil;


namespace FluidSPH.Scattering
{
	struct PParticle 
	{
		int uuid;
		int bpid; //boundary particle id for boundary only
		float3 pos;
		float3 vel;
		float3 w;//angular velocity
		float4 col;
		float life;
		int type;
		
		public override string ToString() => $"PParticle(uuid{uuid}position={pos}, velocity={vel}, color={col}, type={type})";
	};
	
	
	[ImageEffectAllowedInSceneView]
    public class ScatteringImageEffect : ImageEffectBase
    {
        public const string HEADER_DECORATION = " --- ";

		[Header (HEADER_DECORATION + "System" + HEADER_DECORATION)]
				
        //public GameObject mediator;
        //public GpuMpmParticleSystem mpmPS;
		
		//protected SPHGrid SPHGrid => this.sphGrid ??= this.gameObject.FindOrAddTypeInComponentsAndChildren<SPHGrid>();
		//protected SPHGrid sphGrid;
		//protected SPHConfigure Configure => this.configure ??= this.gameObject.FindOrAddTypeInComponentsAndChildren<SPHConfigure>();
		//protected SPHConfigure configure;


        [Header (HEADER_DECORATION + "Marching settings" + HEADER_DECORATION)]
		public float rayOffsetStrength = 1.0f;

		[Header (HEADER_DECORATION + "Base Shape" + HEADER_DECORATION)]
		public float densityOffset = 150;
		public float smokeAbsorption = 60.0f;

		[Header (HEADER_DECORATION + "Lighting" + HEADER_DECORATION)]
		public float lightAbsorptionTowardSun = 1.21f;
		public float lightAbsorptionThroughCloud = 0.75f;
		[Range(0, 1)] public float darknessThreshold = 0.15f;

		[Range (0, 1)] public float forwardScattering = 0.811f;
		[Range (0, 1)] public float backScattering = 0.33f;
		[Range (0, 10)] public float baseBrightness = 1.0f; //should be 1, maybe
		[Range (0, 1)] public float phaseFactor = 0.488f;

		public float fireIntensity = 1.0f;

		[Header (HEADER_DECORATION + "Debug" + HEADER_DECORATION)]
		public bool isDebugScattering;

        protected override void Start() {
            base.Start();
        }

		public void Update()
		{
		}


        protected override void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
			// Validate inputs
			if (this.imageEffectMat == null || imageEffectMat.shader != shader) {
				imageEffectMat = new Material (this.shader);
			}
			
			
			
			
			this.imageEffectMat.SetFloat ("_FireIntensity", fireIntensity);
			this.imageEffectMat.SetFloat ("_DensityOffset", densityOffset);
            
			this.imageEffectMat.SetVector ("_PhaseParams", new Vector4 (forwardScattering, backScattering, baseBrightness, phaseFactor));
			this.imageEffectMat.SetFloat ("_RayOffsetStrength", rayOffsetStrength);

			this.imageEffectMat.SetFloat("_SmokeAbsorption", smokeAbsorption);
			this.imageEffectMat.SetFloat("_LightAbsorptionTowardSun", lightAbsorptionTowardSun);
			this.imageEffectMat.SetFloat("_LightAbsorptionThroughCloud", lightAbsorptionThroughCloud);
			this.imageEffectMat.SetFloat("_DarknessThreshold", darknessThreshold);
			//Debug.Log("_LightAbsorptionThroughCloud: " + lightAbsorptionThroughCloud);
			
			//this.imageEffectMat.SetVector("_BoundingPosition", (Vector3) this.configure.D.simulationSpace.Center );
			
			//Bounds bounds  = this.mpmPS.GetGridBounds();
			//int3 gridSize = this.sphGrid.GridGPUData.gridSize;
			//float3 boundingSize = gridSize * this.sphGrid.GridGPUData.gridSpacing;
			//this.imageEffectMat.SetVector("_BoundingScale", (Vector3) boundingSize );
            //this.imageEffectMat.SetVector ("_BoundsMin", bounds.min);
			//this.imageEffectMat.SetVector ("_BoundsMax", bounds.max);

			//this.sphGrid.GridGPUData.gridBuffer
			//this.imageEffectMat.SetBuffer("_GridBuffer", this.sphGrid.GridGPUData.gridBuffer);
			//this.imageEffectMat.SetVector("_GridDimension", (Vector3) (float3) gridSize );

			//this.imageEffectMat.SetBuffer("_GridBuffer", this.mpmPS.LockGridBuffer);
			//this.imageEffectMat.SetVector("_GridDimension", this.mpmPS.GetGridDimension());
			//Debug.Log("grid dimension: " + this.mpmPS.GetGridDimension() );

			if (this.isDebugScattering) {
				this.imageEffectMat.EnableKeyword("DEBUG_SCATTERING");
			} else {
				this.imageEffectMat.DisableKeyword("DEBUG_SCATTERING");
			}


            Graphics.Blit(src, dst, this.imageEffectMat);
        }
		
		
		//
		// Debug Compute Buffer
		// When you define a struct/class,
		// please use override ToString(), public override string ToString() => $"MpmParticle(position={position}, velocity={velocity})";
		//
		// debugging range is startIndex <= x < endIndex
		// example: 
		//    Util.PrintBuffer<uint2>(this.particlesBuffer, 1024, 1027); 
		//
		public static void PrintBuffer<T>(ComputeBuffer buffer, int startIndex, int endIndex) where T  : struct
		{
			int N = endIndex - startIndex;
			T[] array = new T[N];
			buffer.GetData(array, 0, startIndex, N);
			for (int i = 0; i < N; i++)
			{
				Debug.LogFormat("index={0}: {1}", startIndex + i, array[i]);
			}
		}

    }
}