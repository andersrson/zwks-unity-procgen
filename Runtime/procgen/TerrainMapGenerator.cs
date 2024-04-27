
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace xyz.zwks.procgen {

[Serializable]
public struct TerrainType {
    public float MinHeight;
    public Color Color;
}

[Serializable]
public class TerrainMapGenerator : IPipelineNode, IDisposable, ISerializationCallbackReceiver {

    public List<TerrainType> terrainTypes;

    [SerializeField]
    Texture2D outputTexture;

    [SerializeField]
    Color[] colors;

    NativeArray<Color> DataArray;
    NativeArray<float> InputArray;
    NoiseGenerator ng;

    bool invalidated = true;

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

        outputTexture = TerrainMapJob.TerrainMapToTexture(InputArray, terrainTypes, ng.Width, ng.Height);
        
        invalidated = false;

        _pipeline.NodeHasOutput(this);
    }

    public Texture2D GetTextureOutput() {
        return outputTexture;
    }

    IProceduralPipeline _pipeline;

    public IProceduralPipeline Pipeline { get {return _pipeline; } }
    public void SetPipeline(IProceduralPipeline pipeline) {
        _pipeline = pipeline;
        _pipeline.PipelineDataPublish -= PipelineHasNewData;
        _pipeline.PipelineDataPublish += PipelineHasNewData;
    }

    void PipelineHasNewData(object sender, IPipelineNode node) {
        if(terrainTypes == null || terrainTypes.Count < 1)
            return;

        if(node == this)
            return;

        if(node is NoiseGenerator) {
            if(ng == null || ng != node) {
                ng = _pipeline.GetNode<NoiseGenerator>();
            }
            
            InputArray = ng.DataArray;
            outputTexture = null;
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

            if(DataArray.IsCreated)
                DataArray.Dispose();

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