using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace AI {
public class PathNodeContainer : MonoBehaviour {
    NativeArray<PathNode> nodes_;
    NativeMultiHashMap<int, PathNodeLink> neighbors_;

    public NativeArray<PathNode> Nodes => nodes_;

    void Start() {
        List<WayPoint> wayPoints = FindObjectsOfType<WayPoint>().ToList();
        
        neighbors_ = new NativeMultiHashMap<int, PathNodeLink>(wayPoints.Count, Allocator.Persistent);
        nodes_ = new NativeArray<PathNode>(wayPoints.Count ,Allocator.Persistent);

        int index = 0;
        Dictionary<int, int> realIndex = new Dictionary<int, int>();
        foreach (WayPoint wayPoint in wayPoints) {
            realIndex[index] = wayPoint.Index;
            index++;
        }
        index = 0;

        foreach (WayPoint wayPoint in wayPoints) {
            PathNode node;
            Vector3 wayPointPosition = wayPoint.transform.position;
            
            node.position = new float3(wayPointPosition.x, wayPointPosition.y, wayPointPosition.z);
            node.index = index;
            foreach (EditorNodeLink wayPointLink in wayPoint.Links) {
                int otherIndex = realIndex[wayPointLink.wayPoint.Index];
                neighbors_.Add(index, new PathNodeLink {
                    distance = wayPointLink.distance,
                    otherIndex = otherIndex
                });
            }
            
            nodes_[index] = node;
            index++;
        }
        
        Debug.Log("FINISH");
    }

    void OnDestroy() {
//        for (int i = 0; i < nodes_.Length; i++) {
//            nodes_[i].neighborsIndex.Dispose();
//        }
        
        nodes_.Dispose();
        neighbors_.Dispose();
    }

    // Update is called once per frame
    void Update() {

    }

    void OnDrawGizmos() {
        if (nodes_.Length == 0) {
            return;
        }

        int index = 0;
        foreach (PathNode pathNode in nodes_) {
            Gizmos.DrawWireSphere(pathNode.position, 1);
            
            foreach (PathNodeLink pathNodeLink in neighbors_.GetValuesForKey(index)) {
                Gizmos.DrawLine(pathNode.position, nodes_[pathNodeLink.otherIndex].position);
            }

            index++;
        }
    }
}
}
