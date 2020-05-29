using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Pathfinding : MonoBehaviour
{
    struct PathNode {
        public int x;
        public int y;
        
        public int index;

        public int moveCost;
        public int heuristicCost;
        public int totalCost;

        public bool isWalkable;

        public int cameFromIndex;

        public void CalculateTotalCost() {
            totalCost = moveCost + heuristicCost;
        }
    }

    const int INVALID_INDEX = -1;
    const int MOVE_STRAIGHT_COST = 10;
    const int MOVE_DIAGONAL_COST = 14;

    void Start() {
        NativeArray<JobHandle> jobHandlesArray = new NativeArray<JobHandle>(5, Allocator.Temp);
        for (int i = 0; i < 5; i++) {
            FindPathJob findPathJob = new FindPathJob {
                startPos = new int2(0, 0),
                endPos = new int2(3, 1)
            };
            jobHandlesArray[i] = findPathJob.Schedule();
        }

        JobHandle.CompleteAll(jobHandlesArray);
        jobHandlesArray.Dispose();
    }

    [BurstCompile]
    struct FindPathJob : IJob {

        public int2 startPos;
        public int2 endPos;
        
        public void Execute() {
            int2 gridSize = new int2(4, 4);
        
            NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.Temp);

            for (int x = 0; x < gridSize.x; x++) {
                for (int y = 0; y < gridSize.y; y++) {
                    PathNode node = new PathNode();
                    node.x = x;
                    node.y = y;

                    node.index = CalculateIndex(x, y, gridSize.x);

                    node.moveCost = int.MaxValue;
                    node.heuristicCost = CalculateHeuristicCost(new int2(x, y), endPos);
                    node.CalculateTotalCost();

                    node.isWalkable = true;

                    node.cameFromIndex = INVALID_INDEX;

                    pathNodeArray[node.index] = node;
                }
            }

            NativeArray<int2> neighborsOffset = new NativeArray<int2>(8, Allocator.Temp) {
                [0] = new int2(-1, 0),
                [1] = new int2(+1, 0),
                [2] = new int2(0, -1),
                [3] = new int2(0, +1),
                [4] = new int2(-1, -1),
                [5] = new int2(-1, +1),
                [6] = new int2(+1, -1),
                [7] = new int2(+1, +1)
            };
            
            int endIndex = CalculateIndex(endPos.x, endPos.y, gridSize.x);
            PathNode startNode = pathNodeArray[CalculateIndex(startPos.x, startPos.y, gridSize.x)];
            startNode.moveCost = 0;
            startNode.CalculateTotalCost();
            pathNodeArray[startNode.index] = startNode;
            
            NativeList<int> openList = new NativeList<int>(Allocator.Temp);
            NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

            openList.Add(startNode.index);

            while (openList.Length > 0) {
                int currentIndex = GetLowestCostNodeIndex(openList, pathNodeArray);
                PathNode currentNode = pathNodeArray[currentIndex];

                if (currentIndex == endIndex) {
                    break;
                }

                for (int i = 0; i < openList.Length; i++) {
                    if (openList[i] == currentIndex) {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }
                }

                closedList.Add(currentIndex);
                
                int2 currentPosition = new int2(currentNode.x, currentNode.y);

                for (int i = 0; i < neighborsOffset.Length; i++) {
                    int2 neighborOffset = neighborsOffset[i];
                    int2 neighborPosition = new int2(currentNode.x + neighborOffset.x, currentNode.y + neighborOffset.y);

                    if (!IsPositionInsideGrid(neighborPosition, gridSize)) {
                        continue;
                    }

                    int neighborIndex = CalculateIndex(neighborPosition.x, neighborPosition.y, gridSize.x);

                    if (closedList.Contains(neighborIndex)) {
                        continue;
                    }

                    PathNode neighborNode = pathNodeArray[neighborIndex];
                    if (!neighborNode.isWalkable) {
                        continue;
                    }

                    int tentativeMoveCost = currentNode.moveCost + CalculateHeuristicCost(currentPosition, neighborPosition);
                    if (tentativeMoveCost < neighborNode.moveCost) {
                        neighborNode.cameFromIndex = currentIndex;
                        neighborNode.moveCost = tentativeMoveCost;
                        neighborNode.CalculateTotalCost();
                        pathNodeArray[neighborIndex] = neighborNode;

                        if (!openList.Contains(neighborIndex)) {
                            openList.Add(neighborIndex);
                        }
                    }
                }
            } 

            PathNode endNode = pathNodeArray[endIndex];
            if (endNode.cameFromIndex == INVALID_INDEX) {
                Debug.Log("Path not founded");
            } else {
                NativeList<int2> path = CalculatePath(pathNodeArray, endNode);
                
                path.Dispose();
            }
            
            openList.Dispose();
            closedList.Dispose();

            neighborsOffset.Dispose();
             
            pathNodeArray.Dispose();
        }
        
        NativeList<int2> CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode) {
            if (endNode.cameFromIndex == INVALID_INDEX) {
                return new NativeList<int2>(Allocator.Temp);
            } else {
                NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
                path.Add(new int2(endNode.x, endNode.y));

                PathNode currentNode = endNode;
                while (currentNode.cameFromIndex != INVALID_INDEX) {
                    PathNode cameFromNode = pathNodeArray[currentNode.cameFromIndex];
                    path.Add(new int2(cameFromNode.x, cameFromNode.y));
                    currentNode = cameFromNode;
                }

                return path;
            }
        }

        bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize) {
            return 
                gridPosition.x >= 0 &&
                gridPosition.x < gridSize.x && 
                gridPosition.y >= 0 && 
                gridPosition.y < gridSize.y;
        }

        int CalculateHeuristicCost(int2 pos0, int2 pos1) {
            int xDistance = math.abs(pos0.x - pos1.x);
            int yDistance = math.abs(pos0.y - pos1.y);

            int remaining = math.abs(xDistance - yDistance);

            return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
        }

        int CalculateIndex(int x, int y, int gridWidth) {
            return x + y * gridWidth;
        }

        int GetLowestCostNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray) {
            PathNode lowestNode = pathNodeArray[openList[0]];

            for (int i = 1; i < openList.Length; i++) {
                PathNode tmp = pathNodeArray[openList[i]];
                if (tmp.totalCost < lowestNode.totalCost) {
                    lowestNode = tmp;
                }
            }

            return lowestNode.index;
        }
    }

    public void FindPath(int2 startPos, int2 endPosition) {
        int2 gridSize = new int2(4, 4);
        
        NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.Temp);

        for (int x = 0; x < gridSize.x; x++) {
            for (int y = 0; y < gridSize.y; y++) {
                PathNode node = new PathNode();
                node.x = x;
                node.y = y;

                node.index = CalculateIndex(x, y, gridSize.x);

                node.moveCost = int.MaxValue;
                node.heuristicCost = CalculateHeuristicCost(new int2(x, y), endPosition);
                node.CalculateTotalCost();

                node.isWalkable = true;

                node.cameFromIndex = INVALID_INDEX;

                pathNodeArray[node.index] = node;
            }
        }

        NativeArray<int2> neighborsOffset = new NativeArray<int2>(new int2[] {
            new int2(-1,  0),
            new int2(+1,  0),
            new int2( 0, -1),
            new int2( 0, +1),
            new int2(-1, -1),
            new int2(-1, +1),
            new int2(+1, -1),
            new int2(+1, +1),
            
        }, Allocator.Temp);
        
        int endIndex = CalculateIndex(endPosition.x, endPosition.y, gridSize.x);
        PathNode startNode = pathNodeArray[CalculateIndex(startPos.x, startPos.y, gridSize.x)];
        startNode.moveCost = 0;
        startNode.CalculateTotalCost();
        pathNodeArray[startNode.index] = startNode;
        
        NativeList<int> openList = new NativeList<int>(Allocator.Temp);
        NativeList<int> closedList = new NativeList<int>(Allocator.Temp);

        openList.Add(startNode.index);

        while (openList.Length > 0) {
            int currentIndex = GetLowestCostNodeIndex(openList, pathNodeArray);
            PathNode currentNode = pathNodeArray[currentIndex];

            if (currentIndex == endIndex) {
                break;
            }

            for (int i = 0; i < openList.Length; i++) {
                if (openList[i] == currentIndex) {
                    openList.RemoveAtSwapBack(i);
                    break;
                }
            }

            closedList.Add(currentIndex);
            
            int2 currentPosition = new int2(currentNode.x, currentNode.y);

            for (int i = 0; i < neighborsOffset.Length; i++) {
                int2 neighborOffset = neighborsOffset[i];
                int2 neighborPosition = new int2(currentNode.x + neighborOffset.x, currentNode.y + neighborOffset.y);

                if (!IsPositionInsideGrid(neighborPosition, gridSize)) {
                    continue;
                }

                int neighborIndex = CalculateIndex(neighborPosition.x, neighborPosition.y, gridSize.x);

                if (closedList.Contains(neighborIndex)) {
                    continue;
                }

                PathNode neighborNode = pathNodeArray[neighborIndex];
                if (!neighborNode.isWalkable) {
                    continue;
                }

                int tentativeMoveCost = currentNode.moveCost + CalculateHeuristicCost(currentPosition, neighborPosition);
                if (tentativeMoveCost < neighborNode.moveCost) {
                    neighborNode.cameFromIndex = currentIndex;
                    neighborNode.moveCost = tentativeMoveCost;
                    neighborNode.CalculateTotalCost();
                    pathNodeArray[neighborIndex] = neighborNode;

                    if (!openList.Contains(neighborIndex)) {
                        openList.Add(neighborIndex);
                    }
                }
            }
        } 

        PathNode endNode = pathNodeArray[endIndex];
        if (endNode.cameFromIndex == INVALID_INDEX) {
            Debug.Log("Path not founded");
        } else {
            NativeList<int2> path = CalculatePath(pathNodeArray, endNode);

            foreach (int2 pos in path) {
                Debug.Log(pos);
            }
            
            path.Dispose();
        }
        
        openList.Dispose();
        closedList.Dispose();

        neighborsOffset.Dispose();
         
        pathNodeArray.Dispose();
    }

    NativeList<int2> CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode) {
        if (endNode.cameFromIndex == INVALID_INDEX) {
            return new NativeList<int2>(Allocator.Temp);
        } else {
            NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
            path.Add(new int2(endNode.x, endNode.y));

            PathNode currentNode = endNode;
            while (currentNode.cameFromIndex != INVALID_INDEX) {
                PathNode cameFromNode = pathNodeArray[currentNode.cameFromIndex];
                path.Add(new int2(cameFromNode.x, cameFromNode.y));
                currentNode = cameFromNode;
            }

            return path;
        }
    }

    bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize) {
        return 
            gridPosition.x >= 0 &&
            gridPosition.x < gridSize.x && 
            gridPosition.y >= 0 && 
            gridPosition.y < gridSize.y;
    }

    int CalculateHeuristicCost(int2 pos0, int2 pos1) {
        int xDistance = math.abs(pos0.x - pos1.x);
        int yDistance = math.abs(pos0.y - pos1.y);

        int remaining = math.abs(xDistance - yDistance);

        return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    int CalculateIndex(int x, int y, int gridWidth) {
        return x + y * gridWidth;
    }

    int GetLowestCostNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray) {
        PathNode lowestNode = pathNodeArray[openList[0]];

        for (int i = 1; i < openList.Length; i++) {
            PathNode tmp = pathNodeArray[openList[i]];
            if (tmp.totalCost < lowestNode.totalCost) {
                lowestNode = tmp;
            }
        }

        return lowestNode.index;
    }
}
