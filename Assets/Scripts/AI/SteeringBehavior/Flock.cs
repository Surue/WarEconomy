using System;
using System.Collections.Generic;
using Component;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;

public class Flock : MonoBehaviour {
    [SerializeField] GameObject prefabAgent_;
    [SerializeField] int numberToSpawn_ = 300;
    [SerializeField] float densitySpawn_ = 0.5f;

    List<Agent> agents_;

    [SerializeField] float neighborDetection_ = 1;
    [SerializeField] SO_SteeringBehavior steeringBehavior_;

    [SerializeField] bool useJobs_ = false;

    [Header("DOTS Specific")] [SerializeField]
    Mesh mesh_;

    [SerializeField] Material material_;

    void Start() {
        if (useJobs_) {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityArchetype archetype = entityManager.CreateArchetype(
                typeof(RenderMesh), // Rendering
                typeof(LocalToWorld), // Rendering
                typeof(RenderBounds), // Rendering
                typeof(Translation),
                typeof(NonUniformScale),
                typeof(Rotation),
                typeof(Boid),
                typeof(Separation),
                typeof(Cohesion),
                typeof(Constraint)
            );

            NativeArray<Entity> entityArray = new NativeArray<Entity>(numberToSpawn_, Allocator.Temp);
            entityManager.CreateEntity(archetype, entityArray);

            Unity.Mathematics.Random random = new Unity.Mathematics.Random(1);

            for (int i = 0; i < numberToSpawn_; i++) {
                Entity entity = entityArray[i];

                entityManager.SetSharedComponentData(entity, new RenderMesh {
                    mesh = mesh_,
                    material = material_
                });

                //Scale
                entityManager.SetComponentData(
                    entity,
                    new NonUniformScale {Value = 1}
                );

                //Translation
                entityManager.SetComponentData(
                    entity,
                    new Translation {
                        Value = densitySpawn_ * numberToSpawn_ *
                                random.NextFloat3(new float3(-1, -1, 0), new float3(1, 1, 0))
                    });

                //Agent
                entityManager.SetComponentData(
                    entity,
                    new Boid {
                        speed = 4.0f
                    });
            }

            entityArray.Dispose();
        } else {
            agents_ = new List<Agent>();

            for (int i = 0; i < numberToSpawn_; i++) {
                Agent instance =
                    Instantiate(prefabAgent_, densitySpawn_ * numberToSpawn_ * UnityEngine.Random.insideUnitCircle,
                            Quaternion.identity)
                        .GetComponent<Agent>();
                agents_.Add(instance);
                instance.name = "Boid " + i;
                instance.transform.parent = transform;
            }
        }
    }

    void Update() {
        if (useJobs_) {
        } else {
            foreach (Agent agent in agents_) {
                List<Transform> neighbor = GetNeighbor(agent);

                agent.GetComponent<SpriteRenderer>().color =
                    Color.Lerp(Color.white, Color.black,
                        neighbor.Count / 24.0f); //THIS IF FOR DEBUG PURPUSE NEVER DO THAT AGAIN

                agent.Move(steeringBehavior_.CalculateMove(agent, neighbor));
            }
        }
    }

    List<Transform> GetNeighbor(Agent boid) {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(boid.transform.position, neighborDetection_, Vector2.zero);

        List<Transform> neighbor = new List<Transform>();

        foreach (RaycastHit2D hit in hits) {
            if (hit.collider != boid.Collider2D) {
                neighbor.Add(hit.transform);
            }
        }

        return neighbor;
    }
}

public struct Separation : IComponentData {
    public float3 force;
}

public struct Cohesion : IComponentData {
    public float3 force;
}

public struct Constraint : IComponentData {
    public float3 force;
}

public struct Alignment : IComponentData {
    public float3 force;
}

[BurstCompile]
public class BoidsSystem : JobComponentSystem {

