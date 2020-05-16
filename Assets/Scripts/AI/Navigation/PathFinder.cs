using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AI {
public class PathFinder : MonoBehaviour {
    static PathFinder instance_;

    [SerializeField] WayPoint[] wayPoints_;

    float[] totalCost;

    int[] cameFrom;

    List<Vector3> lastPath_;
    
    public static PathFinder Instance {
        get => instance_;
    }

    void Awake() {
        if (instance_ == null) {
            instance_ = this;
        } else {
            Destroy(this);
        }
        
        WayPoint[] tmp = FindObjectsOfType<WayPoint>();

        wayPoints_ = new WayPoint[tmp.Length];
        
        foreach (WayPoint wayPoint in tmp) {
            wayPoints_[wayPoint.Index] = wayPoint;
        }
        
        totalCost = new float[wayPoints_.Length];
        
        cameFrom = new int[wayPoints_.Length];
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
        List<WayPoint> wayPointsPath = new List<WayPoint>();
        if (startWayPointIndex != endWayPointIndex) {
            wayPointsPath = GetPath(startWayPointIndex, endWayPointIndex);
        }

        //Build path
        List<Vector3> path = new List<Vector3> {startPosition};

        for (int i = wayPointsPath.Count - 1; i >= 0; i--) {
            path.Add(wayPointsPath[i].transform.position);
        }
        path.Add(endPosition);

        lastPath_ = path; //TODO REMOVE
        
        return path;
    }

    List<WayPoint> GetPath(int startWayPointIndex, int endWayPointIndex) {
        List<int> openList = new List<int> {startWayPointIndex};
        List<int> closedList = new List<int>();

        for (int index = 0; index < totalCost.Length; index++) {
            totalCost[index] = 0;
        }
        
        for (int index = 0; index < cameFrom.Length; index++) {
            cameFrom[index] = index;
        }

        Vector3 endPosition = wayPoints_[endWayPointIndex].transform.position;
        while (openList.Count > 0) {
            //Sort by priority
            float smallestCost = Mathf.Infinity;
            int currentNodeIndex = 0;
            foreach (int index in openList) {
                if (totalCost[index] > smallestCost) continue;
                
                smallestCost = totalCost[index];
                currentNodeIndex = index;
            }
            
            //Get the first one
            WayPoint currentWayPoint = wayPoints_[currentNodeIndex];
            openList.Remove(currentNodeIndex);
            
            closedList.Add(currentNodeIndex);
            
            //Get all neighbors
            for (int i = 0; i < currentWayPoint.Links.Count; i++) {
                int indexNeighbor = currentWayPoint.Links[i].wayPointIndex;

                float newCost = totalCost[currentNodeIndex] + (currentWayPoint.Links[i].distance * currentWayPoint.Links[i].weight) +
                                Vector3.Distance(wayPoints_[indexNeighbor].transform.position, endPosition) * 5f;
                
                if(closedList.Contains(indexNeighbor)) continue;

                if (totalCost[indexNeighbor] > newCost || totalCost[indexNeighbor] == 0) {
                    cameFrom[indexNeighbor] = currentNodeIndex;
                    totalCost[indexNeighbor] = newCost;

                    if (!openList.Contains(indexNeighbor)) {
                        openList.Add(indexNeighbor);
                    }
                }
            }

            if (currentNodeIndex == endWayPointIndex) {
                break;
            }
        }

        //Build path with WayPoint
        List<WayPoint> path = new List<WayPoint> {wayPoints_[endWayPointIndex]};
        
        int lastIndex = endWayPointIndex;
        do {
            path.Add(wayPoints_[cameFrom[lastIndex]]);

            if (cameFrom[lastIndex] == lastIndex) {
                Debug.Log("ERROR");
                break;
            }
            
            lastIndex = cameFrom[lastIndex];
        } while (lastIndex != startWayPointIndex);
        
        return path;
    }

    int FindClosestWayPointIndex(Vector3 position) {

        int result = 0;

        float minDistance = Mathf.Infinity;

        for (int index = 0; index < wayPoints_.Length; index++) {
            WayPoint wayPoint = wayPoints_[index];
            float distance = Vector3.Distance(wayPoint.transform.position, position);

            if (distance > minDistance) continue;
            minDistance = distance;
            result = index;
        }

        return result;
    }
    
    static float ManhattanDistance(Vector3 pos1, Vector3 pos2)
    {
        return Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y);
    }

    void OnDrawGizmos() {
        
    }
    
    void DrawArrow(Vector3 start, Vector3 end) {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(start, end);

        Vector3 dir = (end - start).normalized;
        
        Gizmos.DrawWireSphere(end, 0.1f);
    }
}
}
