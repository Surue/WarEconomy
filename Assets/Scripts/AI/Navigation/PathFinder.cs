using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.Assertions.Must;

namespace AI {
public class PathFinder : MonoBehaviour {
    static PathFinder instance_;

    List<WayPoint> wayPoints_;

    public static PathFinder Instance {
        get => instance_;
    }

    void Awake() {
        if (instance_ == null) {
            instance_ = this;
        } else {
            Destroy(this);
        }
    }

    public void RegisterWayPoint(WayPoint wayPoint) {
        wayPoints_.Add(wayPoint);
    }

    public void UnregisterWayPoint(WayPoint wayPoint) {
        wayPoints_.Remove(wayPoint);
    }
    
    public List<Vector3> GetPath(Vector3 startPosition, Vector3 endPosition) {
        return FindPath(startPosition, endPosition);
    }

    List<Vector3> FindPath(Vector3 startPosition, Vector3 endPosition) {
        //Find starting waypoint
        int startWayPointIndex = FindClosestWayPointIndex(startPosition);
        
        //Find end waypoint
        int endWayPointIndex = FindClosestWayPointIndex(endPosition);
        
        //Find all path
        List<WayPoint> wayPointsPath = GetPath(startWayPointIndex, endWayPointIndex);

        //Build path
        List<Vector3> path = new List<Vector3> {endPosition};

        for (int i = wayPointsPath.Count - 1; i >= 0; i--) {
            path.Add(wayPointsPath[i].transform.position);
        }
        path.Add(startPosition);
        
        return path;
    }

    List<WayPoint> GetPath(int startWayPointIndex, int endWayPointIndex) {
        List<int> openList = new List<int> {startWayPointIndex};
        List<int> closedList = new List<int>();

        List<float> openCost = new List<float> {0};
        
        float[] totalCost = new float[wayPoints_.Count];

        Dictionary<int, int> cameFrom = new Dictionary<int, int>();

        Vector3 endPosition = wayPoints_[endWayPointIndex].transform.position;

        while (openList.Count > 0) {
            //Sort by priority
            float smallestCost = Mathf.Infinity;
            int index = 0;
            for (int i = 0; i < openCost.Count; i++) {
                if (!(openCost[i] < smallestCost)) continue;
                
                smallestCost = openCost[i];
                index = i;
            }
            
            //Get the first one
            WayPoint currentWayPoint = wayPoints_[openList[index]];
            openList.RemoveAt(index);
            
            closedList.Add(index);
            
            //Get all neighbors
            foreach (Link neighbor in currentWayPoint.neighbors) {
                int indexNeighbor = 0;
                for (int i = 0; i < wayPoints_.Count; i++) {
                    if (wayPoints_[i] == neighbor.wayPoint) {
                        indexNeighbor = i;
                    }
                }

                float newCost = totalCost[index] + (neighbor.distance * neighbor.weight) +
                                Vector3.Distance(wayPoints_[indexNeighbor].transform.position, endPosition);

                if (!closedList.Contains(indexNeighbor) || totalCost[indexNeighbor] > 0.0f && totalCost[indexNeighbor] < newCost) {
                    cameFrom[indexNeighbor] = index;
                    totalCost[indexNeighbor] = newCost;

                    if (!openList.Contains(indexNeighbor)) {
                        openList.Add(indexNeighbor);
                    }
                }
            }

            if (index == endWayPointIndex) {
                break;
            }
        }
        
        //Build path with WayPoint
        List<WayPoint> path = new List<WayPoint> {wayPoints_[endWayPointIndex]};
        
        int lastIndex = endWayPointIndex;
        do {
            path.Add(wayPoints_[cameFrom[lastIndex]]);

            lastIndex = cameFrom[lastIndex];
        } while (lastIndex != startWayPointIndex);
        
        return path;
    }

    int FindClosestWayPointIndex(Vector3 position) {

        int result = 0;

        float minDistance = Mathf.Infinity;

        for (int index = 0; index < wayPoints_.Count; index++) {
            WayPoint wayPoint = wayPoints_[index];
            float distance = ManhattanDistance(wayPoint.transform.position, position);

            if (!(minDistance < distance)) continue;
            minDistance = distance;
            result = index;
        }

        return result;
    }
    
    static float ManhattanDistance(Vector3 pos1, Vector3 pos2)
    {
        return Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y);
    }
}
}
