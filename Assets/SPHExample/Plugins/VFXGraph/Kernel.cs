using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ComputeShaderUtil {

	public class Kernel
	{
		public int Index { get { return index; } }
		public uint ThreadX { get { return threadX; } }
		public uint ThreadY { get { return threadY; } }
		public uint ThreadZ { get { return threadZ; } }

		int index;
		uint threadX, threadY, threadZ;

		public Kernel(ComputeShader shader, string key)
		{
			index = shader.FindKernel(key);
			if (index < 0)
			{
				Debug.LogWarning("Can't find kernel");
				return;
			}
			shader.GetKernelThreadGroupSizes(index, out threadX, out threadY, out threadZ);
		}
	}
	
	public class Util
	{
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
		
        //
        // Print Compute Buffer
        // When you define a struct/class,
        // please use override ToString(), public override string ToString() => $"MpmParticle(position={position}, velocity={velocity})";
        //
        // debugging range is startIndex <= x < endIndex
        // example: 
        //    Util.PrintBuffer<uint2>(this.particlesBuffer, 1024, 1027); 
        //
        public static void PrintBuffer<T>(GraphicsBuffer buffer, int startIndex, int endIndex) where T  : struct
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