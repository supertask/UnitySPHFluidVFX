using System;
using UnityEngine;
using Unity.Mathematics;

using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;


namespace FluidSPH.Scattering
{
    [System.Serializable, VolumeComponentMenu("Post-processing/SPHFluid/Scattering")]
    public class Scattering : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        ParticlesToGrid _particlesToGrid;
        Material _material;


        public const string HEADER_DECORATION = " --- ";

		[Header (HEADER_DECORATION + "System" + HEADER_DECORATION)]
		
		public ClampedFloatParameter block = new ClampedFloatParameter(0, 0, 1);










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
		public bool isDebugScattering = false;



        public bool IsActive() => _material != null &&
            (block.value > 0);

        public override CustomPostProcessInjectionPoint injectionPoint =>
            CustomPostProcessInjectionPoint.AfterPostProcess;

        public override void Setup()
        {
            _material = CoreUtils.CreateEngineMaterial("Hidden/SPHFluid/PostProcess/ScatteringHDRP");
			_particlesToGrid = GameObject.FindObjectOfType<ParticlesToGrid>() as ParticlesToGrid;
			Debug.LogFormat("material = {0}, _particlesToGrid = {1}", _material, _particlesToGrid);
        }

        public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle srcRT, RTHandle destRT)
        {
            //if (_material == null) return;
            
			/*
			this._material.SetFloat ("_FireIntensity", fireIntensity);
			this._material.SetFloat ("_DensityOffset", densityOffset);
            
			this._material.SetVector ("_PhaseParams", new Vector4 (forwardScattering, backScattering, baseBrightness, phaseFactor));
			this._material.SetFloat ("_RayOffsetStrength", rayOffsetStrength);

			this._material.SetFloat("_SmokeAbsorption", smokeAbsorption);
			this._material.SetFloat("_LightAbsorptionTowardSun", lightAbsorptionTowardSun);
			this._material.SetFloat("_LightAbsorptionThroughCloud", lightAbsorptionThroughCloud);
			this._material.SetFloat("_DarknessThreshold", darknessThreshold);
			//Debug.Log("_LightAbsorptionThroughCloud: " + lightAbsorptionThroughCloud);
			
			
			Bounds bounds  = this._particlesToGrid.GetGridBounds();
			this._material.SetVector("_BoundingPosition", bounds.center );
			this._material.SetVector("_BoundingScale", bounds.size );
            this._material.SetVector ("_BoundsMin", bounds.min);
			this._material.SetVector ("_BoundsMax", bounds.max);

			this._material.SetBuffer("_GridBuffer", this._particlesToGrid.GridBuffer);
			this._material.SetVector("_GridDimension", this._particlesToGrid.GetGridDimension());
			//Debug.Log("grid dimension: " + this._particlesToGrid.GetGridDimension() );

			if (this.isDebugScattering) {
				this._material.EnableKeyword("DEBUG_SCATTERING");
			} else {
				this._material.DisableKeyword("DEBUG_SCATTERING");
			}
			*/
			
			var pass = 0;
			HDUtils.DrawFullScreen(cmd, _material, destRT, null, pass);
		}

		
        public override void Cleanup()
        {
            CoreUtils.Destroy(_material);
        }
    }
}