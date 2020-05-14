using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;
using UnityEngine;

public class WayPointGraphBuilder : MonoBehaviour {
    [SerializeField] bool setup = false;

    void OnValidate() {
        setup = false;

        List<WayPoint> wayPoints = FindObjectsOfType<WayPoint>().ToList();

        PathFinder pathFinder = FindObjectOfType<PathFinder>();

        pathFinder.Reset();

        for (int i = 0; i < wayPoints.Count; i++) {
            wayPoints[i].Reset(i, pathFinder);
        }
        
        for (int i = 0; i < wayPoints.Count; i++) {
            wayPoints[i].ComputeLinks();
        }
    }
}
