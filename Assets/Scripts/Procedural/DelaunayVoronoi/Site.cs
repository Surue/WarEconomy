using System;
using System.Collections.Generic;
using Geometry;
using UnityEngine;

namespace Procedural {
public class Site : IComparable {
    static Stack<Site> pool_ = new Stack<Site>();

    static readonly float EPSILON = 0.005f;

    Vector2 position_;

    public Vector2 Position => position_;

    public float X => position_.x;

    public float Y => position_.y;

    public uint color;
    public float weight;
    uint siteIndex_;
    List<Edge> edges_;

    internal List<Edge> Edges => edges_;

    List<Side> edgeOrientations_;
    List<Vector2> region_;

    Site(Vector2 p, uint index, float weight, uint color) {
        Init(p, index, weight, color);
    }

    public static Site Create(Vector2 p, uint index, float weight, uint color) {
        return pool_.Count > 0 ? pool_.Pop().Init(p, index, weight, color) : new Site(p, index, weight, color);
    }

    Site Init(Vector2 p, uint index, float weight, uint color) {
        position_ = p;
        siteIndex_ = index;
        this.weight = weight;
        this.color = color;

        edges_ = new List<Edge>();
        region_ = null;
        return this;
    }

    internal static void SortSites(List<Site> sites) {
        sites.Sort();
    }

    public int CompareTo(object obj) {
        Site s2 = (Site) obj;

        int returnValue = Voronoi.CompareByYThenX(this, s2);

        // swap _siteIndex values if necessary to match new ordering:
        uint tempIndex;
        if (returnValue == -1) {
            if (siteIndex_ <= s2.siteIndex_) return returnValue;

            tempIndex = siteIndex_;
            siteIndex_ = s2.siteIndex_;
            s2.siteIndex_ = tempIndex;
        } else if (returnValue == 1) {
            if (s2.siteIndex_ <= siteIndex_) return returnValue;

            tempIndex = s2.siteIndex_;
            s2.siteIndex_ = siteIndex_;
            siteIndex_ = tempIndex;
        }

        return returnValue;
    }

    static bool CloseEnough(Vector2 p0, Vector2 p1) {
        return Vector2.Distance(p0, p1) < EPSILON;
    }

    public override string ToString() {
        return "Site[" + siteIndex_ + "] (position = " + position_ + ")";
    }

    void Move(Vector2 p) {
        Clear();
        position_ = p;
    }

    public void Dispose() {
        Clear();
        pool_.Push(this);
    }

    void Clear() {
        if (edges_ != null) {
            edges_.Clear();
            edges_ = null;
        }

        if (edgeOrientations_ != null) {
            edgeOrientations_.Clear();
            edgeOrientations_ = null;
        }

        if (region_ != null) {
            region_.Clear();
            region_ = null;
        }
    }

    public void AddEdge(Edge edge) {
        edges_.Add(edge);
    }

    public Edge NearestEdge() {
        edges_.Sort(Edge.CompareSitesDistances);

        return edges_[0];
    }

    public List<Site> NeighborSites() {
        if (edges_ == null || edges_.Count == 0) {
            return new List<Site>();
        }

        if (edgeOrientations_ == null) {
            ReorderEdges();
        }

        List<Site> list = new List<Site>();

        foreach (Edge edge in edges_) {
            list.Add(NeighborSite(edge));
        }

        return list;
    }

    Site NeighborSite(Edge edge) {
        if (this == edge.LeftSite) {
            return edge.RightSite;
        }

        return this == edge.RightSite ? edge.LeftSite : null;
    }

    internal List<Vector2> Region(Rect clippingBounds) {
        if (edges_ == null || edges_.Count == 0) {
            return new List<Vector2>();
        }

        if (edgeOrientations_ != null) return region_;

        ReorderEdges();

        region_ = ClipToBounds(clippingBounds);

        if (new Polygon2D(region_).IsClockwise()) {
            region_.Reverse();
        }

        return region_;
    }

    void ReorderEdges() {
        EdgeReorderer reorderer = new EdgeReorderer(edges_, VertexOrSite.VERTEX);
        edges_ = reorderer.Edges;

        edgeOrientations_ = reorderer.EdgeOrientation;
        reorderer.Dispose();
    }

    List<Vector2> ClipToBounds(Rect bounds) {
        List<Vector2> points = new List<Vector2>();
        int n = edges_.Count;
        int i = 0;
        while (i < n && edges_[i].Visible == false) {
            ++i;
        }

        if (i == n) {
            return new List<Vector2>();
        }


        Edge edge = edges_[i];
        Side orientation = edgeOrientations_[i];

        if (edge.ClippedEnds[orientation] == null) {
            Debug.LogError("XXX: Null detected when there should be a Vector2!");
        }

        if (edge.ClippedEnds[SideHelper.Other(orientation)] == null) {
            Debug.LogError("XXX: Null detected when there should be a Vector2!");
        }

        points.Add((Vector2) edge.ClippedEnds[orientation]);
        points.Add((Vector2) edge.ClippedEnds[SideHelper.Other(orientation)]);

        for (int j = i + 1; j < n; ++j) {
            edge = edges_[j];
            if (edge.Visible == false) {
                continue;
            }

            Connect(points, j, bounds);
        }

        Connect(points, i, bounds, true);

        return points;
    }

