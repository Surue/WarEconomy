using System;
using System.Collections;
using System.Collections.Generic;
using Geometry;
using Procedural;
using UnityEngine;
using Random = UnityEngine.Random;

public class VoronoiDemo : MonoBehaviour {
    [SerializeField] int pointCount_ = 300;

    List<Vector3> points_;
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
        
        points_ = new List<Vector3>();

        for (int i = 0; i < pointCount_; i++) {
            colors.Add(0);
            points_.Add(new Vector3(Random.Range(0, mapWidth_), 0, Random.Range(0, mapHeight_)));
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
                Vector3 left = (Vector3)edges_[i].p0;
                Vector3 right = (Vector3)edges_[i].p1;
                
                Gizmos.DrawLine(left, right);
            }
        }
        
        Gizmos.color = Color.magenta;
        if (delaunayTriangulation_ != null) {
            for (int i = 0; i < delaunayTriangulation_.Count; i++) {
                Vector3 left = (Vector3)delaunayTriangulation_[i].p0;
                Vector3 right = (Vector3)delaunayTriangulation_[i].p1;
                
                Gizmos.DrawLine(left, right);
            }
        }

        if (spanningTree_ != null) {
            Gizmos.color = Color.green;
            for (int i = 0; i < spanningTree_.Count; i++) {
                Segment segment = spanningTree_[i];
                
                Vector3 left = (Vector3)segment.p0;
                Vector3 right = (Vector3)segment.p1;
                
                Gizmos.DrawLine(left, right);
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(0, 0, 0), new Vector3(0, 0, mapHeight_) );
        Gizmos.DrawLine(new Vector3(0, 0, 0), new Vector3(mapWidth_, 0, 0) );
        Gizmos.DrawLine(new Vector3(mapWidth_, 0, 0), new Vector3(mapWidth_, 0, mapHeight_) );
        Gizmos.DrawLine(new Vector3(0, 0, mapHeight_), new Vector3(mapWidth_, 0, mapHeight_) );
    }
}
