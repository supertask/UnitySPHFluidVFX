using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

using UnityEngine;
using UnityEngine.VFX;

using Unity.Mathematics;


using ComputeShaderUtil;

namespace FluidSPH.Scattering
{
    public struct Cell
    {
        public int mass; //4 byte
        public float3 position;
        public uint3 cellIndex3D;
        public int cellIndex;

        public string GetFloat3(float3 v) {
            return $"(x = {v.x}, y = {v.y}, z = {v.z})";
        }

        public override string ToString()
        {
            return $"MassCell(mass = {mass}, position = {GetFloat3(position)}, cellIndex3D = {cellIndex3D}, cellIndex = {cellIndex})";
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
        [SerializeField] public int gridWidth = 20, gridHeight = 40, gridDepth = 100;
        
        private int numOfCells;
        private Kernel resetGridKernel, particlesToGridKernel;
        private GraphicsBuffer gridBuffer, gridMassBuffer, gridPositionBuffer;
        public GraphicsBuffer GridBuffer => gridBuffer;

        public static class ShaderID
        {
            public static int GridSpacingH = Shader.PropertyToID("_GridSpacingH");
            public static int GridResolutionWidth = Shader.PropertyToID("_GridResolutionWidth");
            public static int GridResolutionHeight = Shader.PropertyToID("_GridResolutionHeight");
            public static int GridResolutionDepth = Shader.PropertyToID("_GridResolutionDepth");
            public static int CellStartPos = Shader.PropertyToID("_CellStartPos"); // I Dont know the meaning
            //public static int ParticleType = Shader.PropertyToID("_ParticleType");

            public static int NumOfParticles = Shader.PropertyToID("_NumOfParticles");

            public static int ParticlesBufferRead = Shader.PropertyToID("_ParticlesBufferRead");
            
            public static int GridBufferWrite = Shader.PropertyToID("_GridBufferWrite");
            public static int GridMassBufferWrite = Shader.PropertyToID("_GridMassBufferWrite");
            public static int GridPositionBufferWrite = Shader.PropertyToID("_GridPositionBufferWrite");

            
        }

        void OnEnable()
        {
            this.numOfCells = this.gridWidth * this.gridHeight * this.gridDepth;
            
            this.gridBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, this.numOfCells, Marshal.SizeOf(typeof(Cell)));
            this.gridBuffer.SetData(Enumerable.Range(0, this.numOfCells)
                .Select(_ => new Cell()).ToArray());
                
            this.gridMassBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, this.numOfCells, Marshal.SizeOf(typeof(int)));
            this.gridPositionBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, this.numOfCells, Marshal.SizeOf(typeof(float3)));

            this.GetComponent<VisualEffect>().SetInt("NumOfCells", this.numOfCells);
            this.GetComponent<VisualEffect>().SetGraphicsBuffer("GridMassBuffer", gridMassBuffer);
            this.GetComponent<VisualEffect>().SetGraphicsBuffer("GridPositionBuffer", gridPositionBuffer);
            
            this.particlesToGridKernel = new Kernel(this.particlesToGridCS, "P2GScattering");
            this.resetGridKernel = new Kernel(this.particlesToGridCS, "ResetGrid");
        }

        void Update()
        {
            this.ResetGrid();
            this.ComputeParticlesToGrid();
            //Util.PrintBuffer<Cell>(this.gridBuffer, 0, 10);
        }
        
        void ResetGrid()
        {
            this.particlesToGridCS.SetVector( ShaderID.CellStartPos, this.GetCellStartPos());
            this.particlesToGridCS.SetInt( ShaderID.GridResolutionWidth, gridWidth);
            this.particlesToGridCS.SetInt( ShaderID.GridResolutionHeight, gridHeight);
            this.particlesToGridCS.SetInt( ShaderID.GridResolutionDepth, gridDepth);
            this.particlesToGridCS.SetFloat( ShaderID.GridSpacingH, gridSpacingH);
            
            this.particlesToGridCS.SetBuffer(this.resetGridKernel.Index, ShaderID.GridBufferWrite, this.gridBuffer);
            this.particlesToGridCS.SetBuffer(this.resetGridKernel.Index, ShaderID.GridMassBufferWrite, this.gridMassBuffer);
            this.particlesToGridCS.SetBuffer(this.resetGridKernel.Index, ShaderID.GridPositionBufferWrite, this.gridPositionBuffer);
            
            this.particlesToGridCS.Dispatch(this.resetGridKernel.Index,
                Mathf.CeilToInt( (this.numOfCells) / (float)this.resetGridKernel.ThreadX),
                (int)this.resetGridKernel.ThreadY,
                (int)this.resetGridKernel.ThreadZ);
        }
        
        
        void ComputeParticlesToGrid()
        {
            this.particlesToGridCS.SetVector( ShaderID.CellStartPos, this.GetCellStartPos());
            this.particlesToGridCS.SetInt( ShaderID.GridResolutionWidth, gridWidth);
            this.particlesToGridCS.SetInt( ShaderID.GridResolutionHeight, gridHeight);
            this.particlesToGridCS.SetInt( ShaderID.GridResolutionDepth, gridDepth);
            this.particlesToGridCS.SetFloat( ShaderID.GridSpacingH, gridSpacingH);
            
            this.particlesToGridCS.SetInt( ShaderID.NumOfParticles, this.sphController.Configure.D.numOfParticle);
            this.particlesToGridCS.SetBuffer(this.particlesToGridKernel.Index, ShaderID.ParticlesBufferRead, this.sphController.SphData.particleBuffer.Data);
            this.particlesToGridCS.SetBuffer(this.particlesToGridKernel.Index, ShaderID.GridBufferWrite, this.gridBuffer);
            
            this.particlesToGridCS.SetBuffer(this.particlesToGridKernel.Index, ShaderID.GridMassBufferWrite, this.gridMassBuffer);
            this.particlesToGridCS.SetBuffer(this.particlesToGridKernel.Index, ShaderID.GridPositionBufferWrite, this.gridPositionBuffer);
            
            this.particlesToGridCS.Dispatch(this.particlesToGridKernel.Index,
                Mathf.CeilToInt(sphController.Configure.D.numOfParticle / (float)this.particlesToGridKernel.ThreadX),
                (int)this.particlesToGridKernel.ThreadY,
                (int)this.particlesToGridKernel.ThreadZ);
        }
        

        public Bounds GetGridBounds()
        {
            return new Bounds(
                Vector3.zero, //TODO: ここは後で修正
                this.gridSpacingH * this.GetGridDimension()
            );
        }

        public Vector3 GetGridDimension()
        {
            return new Vector3(this.gridWidth, this.gridHeight, this.gridDepth);
        }

        public Vector3 GetCellStartPos()
        {
            Bounds gridBounds = this.GetGridBounds();
            Debug.Log("gridBounds: " + gridBounds);
            return gridBounds.center - gridBounds.extents;
        }
        
        void OnDestroy()
        {
            gridBuffer?.Dispose();
            gridBuffer = null;
            
            gridMassBuffer?.Dispose();
            gridMassBuffer = null;
            
            gridPositionBuffer?.Dispose();
            gridPositionBuffer = null;
        }
    }
}