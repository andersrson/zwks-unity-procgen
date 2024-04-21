using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace xyz.zwks.procgen {

public static class MapToTextureJob {

    [BurstCompile]
    struct MapToTexture : IJobParallelFor {
        
        [ReadOnly]
        public NativeArray<float> Inputs;

        [WriteOnly]
        public NativeArray<Color> Colors;
        
        public void Execute(int index) {
            Colors[index] = Color.Lerp(Color.black, Color.white, Inputs[index]);
        }
    }

    public static Texture2D MapToTexture2D(NativeArray<float> values, int width, int height) {
        
        var job = new MapToTexture() {
            Inputs = values,
            Colors = new NativeArray<Color>(width * height, Allocator.TempJob)
        };
        
        var handle = job.Schedule(values.Length, 32);
        handle.Complete();

        Color[] colorMap = new Color[width * height];
        job.Colors.CopyTo(colorMap);
        job.Colors.Dispose();

        Texture2D outTex = new Texture2D(width, height);

        outTex.SetPixels(colorMap);
        outTex.filterMode = FilterMode.Trilinear;
        outTex.wrapMode = TextureWrapMode.Clamp;
        outTex.Apply();

        return outTex;
    }
}
}