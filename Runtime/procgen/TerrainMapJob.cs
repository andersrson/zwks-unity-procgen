using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace xyz.zwks.procgen {

public static class TerrainMapJob {

    [BurstCompile]
    struct TerrainMapToTextureJob : IJobParallelFor {
        
        [ReadOnly]
        public NativeArray<float> Inputs;

        [ReadOnly]
        public NativeArray<TerrainType> TerrainTypes;

        [WriteOnly]
        public NativeArray<Color> Colors;
        
        public void Execute(int index) {
            float height = Inputs[index];
            Color start = TerrainTypes[0].Color;
            
            for(int i = 0; i < TerrainTypes.Length; i++) {
                if(height >= TerrainTypes[i].MinHeight)
                    start = TerrainTypes[i].Color;
                else 
                    break;
            }

            Colors[index] = start;
        }
    }

    public static Texture2D TerrainMapToTexture(NativeArray<float> values, List<TerrainType> terrains, int width, int height) {
        
        NativeArray<TerrainType> nt = new NativeArray<TerrainType>(terrains.Count, Allocator.TempJob);
        nt.CopyFrom(terrains.ToArray());
        var job = new TerrainMapToTextureJob() {
            Inputs = values,
            Colors = new NativeArray<Color>(width * height, Allocator.TempJob),
            TerrainTypes = nt
        };
        
        var handle = job.Schedule(values.Length, 32);
        handle.Complete();

        Color[] colorMap = new Color[width * height];
        job.Colors.CopyTo(colorMap);
        job.Colors.Dispose();

        nt.Dispose();
        
        Texture2D outTex = new Texture2D(width, height);

        outTex.SetPixels(colorMap);
        outTex.filterMode = FilterMode.Trilinear;
        outTex.wrapMode = TextureWrapMode.Clamp;
        outTex.Apply();

        return outTex;
    }
}
}