
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace xyz.zwks.procgen {

[Serializable]
public class MeshGenerator : IPipelineNode, IDisposable, ISerializationCallbackReceiver {

    public Mesh outputMesh;

    public float HeightMapInfluence;

    NativeArray<float> InputArray;
    NoiseGenerator ng;

    bool invalidated = true;

    NativeArray<float3> verts;
    NativeArray<float2> uvs;
    NativeArray<int> Indices;

    public void Invalidate() {
        invalidated = true;
    }

    public void Generate() {

        if(!invalidated)
            return;

        if(ng == null) {
            ng = _pipeline.GetNode<NoiseGenerator>();
            InputArray = ng.DataArray;
        }

        if(outputMesh == null) {
            outputMesh = new Mesh();
        }

        if(!verts.IsCreated)
            verts = new NativeArray<float3>(ng.Width * ng.Height, Allocator.Persistent);
        else if (invalidated) {
            verts.Dispose();
            verts = new NativeArray<float3>(ng.Width * ng.Height, Allocator.Persistent);
        }
        if(!uvs.IsCreated)
            uvs = new NativeArray<float2>(ng.Width * ng.Height, Allocator.Persistent);
        else if (invalidated) {
            uvs.Dispose();
            uvs = new NativeArray<float2>(ng.Width * ng.Height, Allocator.Persistent);
        }
        if(!Indices.IsCreated)
            Indices = new NativeArray<int>((ng.Width - 1) * (ng.Height - 1) * 6, Allocator.Persistent);
        else if (invalidated) {
            Indices.Dispose();
            Indices = new NativeArray<int>((ng.Width - 1) * (ng.Height - 1) * 6, Allocator.Persistent);
        }

        outputMesh = MeshJob.NoiseMapToMesh(InputArray, verts, uvs, Indices, outputMesh, ng.Width, ng.Height, HeightMapInfluence);
        invalidated = false;

        _pipeline.NodeHasOutput(this);
    }

    public Mesh GetMeshOutput() {
        return outputMesh;
    }

    IProceduralPipeline _pipeline;

    public IProceduralPipeline Pipeline { get {return _pipeline; } }
    public void SetPipeline(IProceduralPipeline pipeline) {
        _pipeline = pipeline;
        _pipeline.PipelineDataPublish -= PipelineHasNewData;
        _pipeline.PipelineDataPublish += PipelineHasNewData;
    }

    void PipelineHasNewData(object sender, IPipelineNode node) {

        if(node == this)
            return;

        if(node is NoiseGenerator) {
            if(ng == null || ng != node) {
                ng = _pipeline.GetNode<NoiseGenerator>();
            }
            
            InputArray = ng.DataArray;
            outputMesh = null;
            invalidated = true;
            Generate();
        } 
    }

    public void OnAfterDeserialize() {
        ng = null;
    }

    public void OnBeforeSerialize() { }

#region dispose
    private bool disposedValue;

    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) { }

            if(verts.IsCreated)
                verts.Dispose();
            if(uvs.IsCreated)
                uvs.Dispose();
            if(Indices.IsCreated)
                Indices.Dispose();
            
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~NoiseGenerator()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
#endregion
}

}