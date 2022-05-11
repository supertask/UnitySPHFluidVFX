using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;
using Unity.Mathematics;

using ComputeShaderUtil;


namespace FluidSPH
{
    public class VFXSPHBinder : MonoBehaviour
    {
        public SPHController sphController;
        public ComputeShader copyToGraphicsBufferCS;

        private GraphicsBuffer typeBuffer, positionBuffer, indexBuffer, colorBuffer, lifetimeBuffer;
        private Kernel copyToGBufferKernel;
        
        
        
        void Start()
        {
            this.copyToGBufferKernel = new Kernel(copyToGraphicsBufferCS, "CopyToGraphicsBuffer");

            Debug.Log(sphController.SphData.particleBuffer);

            int maxNumParticles = sphController.Configure.D.numOfParticle;
            Debug.Log("maxNumParticles: " + maxNumParticles);

            typeBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxNumParticles, Marshal.SizeOf(typeof(int)) );            
            positionBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxNumParticles, Marshal.SizeOf(typeof(float3)) );
            indexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxNumParticles, Marshal.SizeOf(typeof(int)) );
            colorBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxNumParticles, Marshal.SizeOf(typeof(float4)) );
            lifetimeBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, maxNumParticles, Marshal.SizeOf(typeof(float)) );

            this.GetComponent<VisualEffect>().SetInt("MaxNumOfParticles", maxNumParticles);
            this.GetComponent<VisualEffect>().SetGraphicsBuffer("PositionBuffer", positionBuffer);
            this.GetComponent<VisualEffect>().SetGraphicsBuffer("IndexBuffer", indexBuffer);
            this.GetComponent<VisualEffect>().SetGraphicsBuffer("ColorBuffer", colorBuffer);
            this.GetComponent<VisualEffect>().SetGraphicsBuffer("TypeBuffer", typeBuffer);
            this.GetComponent<VisualEffect>().SetGraphicsBuffer("LifetimeBuffer", lifetimeBuffer);
            ///this.GetComponent<VisualEffect>().SetGraphicsBuffer("PositionBuffer", (GraphicsBuffer)liquid.PositionMass);
        }

        void OnDestroy()
        {
            typeBuffer?.Dispose();
            typeBuffer = null;

            indexBuffer?.Dispose();
            indexBuffer = null;
            
            positionBuffer?.Dispose();
            positionBuffer = null;
            
            colorBuffer?.Dispose();
            colorBuffer = null;
            
            lifetimeBuffer?.Dispose();
            lifetimeBuffer = null;
        }

        void Update()
        {
            //this.copyToGraphicsBufferCS.SetBuffer(copyToGBufferKernel.Index, "ReadParticleBuffer", sphController.SphData.particleBufferSorted.Data);
            this.copyToGraphicsBufferCS.SetBuffer(copyToGBufferKernel.Index, "ReadParticleBuffer", sphController.SphData.particleBuffer.Data);
            this.copyToGraphicsBufferCS.SetBuffer(copyToGBufferKernel.Index, "WriteParticlePositionBuffer", positionBuffer);
            this.copyToGraphicsBufferCS.SetBuffer(copyToGBufferKernel.Index, "WriteParticleIndexBuffer", indexBuffer);
            this.copyToGraphicsBufferCS.SetBuffer(copyToGBufferKernel.Index, "WriteColorBuffer", colorBuffer);
            this.copyToGraphicsBufferCS.SetBuffer(copyToGBufferKernel.Index, "WriteTypeBuffer", typeBuffer);
            this.copyToGraphicsBufferCS.SetBuffer(copyToGBufferKernel.Index, "WriteLifetimeBuffer", lifetimeBuffer);
            
            this.copyToGraphicsBufferCS.Dispatch(copyToGBufferKernel.Index,
                Mathf.CeilToInt((float)sphController.Configure.D.numOfParticle / copyToGBufferKernel.ThreadX), 1, 1);

            if (Time.frameCount % 24 == 0)
            {
                //PrintBuffer<float4>(liquid.PositionMass, 0, 1);
                //PrintBuffer<float4>(liquid.PositionRadius, 0, 1);
                //PrintBuffer<float4>(liquid.PositionRadius, 0, 1);
                //PrintBuffer<int>(liquid.ParticleNumber, 0, 1);
            }
        }
        
        
        //
        // Print Compute Buffer
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
