using System.Collections.Generic;
using UnityEngine;

namespace Procedural {
public sealed class Vertex {
    public static readonly Vertex VERTEX_AT_INFINITY = new Vertex(float.NaN, float.NaN);
    
    static Stack<Vertex> pool_ = new Stack<Vertex>();

    static int nbVertices_ = 0;

    Vector2 position_;

    public Vector2 Position => position_;

    public float X => position_.x;

    public float Y => position_.y;

    int vertexIndex_;

    public int VertexIndex => vertexIndex_;

    public Vertex(float x, float y) {
        Init(x, y);
    }

    static Vertex Create(float x, float y) {
        if (float.IsNaN(x) || float.IsNaN(y)) {
            return VERTEX_AT_INFINITY;
        }

        return pool_.Count > 0 ? pool_.Pop().Init(x, y) : new Vertex(x, y);
    }

    Vertex Init(float x, float y) {
        position_ = new Vector2(x, y);
        return this;
    }

    public void Dispose() {
        pool_.Push(this);
    }

    public void SetIndex() {
        vertexIndex_ = nbVertices_++;
    }

    public override string ToString() {
        return "Vertex (" + vertexIndex_ + ")";
    }

    public static Vertex Intersect(HalfEdge halfEdge0, HalfEdge halfEdge1) {
        Edge edge;
        HalfEdge halfEdge;
        bool rightOfSite;
		
        Edge edge0 = halfEdge0.edge;
        Edge edge1 = halfEdge1.edge;
        if (edge0 == null || edge1 == null) {
            return null;
        }
        if (edge0.RightSite == edge1.RightSite) {
            return null;
        }
		
        float determinant = edge0.a * edge1.b - edge0.b * edge1.a;
        
        // If the edges are parallel => return null.
        if (-1.0e-10 < determinant && determinant < 1.0e-10) {
            return null;
        }
		
        float intersectionX = (edge0.c * edge1.b - edge1.c * edge0.b) / determinant;
        float intersectionY = (edge1.c * edge0.a - edge0.c * edge1.a) / determinant;
		
        if (Voronoi.CompareByYThenX (edge0.RightSite, edge1.RightSite) < 0) {
            halfEdge = halfEdge0;
            edge = edge0;
        } else {
            halfEdge = halfEdge1;
            edge = edge1;
        }
        rightOfSite = intersectionX >= edge.RightSite.X;
        if (rightOfSite && halfEdge.leftRight == Side.LEFT || !rightOfSite && halfEdge.leftRight == Side.RIGHT) {
            return null;
        }
		
        return Create (intersectionX, intersectionY);
    }
}
}
