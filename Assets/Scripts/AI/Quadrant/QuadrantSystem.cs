using System.Collections;
using System.Collections.Generic;
using Component;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class QuadrantSystem : JobComponentSystem {
    const int quadrantZMultiplier = 1000;
    const int quadrantCellSize = 50;
    
    public static int GetPositionHashMapKey(float3 pos) {
        return (int)(math.floor(pos.x / quadrantCellSize)) + (int)(quadrantZMultiplier * math.floor(pos.y / quadrantCellSize));
    }

    static void DebugDrawQuadrant(float3 pos) {
        Vector3 lowerLeft = new Vector3((math.floor(pos.x / quadrantCellSize)) * quadrantCellSize, math.floor(pos.y / quadrantCellSize) * quadrantCellSize);
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(1, 0) * quadrantCellSize);
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(0, 1) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(1, 0) * quadrantCellSize, lowerLeft + new Vector3(1, 1) * quadrantCellSize);
        Debug.DrawLine(lowerLeft + new Vector3(0, 1) * quadrantCellSize, lowerLeft + new Vector3(1, 1) * quadrantCellSize);
    }

    static int GetEntityCountInQuadrant(NativeMultiHashMap<int, float3> quadrantMultiHashMap, int hashMapKey) {
        float3 pos;
        NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
        int count = 0; 
        if (quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out pos, out nativeMultiHashMapIterator)) {
            do {
                count++;
            } while (quadrantMultiHashMap.TryGetNextValue(out pos, ref nativeMultiHashMapIterator));
        }

        return count;
    }

    [BurstCompile]
    struct SetQuadrantDataHashMapJob : IJobChunk {

        [ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;
        
        public NativeMultiHashMap<int, float3>.ParallelWriter quadrantHashMap;
        
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            NativeArray<Translation> chunkTranslations = chunk.GetNativeArray(translationType);
            
            for (int i = 0; i < chunk.ChunkEntityCount; i++) {

                int hashMapKey = GetPositionHashMapKey(chunkTranslations[i].Value);
                quadrantHashMap.Add(hashMapKey, chunkTranslations[i].Value);
            }
        }
    }

    public static NativeMultiHashMap<int, float3> quadrantMultiHashMap;
    public static JobHandle jobHandle;
    
    protected override void OnCreate() {
        quadrantMultiHashMap = new NativeMultiHashMap<int, float3>(0, Allocator.Persistent);  
    }

    protected override void OnDestroy() {
        quadrantMultiHashMap.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        EntityQuery query = GetEntityQuery(typeof(Translation), typeof(Boid));
        
        quadrantMultiHashMap.Clear();
        if (query.CalculateEntityCount() > quadrantMultiHashMap.Capacity) {
            quadrantMultiHashMap.Capacity = query.CalculateEntityCount();
        }
        
        ArchetypeChunkComponentType<Translation> translationChunk =  GetArchetypeChunkComponentType<Translation>();

        //Get neighbors
        SetQuadrantDataHashMapJob findNeighborsJob = new SetQuadrantDataHashMapJob() {
            translationType = translationChunk,
            quadrantHashMap = quadrantMultiHashMap.AsParallelWriter()
        };

        DebugDrawQuadrant(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        jobHandle = findNeighborsJob.Schedule(query, inputDeps);
        return jobHandle;
    }
}