    [BurstCompile]
    struct SeparationJob : IJobChunk {
        [ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;
        [ReadOnly] public NativeMultiHashMap<int, float3> quadrantHashMap;
        
        public ArchetypeChunkComponentType<Separation> separationType;
        
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            NativeArray<Separation> chunkSeparation = chunk.GetNativeArray(separationType);
            NativeArray<Translation> chunkTranslation = chunk.GetNativeArray(translationType);
            
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            
            for (int i = 0; i < chunk.Count; i++) {
                float3 force = 0;
                float3 position = chunkTranslation[i].Value;

                int count = 0;

                int hashMapKey = QuadrantSystem.GetPositionHashMapKey(position);
                
                float3 otherPosition;
                if (quadrantHashMap.TryGetFirstValue(hashMapKey, out otherPosition, out nativeMultiHashMapIterator)) {
                    do {
                        float distance = math.distance(otherPosition, position);
                        if (distance < 1.5 && distance > 0.001f) {
                            force += position - otherPosition;
                            count++;
                        }
                    } while (quadrantHashMap.TryGetNextValue(out otherPosition, ref nativeMultiHashMapIterator));
                }

                if (count > 0) {
                    force /= count;
                }
                
                chunkSeparation[i] = new Separation
                {
                    force = force
                };
            }
        }
    }
    
//    [BurstCompile]
//    struct AlignmentJob : IJobChunk {
//        [ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;
//        [ReadOnly] public ArchetypeChunkComponentType<Boid> agentType;
//        [ReadOnly] public NativeMultiHashMap<int, float3> quadrantHashMap;
//        
//        public ArchetypeChunkComponentType<Alignment> separationType;
//        
//        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
//            NativeArray<Alignment> chunkAlignment = chunk.GetNativeArray(separationType);
//            NativeArray<Translation> chunkTranslation = chunk.GetNativeArray(translationType);
//            NativeArray<Boid> chunkAgent = chunk.GetNativeArray(agentType);
//            
//
//            float sqrDistance = 1.5f * 1.5f;
//            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
//            
//            for (int i = 0; i < chunk.Count; i++) {
//                float3 desired = Vector3.zero;
//                float3 force = Vector3.zero;
//                float3 position = chunkTranslation[i].Value;
//                
//                int hashMapKey = QuadrantSystem.GetPositionHashMapKey(chunkTranslation[i].Value);
//                float3 otherPosition = new float3(0, 0, 0);
//                int count = 0;
//                if (quadrantHashMap.TryGetFirstValue(hashMapKey, out otherPosition, out nativeMultiHashMapIterator)) {
//                    do {
//                        float distance = math.distance(position, otherPosition);
//                        if (distance < 4.5f && distance > 0.001f) {
//                            desired += rotations[neighbourId] * Vector3.forward;
//                    
//                            force += position - otherPosition;
//                            count++;
//                        }
//                    } while (quadrantHashMap.TryGetNextValue(out otherPosition, ref nativeMultiHashMapIterator));
//                }
//
//                if (count > 0)
//                {
//                    desired /= count;
//                    force = desired - (chunkAgent[i].up);
//                }
//                
//                chunkAlignment[i] = new Alignment
//                {
//                    force = force
//                };
//            }
//        }
//    }
    
    [BurstCompile]
    struct ConstraintJob : IJobChunk {
        [ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;
        
        public ArchetypeChunkComponentType<Constraint> constraintType;
        
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            NativeArray<Constraint> chunkConstraints = chunk.GetNativeArray(constraintType);
            NativeArray<Translation> chunkTranslation = chunk.GetNativeArray(translationType);

            float radius = 500;
            
            for (int i = 0; i < chunk.Count; i++) {
                float3 centerOffset = -chunkTranslation[i].Value;

                float t = math.length(centerOffset) / radius;

                float3 force = new float3(0, 0, 0);

                if (t > 0.8f) {
                    force = t * t * centerOffset;
                }
                
                chunkConstraints[i] = new Constraint
                {
                    force = force
                };
            }
        }
    }
    
    [BurstCompile]
    struct CohesionJob : IJobChunk {
        public ArchetypeChunkComponentType<Cohesion> cohesionType;
        [ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;
        
        [ReadOnly] public NativeMultiHashMap<int, float3> quadrantHashMap;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            NativeArray<Cohesion> chunkCohesion = chunk.GetNativeArray(cohesionType);
            NativeArray<Translation> chunkTranslation = chunk.GetNativeArray(translationType);
            
            NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
            
            for (int i = 0; i < chunk.Count; i++) {
                float3 force = Vector3.zero;
                float3 position = chunkTranslation[i].Value;
                
                int hashMapKey = QuadrantSystem.GetPositionHashMapKey(chunkTranslation[i].Value);
                float3 otherPosition = new float3(0, 0, 0);
                int count = 0;
                if (quadrantHashMap.TryGetFirstValue(hashMapKey, out otherPosition, out nativeMultiHashMapIterator)) {
                    do {
                        float distance = math.distance(position, otherPosition);
                        if (distance < 4.5f && distance > 0.001f) {
                            force += otherPosition;
                            count++;
                        }
                    } while (quadrantHashMap.TryGetNextValue(out otherPosition, ref nativeMultiHashMapIterator));
                }

                if (count > 0) {
                    force /= count;

                    force -= chunkTranslation[i].Value;
                    
                    math.smoothstep(force, new float3(0, 0, 0), 0.5f);
                }
                
                chunkCohesion[i] = new Cohesion
                {
                    force = force
                };
            }
        }
    }
    
    [BurstCompile]
    struct BoidsJob : IJobChunk {
        public float dt;
        [ReadOnly] public ArchetypeChunkComponentType<Separation> separationType;
        [ReadOnly] public ArchetypeChunkComponentType<Cohesion> cohesionType;
        [ReadOnly] public ArchetypeChunkComponentType<Constraint> constraintType;
        public ArchetypeChunkComponentType<Boid> agentType;
        public ArchetypeChunkComponentType<Translation> translationType;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            NativeArray<Separation> chunkSeparation = chunk.GetNativeArray(separationType);
            NativeArray<Cohesion> chunkCohesion = chunk.GetNativeArray(cohesionType);
            NativeArray<Constraint> chunkConstraint = chunk.GetNativeArray(constraintType);
            NativeArray<Translation> chunkTranslation = chunk.GetNativeArray(translationType);
            NativeArray<Boid> chunkAgent = chunk.GetNativeArray(agentType);
            
