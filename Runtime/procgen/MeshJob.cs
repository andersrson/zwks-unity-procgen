using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace xyz.zwks.procgen {

struct MeshData {
    public float3[] vertices;
	public int[] triangles;
	public float2[] uvs;
}

public static class MeshJob {

    [BurstCompile]
    struct MeshGenJob : IJobParallelFor {
        
        [ReadOnly]
        public NativeArray<float> Inputs;

        [ReadOnly]
        public int Width;
        [ReadOnly]
        public int Height;

        [ReadOnly]
        public float MaxHeight;

        [ReadOnly]
        public float TopleftX;

        [ReadOnly]
        public float TopleftZ;

        [WriteOnly]
        public NativeArray<float3> Verts;
        [WriteOnly]
        public NativeArray<float2> UVs;
        
        [NativeDisableParallelForRestriction]
        [WriteOnly]
        public NativeArray<int> Indices;

        public void Execute(int index) {
            int x = index % Width;
            int y = index / Width;

            Verts[index] = new float3(TopleftX + x, Inputs[index] * MaxHeight, TopleftZ - y);
            UVs[index] = new float2(x / (float)Width, y / (float)Height);

            //int idxOffset = y * (Width - 1) + x;
            int idxOffset = (index - y) * 6;

            if(x < Width - 1 && y < Height - 1) {
                Indices[idxOffset] = index;
                Indices[idxOffset+1] = index + Width + 1;
                Indices[idxOffset+2] = index + Width;
                Indices[idxOffset+3] = index + Width + 1;
                Indices[idxOffset+4] = index;
                Indices[idxOffset+5] = index + 1;
            }
        }
    }

    public static Mesh NoiseMapToMesh(NativeArray<float> values, 
            NativeArray<float3> verts, 
            NativeArray<float2> uvs,
            NativeArray<int> Indices,
            Mesh outputMesh,
            int width, int height, float maxHeight = 20) {
        
        var job = new MeshGenJob() {
            Inputs = values,
            Width = width,
            Height = height,
            TopleftX = (width - 1) / -2f,
            TopleftZ = (height - 1) / 2f,
            MaxHeight = maxHeight,

            Verts = verts,
            UVs = uvs,
            Indices = Indices
        };
        
        var handle = job.Schedule(values.Length, 32);
        handle.Complete();

        int[] tris = new int[Indices.Length];
        Indices.CopyTo(tris);

        if(outputMesh == null) {
            outputMesh = new Mesh();
            outputMesh.Clear();
        } else {
            outputMesh.Clear();
        }
        
        // Can't be arsed to deal with submeshes rn..
        //if(width * height > UInt16.MaxValue)
            
        outputMesh.SetVertices(verts);
        outputMesh.triangles = tris;
        outputMesh.SetUVs(0, uvs);
        outputMesh.RecalculateNormals();
        outputMesh.RecalculateTangents();
        
        return outputMesh;
    }
}
}