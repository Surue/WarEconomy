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

    Dictionary<Side, Nullable<Vector2>> clippedVertices_;

    public Dictionary<Side, Nullable<Vector2>> clippedEnds => clippedVertices_;

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
        float dx, dy, absdx, absdy;
        float a, b, c;

        dx = site1.X - site0.X;
        dy = site1.Y - site0.Y;
        absdx = dx > 0 ? dx : -dx;
        absdy = dy > 0 ? dy : -dy;
        c = site0.X * dx + site0.Y * dy + (dx * dx + dy * dy) * 0.5f;
        if (absdx > absdy) {
            a = 1.0f;
            b = dy / dx;
            c /= dx;
        } else {
            b = 1.0f;
            a = dx / dy;
            c /= dy;
        }

        Edge edge = Create();

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
        return Vector2.Distance(LeftSite.Position, RightSite.Position);
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
        return "Edge " + edgeIndex_.ToString() + "; sites " + sites_[Side.LEFT].ToString() + ", " +
               sites_[Side.RIGHT].ToString()
               + "; endVertices " + ((leftVertex_ != null) ? leftVertex_.VertexIndex.ToString() : "null") + ", "
               + ((rightVertex_ != null) ? rightVertex_.VertexIndex.ToString() : "null") + "::";
    }

    public void ClipVertices(Rect bounds) {
        float xmin = bounds.xMin;
        float ymin = bounds.yMin;
        float xmax = bounds.xMax;
        float ymax = bounds.yMax;

        Vertex vertex0, vertex1;
        float x0, x1, y0, y1;

        if (a == 1.0 && b >= 0.0) {
            vertex0 = rightVertex_;
            vertex1 = leftVertex_;
        } else {
            vertex0 = leftVertex_;
            vertex1 = rightVertex_;
        }

        if (a == 1.0) {
            y0 = ymin;
            if (vertex0 != null && vertex0.Y > ymin) {
                y0 = vertex0.Y;
            }

            if (y0 > ymax) {
                return;
            }

            x0 = c - b * y0;

            y1 = ymax;
            if (vertex1 != null && vertex1.Y < ymax) {
                y1 = vertex1.Y;
            }

            if (y1 < ymin) {
                return;
            }

            x1 = c - b * y1;

            if ((x0 > xmax && x1 > xmax) || (x0 < xmin && x1 < xmin)) {
                return;
            }

            if (x0 > xmax) {
                x0 = xmax;
                y0 = (c - x0) / b;
            } else if (x0 < xmin) {
                x0 = xmin;
                y0 = (c - x0) / b;
            }

            if (x1 > xmax) {
                x1 = xmax;
                y1 = (c - x1) / b;
            } else if (x1 < xmin) {
                x1 = xmin;
                y1 = (c - x1) / b;
            }
        } else {
            x0 = xmin;
            if (vertex0 != null && vertex0.X > xmin) {
                x0 = vertex0.X;
            }

            if (x0 > xmax) {
                return;
            }

            y0 = c - a * x0;

            x1 = xmax;
            if (vertex1 != null && vertex1.X < xmax) {
                x1 = vertex1.X;
            }

            if (x1 < xmin) {
                return;
            }

            y1 = c - a * x1;

            if ((y0 > ymax && y1 > ymax) || (y0 < ymin && y1 < ymin)) {
                return;
            }

            if (y0 > ymax) {
                y0 = ymax;
                x0 = (c - y0) / a;
            } else if (y0 < ymin) {
                y0 = ymin;
                x0 = (c - y0) / a;
            }

            if (y1 > ymax) {
                y1 = ymax;
                x1 = (c - y1) / a;
            } else if (y1 < ymin) {
                y1 = ymin;
                x1 = (c - y1) / a;
            }
        }
        
        clippedVertices_ = new Dictionary<Side, Nullable<Vector2>>();
        if (vertex0 == leftVertex_) {
            clippedVertices_[Side.LEFT] = new Vector2(x0, y0);
            clippedVertices_[Side.RIGHT] = new Vector2(x1, y1);
        } else {
            clippedVertices_[Side.RIGHT] = new Vector2(x0, y0);
            clippedVertices_[Side.LEFT] = new Vector2(x1, y1);
        }
    }
}
}