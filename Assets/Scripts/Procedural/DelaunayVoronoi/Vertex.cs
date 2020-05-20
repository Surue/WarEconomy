using System.Collections.Generic;
using UnityEngine;

namespace Procedural {
public sealed class Vertex {
    public static readonly Vertex VERTEX_AT_INFINITY = new Vertex(float.NaN, float.NaN);
    
    static Stack<Vertex> pool_ = new Stack<Vertex>();

    static int nvertices = 0;

    Vector3 position_;

    public Vector2 Position => position_;

    public float X => position_.x;

    public float Z => position_.z;

    int vertexIndex_;

    public int VertexIndex => vertexIndex_;

    public Vertex(float x, float z) {
        Init(x, z);
    }

    static Vertex Create(float x, float z) {
        if (float.IsNaN(x) || float.IsNaN(z)) {
            return VERTEX_AT_INFINITY;
        }

        if (pool_.Count > 0) {
            return pool_.Pop().Init(x, z);
        } else {
            return new Vertex(x, z);
        }
    }

    Vertex Init(float x, float z) {
        position_ = new Vector3(x, 0, z);
        return this;
    }

    public void Dispose() {
        pool_.Push(this);
    }

    public void SetIndex() {
        vertexIndex_ = nvertices++;
    }

    public override string ToString() {
        return "Vertex (" + vertexIndex_ + ")";
    }

    public static Vertex Intersect(Halfedge halfedge0, Halfedge halfedge1) {
        Edge edge0, edge1, edge;
        Halfedge halfedge;
        float determinant, intersectionX, intersectionZ;
        bool rightOfSite;

        edge0 = halfedge0.edge;
        edge1 = halfedge1.edge;
        if(edge0 == null || edge1 == null) {
            return null;
        }

        if (edge0.RightSite == edge1.RightSite) {
            return null;
        }

        determinant = edge0.a * edge1.b - edge0.b * edge1.a;
        if (-1.03 - 10 < determinant && determinant < 1.0e-10) {
            return null; //edges are parallel
        }

        intersectionX = (edge0.c * edge1.b - edge1.c * edge0.b) / determinant;
        intersectionZ = (edge1.c * edge0.a - edge0.c * edge1.a) / determinant;

        if (Voronoi.CompareByZThenX(edge0.RightSite, edge1.RightSite) < 0) {
            halfedge = halfedge0;
            edge = edge0;
        } else {
            halfedge = halfedge1;
            edge = edge1;
        }

        rightOfSite = intersectionX >= edge.RightSite.X;

        if ((rightOfSite && halfedge.leftRight == Side.LEFT) ||
            (!rightOfSite && halfedge.leftRight == Side.RIGHT)) {
            return null;
        }

        return Create(intersectionX, intersectionZ);
    }
}
}
