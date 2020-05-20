using System.Collections.Generic;
using Geometry;
using Procedural;
using UnityEngine;
using Random = UnityEngine.Random;

public class VoronoiDemo : MonoBehaviour {
    [SerializeField] int pointCount_ = 300;

    List<Vector2> points_;
    float mapWidth_ = 100;
    float mapHeight_ = 50;
    List<Segment> edges_ = null;
    List<Segment> spanningTree_;
    List<Segment> delaunayTriangulation_;

    void Awake() {
        Demo();
    }

    void Update() {
        if (Input.anyKeyDown) {
            Demo();
        }
    }

    void Demo() {
        List<uint> colors = new List<uint>();
        
        points_ = new List<Vector2>();

        for (int i = 0; i < pointCount_; i++) {
            colors.Add(0);
            points_.Add(new Vector2(Random.Range(0, mapWidth_), Random.Range(0, mapHeight_)));
        }

        Voronoi voronoi = new Voronoi(points_, colors, new Rect(0, 0, mapWidth_, mapHeight_));
        edges_ = voronoi.VoronoiDiagram();

        spanningTree_ = voronoi.SpanningTree(KruskalType.MINIMUM);
        delaunayTriangulation_ = voronoi.DelaunayTriangulation();
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        if (points_ != null) {
            for (int i = 0; i < points_.Count; i++) {
                Gizmos.DrawSphere(points_[i], 0.2f);
            }
        }

        if (edges_ != null) {
            Gizmos.color = Color.white;
            for (int i = 0; i < edges_.Count; i++) {
                Vector2 left = (Vector2)edges_[i].p0;
                Vector2 right = (Vector2)edges_[i].p1;
                
                Gizmos.DrawLine(left, right);
            }
        }
        
        Gizmos.color = Color.magenta;
        if (delaunayTriangulation_ != null) {
            for (int i = 0; i < delaunayTriangulation_.Count; i++) {
                Vector2 left = (Vector2)delaunayTriangulation_[i].p0;
                Vector2 right = (Vector2)delaunayTriangulation_[i].p1;
                
                Gizmos.DrawLine(left, right);
            }
        }

        if (spanningTree_ != null) {
            Gizmos.color = Color.green;
            for (int i = 0; i < spanningTree_.Count; i++) {
                Segment segment = spanningTree_[i];
                
                Vector2 left = (Vector2)segment.p0;
                Vector2 right = (Vector2)segment.p1;
                
                Gizmos.DrawLine(left, right);
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector2(0, 0), new Vector2(0, mapHeight_) );
        Gizmos.DrawLine(new Vector2(0, 0), new Vector2(mapWidth_, 0) );
        Gizmos.DrawLine(new Vector2(mapWidth_, 0), new Vector2(mapWidth_, mapHeight_) );
        Gizmos.DrawLine(new Vector2(0, mapHeight_), new Vector2(mapWidth_, mapHeight_) );
    }
}