    void Connect(List<Vector2> points, int j, Rect bounds, bool closingUp = false) {
        Vector2 rightPoint = points[points.Count - 1];
        Edge newEdge = edges_[j];
        Side newOrientation = edgeOrientations_[j];
        if (newEdge.ClippedEnds[newOrientation] == null) {
            Debug.LogError("XXX: Null detected when there should be a Vector2!");
        }

        Vector2 newPoint = (Vector2) newEdge.ClippedEnds[newOrientation];
        if (!CloseEnough(rightPoint, newPoint)) {
            if (rightPoint.x != newPoint.x
                && rightPoint.y != newPoint.y) {
                int rightCheck = BoundsCheck.Check(rightPoint, bounds);
                int newCheck = BoundsCheck.Check(newPoint, bounds);
                float px, py;
                if ((rightCheck & BoundsCheck.RIGHT) != 0) {
                    px = bounds.xMax;
                    if ((newCheck & BoundsCheck.BOTTOM) != 0) {
                        py = bounds.yMax;
                        points.Add(new Vector2(px, py));
                    } else if ((newCheck & BoundsCheck.TOP) != 0) {
                        py = bounds.yMin;
                        points.Add(new Vector2(px, py));
                    } else if ((newCheck & BoundsCheck.LEFT) != 0) {
                        py = rightPoint.y - bounds.y + newPoint.y - bounds.y < bounds.height ? bounds.yMin : bounds.yMax;

                        points.Add(new Vector2(px, py));
                        points.Add(new Vector2(bounds.xMin, py));
                    }
                } else if ((rightCheck & BoundsCheck.LEFT) != 0) {
                    px = bounds.xMin;
                    if ((newCheck & BoundsCheck.BOTTOM) != 0) {
                        py = bounds.yMax;
                        points.Add(new Vector2(px, py));
                    } else if ((newCheck & BoundsCheck.TOP) != 0) {
                        py = bounds.yMin;
                        points.Add(new Vector2(px, py));
                    } else if ((newCheck & BoundsCheck.RIGHT) != 0) {
                        py = rightPoint.y - bounds.y + newPoint.y - bounds.y < bounds.height ? bounds.yMin : bounds.yMax;

                        points.Add(new Vector2(px, py));
                        points.Add(new Vector2(bounds.xMax, py));
                    }
                } else if ((rightCheck & BoundsCheck.TOP) != 0) {
                    py = bounds.yMin;
                    if ((newCheck & BoundsCheck.RIGHT) != 0) {
                        px = bounds.xMax;
                        points.Add(new Vector2(px, py));
                    } else if ((newCheck & BoundsCheck.LEFT) != 0) {
                        px = bounds.xMin;
                        points.Add(new Vector2(px, py));
                    } else if ((newCheck & BoundsCheck.BOTTOM) != 0) {
                        px = rightPoint.x - bounds.x + newPoint.x - bounds.x < bounds.width ? bounds.xMin : bounds.xMax;

                        points.Add(new Vector2(px, py));
                        points.Add(new Vector2(px, bounds.yMax));
                    }
                } else if ((rightCheck & BoundsCheck.BOTTOM) != 0) {
                    py = bounds.yMax;
                    if ((newCheck & BoundsCheck.RIGHT) != 0) {
                        px = bounds.xMax;
                        points.Add(new Vector2(px, py));
                    } else if ((newCheck & BoundsCheck.LEFT) != 0) {
                        px = bounds.xMin;
                        points.Add(new Vector2(px, py));
                    } else if ((newCheck & BoundsCheck.TOP) != 0) {
                        px = rightPoint.x - bounds.x + newPoint.x - bounds.x < bounds.width ? bounds.xMin : bounds.xMax;

                        points.Add(new Vector2(px, py));
                        points.Add(new Vector2(px, bounds.yMin));
                    }
                }
            }

            if (closingUp) {
                return;
            }

            points.Add(newPoint);
        }

        if (newEdge.ClippedEnds[SideHelper.Other(newOrientation)] == null) {
            Debug.LogError("XXX: Null detected when there should be a Vector2!");
        }

        Vector2 newRightPoint = (Vector2) newEdge.ClippedEnds[SideHelper.Other(newOrientation)];
        if (!CloseEnough(points[0], newRightPoint)) {
            points.Add(newRightPoint);
        }
    }

    public float Dist(Vector2 p) {
        return Vector2.Distance(p, position_);
    }
}

static class BoundsCheck {
    public static readonly int TOP = 1;
    public static readonly int BOTTOM = 2;
    public static readonly int LEFT = 4;
    public static readonly int RIGHT = 8;

    public static int Check(Vector2 point, Rect bounds) {
        int value = 0;
        if (point.x == bounds.xMin) {
            value |= LEFT;
        }

        if (point.x == bounds.xMax) {
            value |= RIGHT;
        }

        if (point.y == bounds.yMin) {
            value |= TOP;
        }

        if (point.y == bounds.yMax) {
            value |= BOTTOM;
        }

        return value;
    }
}
}