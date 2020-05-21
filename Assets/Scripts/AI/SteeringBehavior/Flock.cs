using System.Collections.Generic;
using Component;
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
                typeof(Cohesion)
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
                        speed = 4.0f,
                        nbNeighbors =  0
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

public class BoidsSystem : JobComponentSystem {

    struct FindNeighborsJob : IJobChunk {
        [ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;
        public ArchetypeChunkComponentType<Boid> agentType;
        [ReadOnly] public NativeArray<Translation> position;
        
        public NativeArray<float3> neighborsPosition;
        
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            NativeArray<Translation> chunkTranslation = chunk.GetNativeArray(translationType);
            NativeArray<Boid> chunkAgents = chunk.GetNativeArray(agentType);
            
            for (int i = 0; i < chunk.Count; i++) {

                int count = 0;

                NativeArray<float3> poses = position.Reinterpret<float3>();
                for (int index = 0; index < poses.Length; index++) {
                    if (firstEntityIndex + i == index) continue;
                    
                    float3 pos = poses[index];
                    float dist = Distance(chunkTranslation[i].Value, pos);

                    if (dist > 4.5f) {
                        continue;
                    }

                    neighborsPosition[firstEntityIndex + i + count] = pos;
                    count++;
                    if (count == 30) {
                        break;
                    }
                }

                chunkAgents[i] = new Boid {
                    speed = chunkAgents[i].speed,
                    nbNeighbors = count
                };
            }
        }

        float Distance(float3 pos0, float3 pos1) {
            return Mathf.Sqrt(Mathf.Pow(pos1.x - pos0.x, 2) + Mathf.Pow(pos1.y - pos0.y, 2));
        }
    }
    
    struct SeparationJob : IJobChunk {
        [ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;
        [ReadOnly] public ArchetypeChunkComponentType<Boid> agentType;
        [ReadOnly] public NativeArray<float3> neighborsPosition;
        
        public ArchetypeChunkComponentType<Separation> separationType;
        
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            NativeArray<Separation> chunkSeparation = chunk.GetNativeArray(separationType);
            NativeArray<Translation> chunkTranslation = chunk.GetNativeArray(translationType);
            NativeArray<Boid> chunkAgent = chunk.GetNativeArray(agentType);

            float sqrDistance = 1.5f * 1.5f;
            
            for (int i = 0; i < chunk.Count; i++) {
                float3 force = 0;
                float3 position = chunkTranslation[i].Value;

                int count = 0;
                
                for (int j = 0; j < chunkAgent[i].nbNeighbors; j++) {
                    
                    float3 otherPosition = neighborsPosition[firstEntityIndex + i + j];
                    
                    if (SqrDistance(otherPosition, position) > sqrDistance) continue;
                    
                    force += position - otherPosition;
                    count++;
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
        
        float SqrDistance(float3 pos0, float3 pos1) {
            return Mathf.Pow(pos1.x - pos0.x, 2) + Mathf.Pow(pos1.y - pos0.y, 2);
        }
    }
    
    struct CohesionJob : IJobChunk {
        public ArchetypeChunkComponentType<Cohesion> cohesionType;
        [ReadOnly] public ArchetypeChunkComponentType<Translation> translationType;
        [ReadOnly] public ArchetypeChunkComponentType<Boid> agentType;
        
        [ReadOnly] public  NativeArray<float3> neighborsPosition;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            NativeArray<Cohesion> chunkCohesion = chunk.GetNativeArray(cohesionType);
            NativeArray<Translation> chunkTranslation = chunk.GetNativeArray(translationType);
            NativeArray<Boid> chunkAgent = chunk.GetNativeArray(agentType);
            
            for (int i = 0; i < chunk.Count; i++) {
                float3 move = Vector3.zero;
                
                for (int j = 0; j < chunkAgent[i].nbNeighbors; j++) {
                    move += neighborsPosition[firstEntityIndex + i + j];
                }

                if (chunkAgent[i].nbNeighbors > 0) {
                    move /= chunkAgent[i].nbNeighbors;

                    move -= chunkTranslation[i].Value;
                    
                    //Add smooth damping
                }
                
                chunkCohesion[i] = new Cohesion
                {
                    force = move
                };
            }
        }
    }
    
    struct BoidsJob : IJobChunk {
        public float dt;
        [ReadOnly] public ArchetypeChunkComponentType<Separation> separationType;
        [ReadOnly] public ArchetypeChunkComponentType<Cohesion> cohesionType;
        public ArchetypeChunkComponentType<Boid> agentType;
        public ArchetypeChunkComponentType<Translation> translationType;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex) {
            NativeArray<Separation> chunkSeparation = chunk.GetNativeArray(separationType);
            NativeArray<Cohesion> chunkCohesion = chunk.GetNativeArray(cohesionType);
            NativeArray<Translation> chunkTranslation = chunk.GetNativeArray(translationType);
            NativeArray<Boid> chunkAgent = chunk.GetNativeArray(agentType);
            
            const float separationWeight = 5;
            const float sqrSeparationWeight = separationWeight * separationWeight;
            
            const float cohesionWeight = 1;
            const float sqrCohesionWeight = cohesionWeight * cohesionWeight;
            
            for (int i = 0; i < chunk.Count; i++) {

                float3 separationForce = chunkSeparation[i].force;

                if (SqrMagnitude(separationForce) > sqrSeparationWeight) {
                    separationForce = separationForce / Magnitude(separationForce) * separationWeight;
                }
                
                float3 cohesionForce = chunkCohesion[i].force;
                if (SqrMagnitude(cohesionForce) > sqrCohesionWeight) {
                    cohesionForce = cohesionForce / Magnitude(cohesionForce) * cohesionWeight;
                }
                
                float3 newVelocity = cohesionForce + separationForce;

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
        
        float SqrMagnitude(float3 v) {
            return Mathf.Pow(v.x, 2) + Mathf.Pow(v.y, 2);
        }

        float Magnitude(float3 v) {
            return Mathf.Sqrt(Mathf.Pow(v.x, 2) + Mathf.Pow(v.y, 2));
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

        NativeArray<Translation> positions = entityQuery_.ToComponentDataArrayAsync<Translation>(Allocator.TempJob, out inputDeps);

        //Get neighbors
        FindNeighborsJob findNeighborsJob = new FindNeighborsJob() {
            translationType = translationChunk,
            agentType = agentChunk,
            position = positions,
            neighborsPosition = neighborsPosition_
        };

        JobHandle findNeighborsJobHandle = findNeighborsJob.Schedule(entityQuery_, inputDeps);
        
        //Separation
        SeparationJob separationJob = new SeparationJob {
            separationType = separationChunk,
            translationType = translationChunk,
            neighborsPosition =  neighborsPosition_,
            agentType = agentChunk
        };

        JobHandle separationJobHandle = separationJob.Schedule(entityQuery_, findNeighborsJobHandle);
        
        //Cohesion
        CohesionJob cohesionJob = new CohesionJob {
            cohesionType = cohesionChunk,
            translationType = translationChunk,
            neighborsPosition =  neighborsPosition_,
            agentType = agentChunk
        };

        JobHandle cohesionJobHandle = cohesionJob.Schedule(entityQuery_, separationJobHandle);

        //Boids
        BoidsJob boidsJob = new BoidsJob() {
            cohesionType = cohesionChunk,
            separationType = separationChunk,
            agentType = agentChunk,
            translationType = translationChunk,
            dt = Time.DeltaTime
        };
            
        JobHandle boidsJobHandle = boidsJob.Schedule(entityQuery_, cohesionJobHandle);
        
        return  positions.Dispose(boidsJobHandle);
    }
}
   