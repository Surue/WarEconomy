using System.Collections.Generic;
using UnityEngine;

namespace Procedural {
public class PoissonPoint {
    public Vector3 position;
    public float radius;
}

public class PoissonDisk {
    public static List<PoissonPoint> Generate(Vector2 sampleRegionSize, float minRadius, float maxRadius, int rejectionNumber) {
        float cellSize = minRadius / Mathf.Sqrt(2);
        //Build the grid 
        int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];
        Vector2Int gridSize = new Vector2Int(grid.GetLength(0), grid.GetLength(1));
        
        int cellOffset = Mathf.FloorToInt(maxRadius * 2);
        
        List<PoissonPoint> poissonPoints = new List<PoissonPoint>();
        List<Vector3> spawnPoints = new List<Vector3> {new Vector3(sampleRegionSize.x / 2, 0, sampleRegionSize.y / 2)};

        //Center point

        while (spawnPoints.Count > 0) {
            int spawnIndex = Random.Range(0, spawnPoints.Count);

            Vector3 spawnCenter = spawnPoints[spawnIndex];

            bool candidateValid = false;

            for (int i = 0; i < rejectionNumber; i++) {
                float angle = Random.value * Mathf.PI * 2;

                Vector3 dir = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
                Vector3 candidate = spawnCenter + dir * Random.Range(minRadius, 2 * minRadius);
                float candidateRadius = Random.Range(minRadius, maxRadius);
                /**/
                if (candidate.x < 0 || candidate.x >= sampleRegionSize.x || candidate.z < 0 || candidate.z >= sampleRegionSize.y) continue;

                //Cell currentPosition
                int cellX = (int) (candidate.x / cellSize);
                int cellZ = (int) (candidate.z / cellSize);

                //Goes from a 5 by 5 square around the position
                int searchStartX = Mathf.Max(0, cellX - cellOffset);
                int searchEndX = Mathf.Min(cellX + cellOffset, gridSize.x - 1);

                int searchStartY = Mathf.Max(0, cellZ - cellOffset);
                int searchEndY = Mathf.Min(cellZ + cellOffset, gridSize.y - 1);

                //Loop through each square adjacent
                bool collide = false;
                for (int x = searchStartX; x <= searchEndX; x++) {
                    for (int y = searchStartY; y <= searchEndY; y++) {

                        int pointIndex = grid[x, y] - 1;

                        //If the point index has no assignation yet (by default the value == 0 then minus 1 goes to == -1)
                        if (pointIndex == -1) continue;

                        //Check if the square distance between the point in the grid and the candidate is valid
                        float dist = (candidate - poissonPoints[pointIndex].position).sqrMagnitude;

                        if (dist < Mathf.Pow((candidateRadius + poissonPoints[pointIndex].radius) / 2, 2)) {
                            collide = true;
                            break;
                        }
                    }

                    if (collide) {
                        break;
                    }
                }
                
                if(collide) { continue; }


                //If the candidate is valid (it's not in a radius of another object)
                poissonPoints.Add(new PoissonPoint{position = candidate, radius = candidateRadius});
                spawnPoints.Add(candidate);
                grid[(int) (candidate.x / cellSize), (int) (candidate.z / cellSize)] = poissonPoints.Count; //Index of the added point
                candidateValid = true;
                break;
            }

            //If the candidate is invalid, then remove the spawn point
            if (!candidateValid) {
                spawnPoints.RemoveAt(spawnIndex);
            }
        }

        return poissonPoints;
    }
}
}
