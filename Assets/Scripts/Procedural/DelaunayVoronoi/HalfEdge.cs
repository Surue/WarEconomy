using System;
using System.Collections.Generic;
using UnityEngine;

namespace Procedural {
public sealed class HalfEdge {
    static Stack<HalfEdge> pool_ = new Stack<HalfEdge>();

    public HalfEdge edgeListLeftNeighbor, edgeListRightNeighbor;
    public HalfEdge nextInPriorityQueue;

    public Edge edge;
    public Nullable<Side> leftRight;
    public Vertex vertex;

    public float yStar;

    public HalfEdge(Edge edge = null, Nullable<Side> leftRight = null) {
        Init(edge, leftRight);
    }
    
    public static HalfEdge Create(Edge edge, Nullable<Side> leftRight) {
        if (pool_.Count > 0) {
            return pool_.Pop().Init(edge, leftRight);
        } else {
            return new HalfEdge(edge, leftRight);
        }
    }

    public static HalfEdge CreateDummy() {
        return Create(null, null);
    }

    HalfEdge Init(Edge edge, Nullable<Side> leftRight) {
        this.edge = edge;
        this.leftRight = leftRight;
        nextInPriorityQueue = null;
        vertex = null;
        return this;
    }

    public override string ToString() {
        return "Halfedge (leftRight : " + leftRight + ", vertex: " + vertex + ")";
    }

    public void Dispose() {
        if (edgeListLeftNeighbor != null || edgeListRightNeighbor != null) {
            return;
        }

        if (nextInPriorityQueue != null) {
            return;
        }

        edge = null;
        leftRight = null;
        vertex = null;
        pool_.Push(this);
    }

    public void HardDispose() {
        edgeListLeftNeighbor = null;
        edgeListRightNeighbor = null;
        nextInPriorityQueue = null;
        Dispose();
    }

    internal bool IsLeftOf(Vector2 pos) {
        bool rightOfSite, above;

        Site topSite = edge.RightSite;
        rightOfSite = pos.x > topSite.X;
        if (rightOfSite && leftRight == Side.LEFT) {
            return true;
        }
        if (!rightOfSite && leftRight == Side.RIGHT) {
            return false;
        }
			
        if (edge.a == 1.0) {
            float dyp = pos.y - topSite.Y;
            float dxp = pos.x - topSite.X;
            bool fast = false;
            if ((!rightOfSite && edge.b < 0.0) || (rightOfSite && edge.b >= 0.0)) {
                above = dyp >= edge.b * dxp;	
                fast = above;
            } else {
                above = pos.x + pos.y * edge.b > edge.c;
                if (edge.b < 0.0) {
                    above = !above;
                }
                if (!above) {
                    fast = true;
                }
            }

            if (fast) return leftRight == Side.LEFT ? above : !above;
            
            float dxs = topSite.X - edge.LeftSite.X;
            above = edge.b * (dxp * dxp - dyp * dyp) <
                    dxs * dyp * (1.0 + 2.0 * dxp / dxs + edge.b * edge.b);
            if (edge.b < 0.0) {
                above = !above;
            }
        } else { 
            float yl = edge.c - edge.a * pos.x;
            float t1 = pos.y - yl;
            float t2 = pos.x - topSite.X;
            float t3 = yl - topSite.Y;
            above = t1 * t1 > t2 * t2 + t3 * t3;
        }
        return leftRight == Side.LEFT ? above : !above;
    }
}
}
