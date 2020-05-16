using System.Collections.Generic;
using Procedural;
using UnityEditor;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Poisson disk")]
    [SerializeField] Vector2 sampleRegionSize_;
    [SerializeField] float minRadius_ = 1;
    [SerializeField] float maxRadius_ = 3;
    [SerializeField] int rejectionNumber_ = 30;

    [SerializeField] bool displayGrid_ = true;
    
    List<Procedural.PoissonPoint> points_;
    
    void OnValidate() {
        points_ = PoissonDisk.Generate(sampleRegionSize_, minRadius_, maxRadius_, rejectionNumber_);
    }

    void OnDrawGizmos() {
        Gizmos.DrawWireCube(new Vector3(sampleRegionSize_.x/2.0f, 0, sampleRegionSize_.y/2.0f), new Vector3(sampleRegionSize_.x/2.0f, 0, sampleRegionSize_.y/2.0f));
        
        if (points_ == null || points_.Count <= 0) return;
        

        foreach (PoissonPoint t in points_) {
            Handles.DrawSolidDisc(t.position, Vector3.up, t.radius * 0.5f);
        }

        if (!displayGrid_) return;
        
        float cellSize = minRadius_ / Mathf.Sqrt(2);

        Vector2Int gridSize = new Vector2Int(Mathf.CeilToInt(sampleRegionSize_.x / cellSize), Mathf.CeilToInt(sampleRegionSize_.y / cellSize));
        
        Gizmos.color = Color.white;
        for (int x = 0; x < gridSize.x; x++) {
            for (int y = 0; y < gridSize.y; y++) {
                    Gizmos.DrawWireCube(new Vector3(x * cellSize + cellSize * 0.5f, 0, y * cellSize + cellSize * 0.5f), new Vector3(cellSize, 0, cellSize));
            }
        }
    }
}
