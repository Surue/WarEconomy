using System;
using System.Collections.Generic;
using Geometry;
using UnityEngine;

namespace Procedural {
public sealed class Edge {
    private static Stack<Edge> pool_ = new Stack<Edge>();

    public float a, b, c;

    static int nbEdges_ = 0;
    
    public static readonly Edge DELETED = new Edge();

    Dictionary<Side, Nullable<Vector3>> clippedVertices_;

    public Dictionary<Side, Nullable<Vector3>> clippedEnds => clippedVertices_;

    Dictionary<Side, Site> sites_;

    public Site LeftSite {
        get => sites_[Side.LEFT];
        set => sites_[Side.LEFT] = value;
    }
    
    public Site RightSite {
        get => sites_[Side.RIGHT];
        set => sites_[Side.RIGHT] = value;
    }

    int edgeIndex_;

    Vertex rightVertex_;

    public Vertex RightVertex => rightVertex_;
    Vertex leftVertex_;

    public Vertex LeftVertex => leftVertex_;

    public static Edge CreateBisectingEdge(Site site0, Site site1) {
        float dx, dz, absDx, absDy;
        float a, b, c;

        dx = site1.X - site0.X;
        dz = site1.Z - site0.Z;

        absDx = dx > 0 ? dx : -dx;
        absDy = dz > 0 ? dz : -dz;

        c = site0.X * dx + site0.Z * dz + (dz * dz + dz * dz) * 0.5f;

        if (absDx > absDy) {
            a = 1.0f;
            b = dz / dx;
            c /= dx;
        } else {
            b = 1.0f;
            a = dx / dz;
            c /= dz;
        }

        Edge edge = Edge.Create();

        edge.LeftSite = site0;
        edge.RightSite = site1;

        site0.AddEdge(edge);
        site1.AddEdge(edge);

        edge.leftVertex_ = null;
        edge.rightVertex_ = null;

        edge.a = a;
        edge.b = b;
        edge.c = c;

        return edge;
    }

    static Edge Create() {
        Edge edge;

        if (pool_.Count > 0) {
            edge = pool_.Pop();
            edge.Init();
        } else {
            edge = new Edge();
        }

        return edge;
    }

    public Segment DelaunaySegment() {
        return new Segment(LeftSite.Position, RightSite.Position);
    }

    public Segment VoronoiSegment() {
        if (!Visible) {
            return new Segment(null, null);
        }
        
        return new Segment(clippedVertices_[Side.LEFT], clippedVertices_[Side.RIGHT]);
    }

    public Vertex Vertex(Side leftRight) {
        return (leftRight == Side.LEFT) ? leftVertex_ : rightVertex_;
    }

    public void SetVertex(Side leftRight, Vertex v) {
        if (leftRight == Side.LEFT) {
            leftVertex_ = v;
        } else {
            rightVertex_ = v;
        }
    }

    public bool IsPartOfConvexHull() {
        return (leftVertex_ == null || rightVertex_ == null);
    }

    public float SitesDistance() {
        return Vector3.Distance(LeftSite.Position, RightSite.Position);
    }

    public static int CompareSitesDistanceMax(Edge edge0, Edge edge1) {
        float length0 = edge0.SitesDistance();
        float length1 = edge1.SitesDistance();
        if (length0 < length1) {
            return 1;
        }

        if (length0 > length1) {
            return -1;
        }

        return 0;
    }

    public static int CompareSitesDistances(Edge edge0, Edge edge1) {
        return -CompareSitesDistanceMax(edge0, edge1);
    }

    public bool Visible => clippedVertices_ != null;

    public Site Site(Side leftRight) {
        return sites_[leftRight];
    }

    public void Dispose() {
        leftVertex_ = null;
        rightVertex_ = null;

        if (clippedVertices_ != null) {
            clippedVertices_[Side.LEFT] = null;
            clippedVertices_[Side.RIGHT] = null;
            clippedVertices_ = null;
        }

        sites_[Side.LEFT] = null;
        sites_[Side.RIGHT] = null;
        sites_ = null;
        
        pool_.Push(this);
    }

    Edge() {
        edgeIndex_ = nbEdges_++;
        Init();
    }

    void Init() {
        sites_ = new Dictionary<Side, Site>();
    }

    public override string ToString() {
        return "Edge " + edgeIndex_.ToString () + "; sites " + sites_ [Side.LEFT].ToString () + ", " + sites_ [Side.RIGHT].ToString ()
               + "; endVertices " + ((leftVertex_ != null) ? leftVertex_.VertexIndex.ToString () : "null") + ", "
               + ((rightVertex_ != null) ? rightVertex_.VertexIndex.ToString () : "null") + "::";
    }

    public void ClipVertices(Rect bounds) {
        float xMin = bounds.xMin;
        float zMin = bounds.yMin;
        float xMax = bounds.xMax;
        float zMax = bounds.yMax;

        Vertex vertex0, vertex1;
        float x0, x1, z0, z1;

        if (a == 1.0f && b >= 0.0f) {
            vertex0 = rightVertex_;
            vertex1 = leftVertex_;
        } else {
            vertex0 = leftVertex_;
            vertex1 = rightVertex_;
        }

        if (a == 1.0) {
            z0 = zMin;
            if (vertex0 != null && vertex0.Z > zMin) {
                z0 = vertex0.Z;
            }

            if (z0 > zMax) {
                return;
            }

            x0 = c - b * z0;

            z1 = zMax;
            if (vertex1 != null && vertex1.Z < zMax) {
                z1 = vertex1.Z;
            }

            if (z1 < zMin) {
                return;
            }

            x1 = c - b * z1;

            if ((x0 > xMax && x1 > xMax) || (x0 < xMin && x1 < xMin)) {
                return;
            }

            if (x0 > xMax) {
                x0 = xMax;
                z0 = (c - x0) / b;
            } else if (x0 < xMin) {
                x0 = xMin;
                z0 = (c - x0) / b;
            }

            if (x1 > xMax) {
                x1 = xMax;
                z1 = (c - x1) / b;
            }else if (x1 < xMin) {
                x1 = xMin;
                z1 = (c - x1) / b;
            }
        } else {
            x0 = xMin;
            if (vertex0 != null && vertex0.X > xMin) {
                x0 = vertex0.X;
            }

            if (x0 > xMax) {
                return;
            }

            z0 = c - a * x0;

            x1 = xMax;
            if (vertex1 != null && vertex1.X < xMax) {
                x1 = vertex1.X;
            }

            if (x1 < xMin) {
                return;
            }

            z1 = c - a * x1;

            if ((z0 > zMax && z1 > zMax) || (z0 < zMin && z1 <zMin)) {
                return;
            }

            if (z0 > zMax) {
                z0 = zMax;
                x0 = (c - z0) / a;
            } else if (z0 < zMin) {
                z0 = zMin;
                x0 = (c - z0) / a;
            }

            if (z1 > zMax) {
                z1 = zMax;
                x1 = (c - z1) / a;
            }else if (z1 < zMin) {
                z1 = zMin;
                x1 = (c - z1) / a;
            }
        }
        
        clippedVertices_ = new Dictionary<Side, Nullable<Vector3>>();
        if (vertex0 == leftVertex_) {
            clippedVertices_[Side.LEFT] = new Vector3(x0, 0, z0);
            clippedVertices_[Side.RIGHT] = new Vector3(x1, 0, z1);
        } else {
            clippedVertices_[Side.RIGHT] = new Vector3(x0, 0, z0);
            clippedVertices_[Side.LEFT] = new Vector3(x1, 0, z1);
        }
    }
}
}
