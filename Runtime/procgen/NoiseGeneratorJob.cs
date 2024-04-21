using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace xyz.zwks.procgen {

public static class NoiseGeneratorJob {

    public static readonly int iterations = 64;
    
    [BurstCompile]
    struct NoiseJob : IJobParallelFor {

        public int2 Dimensions;
        public float Scale;
        public int Octaves;
        public float Persistance;
        public float Lacunarity;

        [ReadOnly]
        public NativeArray<float2> OctaveOffsets;

        [WriteOnly]
        public NativeArray<float> Result;

        public void Execute(int index) {
            var halfWidth = Dimensions.x / 2;
            var halfHeight = Dimensions.y / 2;

            var amplitude = 1f;
            var frequency = 1f;
            var noiseHeight = 0f;

            var x = index % Dimensions.x;
            var y = index / Dimensions.x;

            for (var i = 0; i < Octaves; i++) {
                var sampleX = (x - halfWidth) / Scale * frequency + OctaveOffsets[i].x;
                var sampleY = (y - halfHeight) / Scale * frequency + OctaveOffsets[i].y;

                var perlinValue = Unity.Mathematics.noise.cnoise(new float2(sampleX, sampleY));
                perlinValue = perlinValue  * 2 - 1;

                noiseHeight += perlinValue * amplitude;

                amplitude *= Persistance;
                frequency *= Lacunarity;
            }
            
            Result[index] = noiseHeight;
        }
    }

    public static NativeArray<float> GenerateNoiseMap(int2 dimensions, uint seed, float scale, int octaves, float persistance, float lacunarity, float2 offset, NativeArray<float> jobResult) {
        if (scale <= 0) {
            scale = 0.0001f;
        }
        
        var random = new Unity.Mathematics.Random(seed);
        
        using var octaveOffsets = new NativeArray<float2>(octaves, Allocator.TempJob);

        for (var i = 0; i < octaves; i++) {
            var offsetX = random.NextInt(-100000, 100000) + offset.x;
            var offsetY = random.NextInt(-100000, 100000) + offset.y;
            var nativeOctaveOffsets = octaveOffsets;
            nativeOctaveOffsets[i] = new float2(offsetX, offsetY);
        }

        var job = new NoiseJob() {
            Dimensions = dimensions,
            Lacunarity = lacunarity,
            Octaves = octaves,
            OctaveOffsets = octaveOffsets,
            Persistance = persistance,
            Result = jobResult,
            Scale = scale,
        };

        var handle = job.Schedule(jobResult.Length, 32);
        handle.Complete();
        
        return jobResult;
    }

}
}