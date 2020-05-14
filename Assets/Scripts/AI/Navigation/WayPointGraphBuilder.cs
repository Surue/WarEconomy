using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;
using UnityEditor;
using UnityEngine;

public class WayPointGraphBuilder : MonoBehaviour {
    [SerializeField] bool setup_ = false;

    [SerializeField] Vector2 mapCenter_;
    [SerializeField] Vector2 mapSize_;
    [SerializeField][Range(1, 10)] float radiusBetweenSubWayPoint_;

    [SerializeField] List<WayPoint> mainWayPoints_;
    [SerializeField] List<WayPoint> subWayPoints_;
    void OnValidate() {
        if (radiusBetweenSubWayPoint_ <= 1) {
            radiusBetweenSubWayPoint_ = 1f;
        }
        
        if (!setup_) return;
        
        setup_ = false;

        if (subWayPoints_ == null) {
            subWayPoints_ = new List<WayPoint>();
        }
        
        foreach (WayPoint subWayPoint in subWayPoints_) {
            if(subWayPoint)
                DestroyImmediate(subWayPoint.gameObject);
        }
        
        subWayPoints_ = new List<WayPoint>();
        mainWayPoints_ = new List<WayPoint>();

        List<WayPoint> wayPoints = FindObjectsOfType<WayPoint>().ToList();

        PathFinder pathFinder = FindObjectOfType<PathFinder>();

        for (int i = 0; i < wayPoints.Count; i++) {
            wayPoints[i].Reset(i, pathFinder);
            
            mainWayPoints_.Add(wayPoints[i]);
        }
        
        for (int i = 0; i < wayPoints.Count; i++) {
            wayPoints[i].ComputeLinks();
        }

        //Generate sub point
        for (int x = 0; x < mapSize_.x / (radiusBetweenSubWayPoint_ * 2); x++) {
            for (int y = 0; y < mapSize_.y / (radiusBetweenSubWayPoint_ * 2); y++) {
                bool canPlace = true;
                
                Vector3 position = new Vector3(x * radiusBetweenSubWayPoint_ * 2 + mapCenter_.x - mapSize_.x * 0.5f, 0,
                    y * radiusBetweenSubWayPoint_ * 2 + mapCenter_.y - mapSize_.y * 0.5f);
                
                foreach (WayPoint wayPoint in wayPoints) {
                    if (Vector3.Distance(position, wayPoint.transform.position) < radiusBetweenSubWayPoint_) {
                        canPlace = false;
                        break;
                    }
                }

                if (canPlace) {
                    GameObject instance = new GameObject();
                    instance.transform.position = position;
                    instance.transform.parent = transform;

                    instance.AddComponent<WayPoint>();
                    
                    subWayPoints_.Add(instance.GetComponent<WayPoint>());
                }
            }
        }

        for (int i = 0; i < subWayPoints_.Count; i++) {
            subWayPoints_[i].Reset(i + wayPoints.Count, pathFinder);
            
            foreach (WayPoint wayPoint in subWayPoints_) {
                float distance = Vector3.Distance(wayPoint.transform.position, subWayPoints_[i].transform.position);
                if (distance < radiusBetweenSubWayPoint_ * 2 + radiusBetweenSubWayPoint_ ) {
                    if (!subWayPoints_[i].IsNeighbor(wayPoint)) {
                        subWayPoints_[i].AddNeighbor(wayPoint, 2, distance);
                    }
                }
            }
            
            foreach (WayPoint wayPoint in wayPoints) {
                float distance = Vector3.Distance(wayPoint.transform.position, subWayPoints_[i].transform.position);
                if (distance < radiusBetweenSubWayPoint_ * 2 + radiusBetweenSubWayPoint_) {
                    if (!subWayPoints_[i].IsNeighbor(wayPoint)) {
                        subWayPoints_[i].AddNeighbor(wayPoint, 2, distance);
                    }
                }
            }
        }

        for (int i = 0; i < subWayPoints_.Count; i++) {
            subWayPoints_[i].ComputeLinks();
        }
    }

    void OnDrawGizmos() {
        Gizmos.DrawWireCube(new Vector3(mapCenter_.x, 0, mapCenter_.y), new Vector3(mapSize_.x, 0, mapSize_.y));
//
//        for (int x = 0; x < mapSize_.x / (radiusBetweenSubWayPoint_ * 2); x++) {
//            for (int y = 0; y < mapSize_.y / (radiusBetweenSubWayPoint_ * 2); y++) {
//
//                Vector3 position = new Vector3(x * radiusBetweenSubWayPoint_ * 2 + mapCenter_.x - mapSize_.x * 0.5f, 0,
//                    y * radiusBetweenSubWayPoint_ * 2 + mapCenter_.y - mapSize_.y * 0.5f);
//                
//                Gizmos.DrawWireSphere(position, 0.5f);
//
//            }
//        }
    }
}
