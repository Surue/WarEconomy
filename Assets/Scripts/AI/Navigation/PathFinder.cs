using System.Collections.Generic;
using UnityEngine;

namespace AI {
public class PathFinder : MonoBehaviour {
    static PathFinder instance_;

    List<WayPoint> wayPoints_ = new List<WayPoint>();

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

    public void Reset() {
        wayPoints_ = new List<WayPoint>();
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
        
        return path;
    }

    List<WayPoint> GetPath(int startWayPointIndex, int endWayPointIndex) {
        List<int> openList = new List<int> {startWayPointIndex};
        List<int> closedList = new List<int>();
        
        float[] totalCost = new float[wayPoints_.Count];
        for (int i = 0; i < totalCost.Length; i++) {
            totalCost[i] = Mathf.Infinity;
        }
        totalCost[startWayPointIndex] = 0f;
        
        int[] cameFrom = new int[wayPoints_.Count];

        Vector3 endPosition = wayPoints_[endWayPointIndex].transform.position;
        
        while (openList.Count > 0) {
            //Sort by priority
            float smallestCost = Mathf.Infinity;
            int currentNodeIndex = 0;
            foreach (int index in openList) {
                if (!(totalCost[index] < smallestCost)) continue;
                
                smallestCost = totalCost[index];
                currentNodeIndex = index;
            }
            
            //Get the first one
            WayPoint currentWayPoint = wayPoints_[currentNodeIndex];
            openList.Remove(currentNodeIndex);
            
            closedList.Add(currentNodeIndex);
            
            //Get all neighbors
            foreach (Link neighbor in currentWayPoint.links_) {
                int indexNeighbor = neighbor.wayPointIndex;

                float newCost = totalCost[currentNodeIndex] + (neighbor.distance * neighbor.weight) +
                                Vector3.Distance(wayPoints_[indexNeighbor].transform.position, endPosition);

                if (!closedList.Contains(indexNeighbor) && totalCost[indexNeighbor] > newCost) {
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

        Debug.Log("===============================");
        foreach (int i in closedList) {
            Debug.Log(i);
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
}
}
