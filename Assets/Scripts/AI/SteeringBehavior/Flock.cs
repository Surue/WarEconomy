using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine.Rendering;

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
                typeof(RenderMesh),     // Rendering
                typeof(LocalToWorld),   // Rendering
                typeof(RenderBounds),   // Rendering
                typeof(Translation),
                typeof(Scale),
                typeof(Rotation)
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
                    new Scale {Value = 1}
                );

                //Translation
                entityManager.SetComponentData(
                    entity,
                    new Translation {
                        Value = densitySpawn_ * numberToSpawn_ *
                                random.NextFloat3(new float3(-1, -1, 0), new float3(1, 1, 0))
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

public class MoveSystem : ComponentSystem {
    protected override void OnUpdate() {
        Entities.ForEach((ref Translation translation) => {
            float moveSpeed = 0.1f;
//            translation.Value.y += moveSpeed;
        });
    }
}