            const float separationWeight = 5;
            const float sqrSeparationWeight = separationWeight * separationWeight;
            
            const float cohesionWeight = 1;
            const float sqrCohesionWeight = cohesionWeight * cohesionWeight;
            
            const float constraintWeight = 0.1f;
            const float sqrConstraintWeight = constraintWeight * constraintWeight;
            
            for (int i = 0; i < chunk.Count; i++) {

                float3 separationForce = chunkSeparation[i].force;

                if (math.lengthsq(separationForce) > sqrSeparationWeight) {
                    separationForce = math.normalize(separationForce) * separationWeight;
                }
                
                float3 cohesionForce = chunkCohesion[i].force;
                if (math.lengthsq(cohesionForce) > sqrCohesionWeight) {
                    cohesionForce = math.normalize(cohesionForce) * cohesionWeight;
                }
                
                float3 constraintForce = chunkConstraint[i].force;
                if (math.lengthsq(constraintForce) > sqrConstraintWeight) {
                    constraintForce = math.normalize(constraintForce) * constraintWeight;
                }
                
                float3 newVelocity = cohesionForce + separationForce + constraintForce;

//                if (Magnitude(newVelocity) > chunkAgent[i].speed) {
//                    newVelocity = (newVelocity / Magnitude(newVelocity)) * chunkAgent[i].speed;
//                }

                chunkAgent[i] = new Boid {
                    velocity = newVelocity,
                    speed = chunkAgent[i].speed
                };
                
                chunkTranslation[i] = new Translation
                {
                    Value = chunkTranslation[i].Value + (newVelocity * chunkAgent[i].speed * dt),
                };
            }
        }
    }
    
    EntityQuery entityQuery_;

    NativeArray<float3> neighborsPosition_;
    int maxNeighbors = 30;

    protected override void OnCreate() {
        entityQuery_ = GetEntityQuery(typeof(Separation), typeof(Translation), typeof(Boid), typeof(Cohesion));
        
        neighborsPosition_ = new NativeArray<float3>(100 * maxNeighbors, Allocator.Persistent);
    }

    protected override void OnDestroy() {
        neighborsPosition_.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        if (neighborsPosition_.Length < entityQuery_.CalculateEntityCount() * maxNeighbors) {
            neighborsPosition_.Dispose();
            neighborsPosition_ = new NativeArray<float3>(entityQuery_.CalculateEntityCount() * maxNeighbors, Allocator.Persistent);
        }
        
        ArchetypeChunkComponentType<Separation> separationChunk =  GetArchetypeChunkComponentType<Separation>();
        ArchetypeChunkComponentType<Translation> translationChunk =  GetArchetypeChunkComponentType<Translation>();
        ArchetypeChunkComponentType<Boid> agentChunk = GetArchetypeChunkComponentType<Boid>();
        ArchetypeChunkComponentType<Cohesion> cohesionChunk = GetArchetypeChunkComponentType<Cohesion>();
        ArchetypeChunkComponentType<Constraint> constraintChunk = GetArchetypeChunkComponentType<Constraint>();

        NativeMultiHashMap<int, float3> quadrantHasMap = QuadrantSystem.quadrantMultiHashMap;

        QuadrantSystem.jobHandle.Complete();
        
        //Separation
        SeparationJob separationJob = new SeparationJob {
            separationType = separationChunk,
            translationType = translationChunk,
            quadrantHashMap =  quadrantHasMap
        };

        JobHandle separationJobHandle = separationJob.Schedule(entityQuery_, inputDeps);
        
        //Cohesion
        CohesionJob cohesionJob = new CohesionJob {
            cohesionType = cohesionChunk,
            translationType = translationChunk,
            quadrantHashMap =  quadrantHasMap
        };

        JobHandle cohesionJobHandle = cohesionJob.Schedule(entityQuery_, separationJobHandle);
        
        //Constraint
        ConstraintJob constraintJob = new ConstraintJob {
            constraintType = constraintChunk,
            translationType = translationChunk
        };

        JobHandle constraintJobHandle = constraintJob.Schedule(entityQuery_, cohesionJobHandle);

        //Boids
        BoidsJob boidsJob = new BoidsJob() {
            cohesionType = cohesionChunk,
            separationType = separationChunk,
            agentType = agentChunk,
            translationType = translationChunk,
            constraintType = constraintChunk,
            dt = Time.DeltaTime
        };

        JobHandle boidsJobHandle = boidsJob.Schedule(entityQuery_, constraintJobHandle);
        
        separationJobHandle.Complete();
        cohesionJobHandle.Complete();
        
        
        return  boidsJobHandle;
    }
}
   