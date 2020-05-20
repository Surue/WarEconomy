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

        if (pool_.Count > 0) {
            return pool_.Pop().Init(x, y);
        } else {
            return new Vertex(x, y);
        }
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

    public static Vertex Intersect(Halfedge halfedge0, Halfedge halfedge1) {
        Edge edge0, edge1, edge;
        Halfedge halfedge;
        float determinant, intersectionX, intersectionY;
        bool rightOfSite;
		
        edge0 = halfedge0.edge;
        edge1 = halfedge1.edge;
        if (edge0 == null || edge1 == null) {
            return null;
        }
        if (edge0.RightSite == edge1.RightSite) {
            return null;
        }
		
        determinant = edge0.a * edge1.b - edge0.b * edge1.a;
        if (-1.0e-10 < determinant && determinant < 1.0e-10) {
            // the edges are parallel
            return null;
        }
		
        intersectionX = (edge0.c * edge1.b - edge1.c * edge0.b) / determinant;
        intersectionY = (edge1.c * edge0.a - edge0.c * edge1.a) / determinant;
		
        if (Voronoi.CompareByYThenX (edge0.RightSite, edge1.RightSite) < 0) {
            halfedge = halfedge0;
            edge = edge0;
        } else {
            halfedge = halfedge1;
            edge = edge1;
        }
        rightOfSite = intersectionX >= edge.RightSite.X;
        if ((rightOfSite && halfedge.leftRight == Side.LEFT)
            || (!rightOfSite && halfedge.leftRight == Side.RIGHT)) {
            return null;
        }
		
        return Vertex.Create (intersectionX, intersectionY);
    }
}
}
