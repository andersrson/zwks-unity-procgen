
using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace xyz.zwks.procgen {

[Serializable]
public class NoiseGenerator : IPipelineNode, IDisposable {

#region noise props
    
    public int Width = 128;
    
    public int Height = 128;

    public float Scale = 1.5f;

    public float OffsetX = 0f; 

    public float OffsetY = 0f;

    public uint Seed = 1;

    public int Octaves = 3;

    public float Lacunarity = 2.1f;

    public float Persistence = 0.5f;
#endregion

    Texture2D texture2DOutput;

    bool invalidated = true;
    bool invalidatedDimensions = true;
    bool hasGenerated = false;

    internal NativeArray<float> DataArray;

    public bool Validate() {

        if(Width < 0 || Height < 0)
            return false;
        if(Octaves < 1)
            return false;
        if(Lacunarity < 0f || Persistence < 0f)
            return false;

        if(Scale == (int)Scale 
            && OffsetX == (int)OffsetX
            && OffsetY == (int)OffsetY
            && Lacunarity == (int)Lacunarity) {
            
            Debug.LogWarning("Scale, offset, and lacunarity are all integral values - this will generate a uniform map");
            return false;
        }

        if(Scale == (int)Scale 
            && OffsetX == (int)OffsetX
            && OffsetY == (int)OffsetY
            && Octaves == 1) {
            
            Debug.LogWarning("Scale and offset are all integral values and octaves is set to 1 - this will generate a uniform map");
            return false;
        }

        return true;
    }

    public void InvalidateOutput() {
        invalidated = true;
        hasGenerated = false;
    }

    public void InvalidateDimensions() {
        invalidatedDimensions = true;
        hasGenerated = false;
    }

    public Texture2D GetTextureOutput() {

        if(texture2DOutput  != null)
            return texture2DOutput;
        
        if(DataArray.Length == 0 || !DataArray.IsCreated)
            return null;
        
        texture2DOutput = MapToTextureJob.MapToTexture2D(DataArray, Width, Height);

        return texture2DOutput;
    }

    public void Generate() {

        if(!invalidated && !invalidatedDimensions && hasGenerated)
            return;

        if(!DataArray.IsCreated)
            DataArray = new NativeArray<float>(Width * Height, Allocator.Persistent);
        else {
            if(DataArray.Length != Width * Height) {
                DataArray.Dispose();
                DataArray = new NativeArray<float>(Width * Height, Allocator.Persistent);
            }
        }

        NoiseGeneratorJob.GenerateNoiseMap(new int2(Width, Height), Seed, Scale, 
            Octaves, Persistence, Lacunarity, new float2(OffsetX, OffsetY), DataArray);

        MapNormalizerJob.Normalize(DataArray);
        invalidated = false;
        invalidatedDimensions = false;
        hasGenerated = true;
        texture2DOutput = null;
        
        _pipeline.NodeHasOutput(this);
    }

    IProceduralPipeline _pipeline;
    
    public IProceduralPipeline Pipeline { get {return _pipeline; } }
    public void SetPipeline(IProceduralPipeline pipeline) {
        _pipeline = pipeline;
    }

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
}

}