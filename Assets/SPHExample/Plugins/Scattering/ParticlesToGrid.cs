using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

using UnityEngine;

using ComputeShaderUtil;

namespace FluidSPH.Scattering
{
    public struct Cell
    {
        public float mass; //4 byte

        public override string ToString() {
            return $"MassCell(mass={mass})";
        }
    };

    public class ParticlesToGrid : MonoBehaviour
    {
        //
        // https://github.com/supertask/Unity-MLS-MPM-Fluid-Test/blob/67b95f5dc0cf7f6eefe8756b2166e6ca1d404669/Assets/MLS-MPM-Core/Shaders/Grid.hlsl#L86-L90
        // https://github.com/supertask/Unity-MLS-MPM-Fluid-Test/blob/3fdc0d185f5a6f05110f46f60b9531a9bff4a09f/Assets/MLS-MPM-Core/Scripts/GpuMpmParticleSystem.cs
        //
        [SerializeField] public SPHController sphController;
        [SerializeField] public ComputeShader particlesToGridCS;

        [SerializeField] public float gridSpacingH = 0.5f; //0.5m
        [SerializeField] public int gridWidth = 80, gridHeight = 80, gridDepth = 80;
        
        private int numOfCells;
        private Kernel particlesToGridKernel;
        private GraphicsBuffer gridBuffer;

        public static class ShaderID
        {
            public static int GridSpacingH = Shader.PropertyToID("_GridSpacingH");
            public static int GridResolutionWidth = Shader.PropertyToID("_GridResolutionWidth");
            public static int GridResolutionHeight = Shader.PropertyToID("_GridResolutionHeight");
            public static int GridResolutionDepth = Shader.PropertyToID("_GridResolutionDepth");
            public static int CellStartPos = Shader.PropertyToID("_CellStartPos"); // I Dont know the meaning
            //public static int ParticleType = Shader.PropertyToID("_ParticleType");

            public static int GridBufferWrite = Shader.PropertyToID("_GridBufferWrite");
            public static int ParticlesBufferRead = Shader.PropertyToID("_ParticlesBufferRead");
        }

        void OnEnable()
        {
            this.numOfCells = this.gridWidth * this.gridHeight * this.gridDepth;
            
            this.gridBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, this.numOfCells, Marshal.SizeOf(typeof(Cell)));
            this.gridBuffer.SetData(Enumerable.Range(0, this.numOfCells)
                .Select(_ => new Cell()).ToArray());
                
            this.particlesToGridKernel = new Kernel(this.particlesToGridCS, "P2GScattering");

        }

        void Update()
        {
            this.ComputeParticlesToGrid();
        }
        
        void ComputeParticlesToGrid()
        {
            this.particlesToGridCS.SetBuffer(this.particlesToGridKernel.Index, ShaderID.ParticlesBufferRead, this.sphController.SphData.particleBuffer.Data);
            this.particlesToGridCS.SetBuffer(this.particlesToGridKernel.Index, ShaderID.GridBufferWrite, this.gridBuffer);
            this.particlesToGridCS.Dispatch(this.particlesToGridKernel.Index,
                Mathf.CeilToInt(sphController.Configure.D.numOfParticle / (float)this.particlesToGridKernel.ThreadX),
                (int)this.particlesToGridKernel.ThreadY,
                (int)this.particlesToGridKernel.ThreadZ);
        }
    }
}