using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] Vector2 sampleRegionSize_;
    [SerializeField] float minRadius_ = 1;
    [SerializeField] float maxRadius_ = 3;
    [SerializeField] int rejectionNumber_ = 30;

    [SerializeField] bool displayGrid_ = true;
    
    int[,] grid_;
    List<Vector3> points_;
    List<float> pointRadius_;
    float cellSize_;

    void Start() {
//        cellSize_ = radius_ / Mathf.Sqrt(2);
    }

    void OnValidate() {
        cellSize_ = minRadius_ / Mathf.Sqrt(2);
        //Build the grid 
        grid_ = new int[Mathf.CeilToInt(sampleRegionSize_.x / cellSize_), Mathf.CeilToInt(sampleRegionSize_.y / cellSize_)];

        points_ = new List<Vector3>();
        List<Vector3> spawnPoints = new List<Vector3>();
        pointRadius_ = new List<float>();
        
        //Center point
        spawnPoints.Add(new Vector3(sampleRegionSize_.x / 2, 0, sampleRegionSize_.y / 2));

        while (spawnPoints.Count > 0) {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
    
            Vector3 spawnCenter = spawnPoints[spawnIndex];

            bool candidateValid = false;

            for (int i = 0; i < rejectionNumber_; i++) {
                float angle = Random.value * Mathf.PI * 2;
                
                Vector3 dir = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));
                Vector3 candidate = spawnCenter + dir * Random.Range(minRadius_, 2 * minRadius_);
                float candidateRadius = Random.Range(minRadius_, maxRadius_);
                
                if (!IsValid(candidate, candidateRadius)) continue;
                //If the candidate is valid (it's not in a radius of another object)
                points_.Add(candidate);
                pointRadius_.Add(candidateRadius);
                spawnPoints.Add(candidate);
                grid_[(int)(candidate.x / cellSize_), (int)(candidate.z / cellSize_)] = points_.Count; //Index of the added point
                candidateValid = true;
                break;
            }
            
            //If the candidate is invalid, then remove the spawn point
            if (!candidateValid) {
                spawnPoints.RemoveAt(spawnIndex);
            }
        }
    }

    bool IsValid(Vector3 candidate, float candidateRadius) {
        if (candidate.x < 0 || candidate.x >= sampleRegionSize_.x || candidate.z < 0 || candidate.z >= sampleRegionSize_.y) return false;
        
        //Cell currentPosition
        int cellX = (int)(candidate.x / cellSize_);
        int cellZ = (int)(candidate.z / cellSize_);

        int offset = Mathf.FloorToInt(maxRadius_ * 2);
    
        //Goes from a 5 by 5 square around the position
        int searchStartX = Mathf.Max(0, cellX - offset);
        int searchEndX = Mathf.Min(cellX + offset, grid_.GetLength(0) - 1);
        
        int searchStartY = Mathf.Max(0, cellZ - offset);
        int searchEndY = Mathf.Min(cellZ + offset, grid_.GetLength(1) - 1);
        
        //Loop through each square adjacent
        for (int x = searchStartX; x <= searchEndX; x++) {
            for (int y = searchStartY; y <= searchEndY; y++) {
                
                int pointIndex = grid_[x, y] - 1;
                
                //If the point index has no assignation yet (by default the value == 0 then minus 1 goes to == -1)
                if (pointIndex == -1) continue;
                
                //Check if the square distance between the point in the grid and the candidate is valid
                float dist = (candidate - points_[pointIndex]).sqrMagnitude;
                    
                if (dist < Mathf.Pow((candidateRadius + pointRadius_[pointIndex]) / 2, 2)) {
                    return false;
                }
            }
        }
        
        return true;
    }

    void OnDrawGizmos() {
        if (points_ == null || points_.Count <= 0) return;
        
        Gizmos.DrawWireCube(new Vector3(sampleRegionSize_.x/2.0f, 0, sampleRegionSize_.y/2.0f), new Vector3(sampleRegionSize_.x/2.0f, 0, sampleRegionSize_.y/2.0f));

        for (int index = 0; index < points_.Count; index++) {
            Vector3 pos = points_[index];
            float radius = pointRadius_[index];
            Handles.DrawSolidDisc(pos, Vector3.up, radius * 0.5f);
        }

        if (!displayGrid_) return;

        Gizmos.color = Color.red;
        for (int x = 0; x < grid_.GetLength(0); x++) {
            for (int y = 0; y < grid_.GetLength(1); y++) {
                if (grid_[x, y] == 0) {
                    Gizmos.DrawWireCube(
                        new Vector3(x * cellSize_ + cellSize_ * 0.5f, 0, y * cellSize_ + cellSize_ * 0.5f),
                        new Vector3(cellSize_, 0, cellSize_));
                    Gizmos.DrawLine(
                        new Vector3(x * cellSize_, 0, y * cellSize_),
                        new Vector3(x * cellSize_ + cellSize_, 0, y * cellSize_ + cellSize_));
                    
                    Gizmos.DrawLine(
                        new Vector3(x * cellSize_ + cellSize_, 0, y * cellSize_),
                        new Vector3(x * cellSize_, 0, y * cellSize_ + cellSize_));
                }
            }
        }
        
        Gizmos.color = Color.white;
        for (int x = 0; x < grid_.GetLength(0); x++) {
            for (int y = 0; y < grid_.GetLength(1); y++) {
                if(grid_[x,y] != 0)
                    Gizmos.DrawWireCube(new Vector3(x * cellSize_ + cellSize_ * 0.5f, 0, y * cellSize_ + cellSize_ * 0.5f), new Vector3(cellSize_, 0, cellSize_));
            }
        }
    }
}
