using System;
using System.Collections.Generic;
using Geometry;
using UnityEngine;

namespace Procedural {
public class Site : IComparable
{
    static Stack<Site> pool_ = new Stack<Site>();

    Vector3 position_;

    public Vector3 Position => position_;

    public float X => position_.x;

    public float Z => position_.z;

    public uint color;
    public float weight;
    int siteIndex_;
    List<Edge> edges_;

    internal List<Edge> Edges => edges_;

    List<Side> edgeOrientations_;
    List<Vector3> region_;

    Site(Vector3 p, int index, float weight, uint color) {
        Init(p, index, weight, color);
    }

    public static Site Create(Vector3 p, int index, float weight, uint color) {
        if (pool_.Count > 0) {
            return pool_.Pop().Init(p, index, weight, color);
                
        } else{
            return new Site(p, index, weight, color);
        }
    }

    Site Init(Vector3 p, int index, float weight, uint color) {
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

    public int CompareTo(System.Object obj) {
        Site s2 = (Site) obj;

        int returnValue = Voronoi.CompareByZThenX(this, s2);

        int tmpIndex;
        if (returnValue == -1) {
            if (siteIndex_ > s2.siteIndex_) {
                tmpIndex = siteIndex_;
                siteIndex_ = s2.siteIndex_;
                s2.siteIndex_ = tmpIndex;
            }
        }else if (returnValue == 1) {
            if (s2.siteIndex_ > siteIndex_) {
                tmpIndex = s2.siteIndex_;
                s2.siteIndex_ = siteIndex_;
                siteIndex_ = tmpIndex;
            }
        }

        return returnValue;
    }

    static readonly float EPSILON = 0.005f;

    static bool CloseEngough(Vector3 p0, Vector3 p1) {
        return Vector3.Distance(p0, p1) < EPSILON;
    }

    public override string ToString() {
        return "Site[" + siteIndex_ + "] (position = " + position_ + ")";
    }

    void Move(Vector3 p) {
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
        Edge edge;
        for (int i = 0; i < edges_.Count; i++) {
            edge = edges_[i];
            list.Add(NeighborSite(edge));
        }

        return list;
    }

    Site NeighborSite(Edge edge) {
        if (this == edge.LeftSite) {
            return edge.RightSite;
        }

        if (this == edge.RightSite) {
            return edge.LeftSite;
        }

        return null;
    }

    internal List<Vector3> Region(Rect clippingBounds) {
        if (edges_ == null || edges_.Count == 0) {
            return new List<Vector3>();
        }

        if (edgeOrientations_ == null) {
            ReorderEdges();

            region_ = ClipToBounds(clippingBounds);

            if ((new Polygon(region_)).IsClockwise()) {
                region_.Reverse();
            }
        }

        return region_;
    }

    void ReorderEdges() {
        EdgeReorderer reorderer = new EdgeReorderer(edges_, VertexOrSite.VERTEX);
        edges_ = reorderer.Edges;

        edgeOrientations_ = reorderer.EdgeOrientation;
        reorderer.Dispose();
    }

    List<Vector3> ClipToBounds(Rect bounds) {
        List<Vector3> points = new List<Vector3>();
        int n = edges_.Count;
        int i = 0;
        Edge edge;

        while (i < n && (edges_[i].Visible == false)) {
            ++i;
        }

        if (i == n) {
            return new List<Vector3>();
        }

        edge = edges_[i];
        Side orientation = edgeOrientations_[i];

        if (edge.clippedEnds[orientation] == null) {
            Debug.LogError("Null detected when ther should be a Vector3");
        }

        if (edge.clippedEnds[SideHelper.Other(orientation)] == null) {
            Debug.LogError("Null detected when ther should be a Vector3");
        }
        
        points.Add((Vector3)edge.clippedEnds[orientation]);
        points.Add((Vector3)edge.clippedEnds[SideHelper.Other(orientation)]);

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

    void Connect(List<Vector3> points, int j, Rect bounds, bool closingUp = false) {
        Vector3 rightPoint = points[points.Count - 1];
        Edge newEdge = edges_[j];
        Side newOrientation = edgeOrientations_[j];

        if (newEdge.clippedEnds[newOrientation] == null) {
            Debug.LogError("Null detected when there should be a Vector3");
        }

        Vector3 newPoint = (Vector3) newEdge.clippedEnds[newOrientation];

        if (!CloseEngough(rightPoint, newPoint)) {
            if (rightPoint.x != newPoint.x && rightPoint.z != newPoint.z) {
                int rightCheck = BoundsCheck.Check(rightPoint, bounds);
                int newCheck = BoundsCheck.Check(newPoint, bounds);

                float posX, posZ;
                if ((rightCheck & BoundsCheck.RIGHT) != 0) {
                    posX = bounds.xMax;
                    if ((newCheck & BoundsCheck.BOTTOM) != 0) {
                        posZ = bounds.yMax;
                        points.Add(new Vector3(posX, 0, posZ));
                    } else if((newCheck & BoundsCheck.TOP) != 0) {
                        posZ = bounds.yMin;
                        points.Add(new Vector3(posX, 0, posZ));
                    }else if ((newCheck & BoundsCheck.LEFT) != 0) {
                        if (rightPoint.z - bounds.y + newPoint.z - bounds.y < bounds.height) {
                            posZ = bounds.yMin;
                        } else {
                            posZ = bounds.yMax;
                        }
                        
                        points.Add(new Vector3(posX, 0, posZ));
                        points.Add(new Vector3(bounds.xMin, 0, posZ));
                    }
                } else if ((rightCheck & BoundsCheck.LEFT) != 0) {
                    posX = bounds.xMin;
                    if ((newCheck & BoundsCheck.BOTTOM) != 0) {
                        posZ = bounds.yMax;
                        points.Add(new Vector3(posX, 0, posZ));
                    } else if((newCheck & BoundsCheck.TOP) != 0) {
                        posZ = bounds.yMin;
                        points.Add(new Vector3(posX, 0, posZ));
                    }else if ((newCheck & BoundsCheck.LEFT) != 0) {
                        if (rightPoint.z - bounds.y + newPoint.z - bounds.y < bounds.height) {
                            posZ = bounds.yMin;
                        } else {
                            posZ = bounds.yMax;
                        }
                        
                        points.Add(new Vector3(posX, 0, posZ));
                        points.Add(new Vector3(bounds.xMax, 0, posZ));
                    }
                } else if ((rightCheck & BoundsCheck.TOP) != 0) {
                    posZ = bounds.yMin;
                    if ((newCheck & BoundsCheck.RIGHT) != 0) {
                        posX = bounds.xMax;
                        points.Add(new Vector3(posX, 0, posZ));
                    } else if((newCheck & BoundsCheck.LEFT) != 0) {
                        posX = bounds.xMin;
                        points.Add(new Vector3(posX, 0, posZ));
                    }else if ((newCheck & BoundsCheck.BOTTOM) != 0) {
                        if (rightPoint.x - bounds.x + newPoint.x - bounds.x < bounds.height) {
                            posX = bounds.xMin;
                        } else {
                            posX = bounds.xMax;
                        }
                        
                        points.Add(new Vector3(posX, 0, posZ));
                        points.Add(new Vector3(posX, 0, bounds.yMax));
                    }
                } else if ((rightCheck & BoundsCheck.BOTTOM) != 0) {
                    posZ = bounds.yMax;
                    if ((newCheck & BoundsCheck.RIGHT) != 0) {
                        posX = bounds.xMax;
                        points.Add(new Vector3(posX, 0, posZ));
                    } else if((newCheck & BoundsCheck.LEFT) != 0) {
                        posX = bounds.xMin;
                        points.Add(new Vector3(posX, 0, posZ));
                    }else if ((newCheck & BoundsCheck.TOP) != 0) {
                        if (rightPoint.x - bounds.x + newPoint.x - bounds.x < bounds.height) {
                            posX = bounds.xMin;
                        } else {
                            posX = bounds.xMax;
                        }
                        
                        points.Add(new Vector3(posX, 0, posZ));
                        points.Add(new Vector3(posX, 0, bounds.yMin));
                    }
                }
            }
            if (closingUp) {
                return;
            }
            points.Add(newPoint);
        }

        if (newEdge.clippedEnds[SideHelper.Other(newOrientation)] == null) {
            Debug.LogError("Null detected when there shuold be a Vector3");
        }

        Vector3 newRightPoint = (Vector3) newEdge.clippedEnds[SideHelper.Other(newOrientation)];
        if (!CloseEngough(points[0], newRightPoint)) {
            points.Add(newRightPoint);
        }
    }

    public float Dist(Vector3 p) {
        return Vector3.Distance(p, position_);
    }
}

static class BoundsCheck {
    public static readonly int TOP = 1;
    public static readonly int BOTTOM = 2;
    public static readonly int LEFT = 4;
    public static readonly int RIGHT = 8;

    public static int Check(Vector3 point, Rect bounds) {
        int value = 0;
        if (point.x == bounds.xMin) {
            value |= LEFT;
        }
        
        if (point.x == bounds.xMax) {
            value |= RIGHT;
        }
        
        if (point.z == bounds.yMin) {
            value |= TOP;
        }
        
        if (point.z == bounds.yMax) {
            value |= BOTTOM;
        }

        return value;
    }
    
}
}

