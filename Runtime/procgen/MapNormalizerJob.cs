using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace xyz.zwks.procgen {

public static class MapNormalizerJob {

    public static int iterations = 32;

    [BurstCompile]
    struct NormalizeJob : IJobParallelFor {
        [ReadOnly]
        public float minValue;
        [ReadOnly]
        public float maxValue;

        public NativeArray<float> Result;

        public void Execute(int index) {
            Result[index] = math.unlerp(minValue, maxValue, Result[index]);
        }
    }

    public static void Normalize(NativeArray<float> values) {
        
        var maxNoiseHeight = float.MinValue;
        var minNoiseHeight = float.MaxValue;

        for (var i = 0; i < values.Length; i++) {
            var noiseHeight = values[i];
            if (noiseHeight > maxNoiseHeight) {
                maxNoiseHeight = noiseHeight;
            } else if (noiseHeight < minNoiseHeight) {
                minNoiseHeight = noiseHeight;
            }
        }

        if(minNoiseHeight == maxNoiseHeight) {
            Debug.LogWarning("MapNormalizerJob.Normalize(): Input array values are all equal.");
            return;
        }

        var job = new NormalizeJob() {
            minValue = minNoiseHeight,
            maxValue =  maxNoiseHeight,
            Result = values,
        };

        var handle = job.Schedule(values.Length, iterations);
        handle.Complete();
    }
}
}