using System.Collections.Generic;
using UnityEngine;

namespace Procedural {
public enum VertexOrSite {
    VERTEX,
    SITE
}

sealed class EdgeReorderer {
    List<Edge> edges_;
    List<Side> edgeOrientation_;

    public List<Edge> Edges => edges_;

    public List<Side> EdgeOrientation => edgeOrientation_;

    public EdgeReorderer(List<Edge> edges, VertexOrSite criteria) {
        edges_ = new List<Edge>();
        edgeOrientation_ = new List<Side>();

        if (edges.Count > 0) {
            edges_ = ReorderEdges(edges, criteria);
        }
    }

    public void Dispose() {
        edges_ = null;
        edgeOrientation_ = null;
    }

    List<Edge> ReorderEdges(List<Edge> edges, VertexOrSite criteria) {
        int i;
        int n = edges.Count;

        Edge edge;
        bool[] done = new bool[n];
        int nDone = 0;
        for (int j = 0; j < n; j++) {
            done[j] = false;
        }
        
        List<Edge> newEdges = new List<Edge>();

        i = 0;
        edge = edges[i];
        newEdges.Add(edge);
        edgeOrientation_.Add(Side.LEFT);

        Vector3 firstPoint = (criteria == VertexOrSite.VERTEX) ? (Vector3)edge.LeftVertex.Position : edge.LeftSite.Position;
        Vector3 lastPoint = (criteria == VertexOrSite.VERTEX) ? (Vector3)edge.RightVertex.Position : edge.RightSite.Position;

        if (firstPoint == (Vector3)Vertex.VERTEX_AT_INFINITY.Position || lastPoint == (Vector3)Vertex.VERTEX_AT_INFINITY.Position) {
            return new List<Edge>();
        }

        done[i] = true;
        ++nDone;

        while (nDone < n) {
            for (i = 1; i < n; ++i) {
                if (done[i]) {
                    continue;
                }

                edge = edges[i];
                
                Vector3 leftPoint = (criteria == VertexOrSite.VERTEX) ? (Vector3)edge.LeftVertex.Position : edge.LeftSite.Position;
                Vector3 rightPoint = (criteria == VertexOrSite.VERTEX) ? (Vector3)edge.RightVertex.Position : edge.RightSite.Position;

                if (firstPoint == (Vector3)Vertex.VERTEX_AT_INFINITY.Position || lastPoint == (Vector3)Vertex.VERTEX_AT_INFINITY.Position) {
                    return new List<Edge>();
                }

                if (leftPoint == lastPoint) {
                    lastPoint = rightPoint;
                    edgeOrientation_.Add(Side.LEFT);
                    newEdges.Add(edge);
                    done[i] = true;
                } else if (rightPoint == firstPoint) {
                    firstPoint = leftPoint;
                    edgeOrientation_.Insert(0, Side.LEFT);
                    newEdges.Insert(0, edge);
                    done[i] = true;
                } else if (leftPoint == firstPoint) {
                    firstPoint = rightPoint;
                    edgeOrientation_.Insert(0, Side.RIGHT);
                    newEdges.Insert(0, edge);
                    done[i] = true;
                }else if (rightPoint == lastPoint) {
                    lastPoint = leftPoint;
                    edgeOrientation_.Add(Side.RIGHT);
                    newEdges.Add(edge);
                    done[i] = true;
                }

                if (done[i]) {
                    ++nDone;
                }
            }
        }

        return newEdges;
    }
}
}
