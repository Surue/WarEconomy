﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Procedural {
public sealed class Halfedge {
    static Stack<Halfedge> pool_ = new Stack<Halfedge>();

    public Halfedge edgeListLeftNeighbor, edgeListRightNeighbor;
    public Halfedge nextInPriorityQueue;

    public Edge edge;
    public Nullable<Side> leftRight;
    public Vertex vertex;

    public float yStar;

    public Halfedge(Edge edge = null, Nullable<Side> leftRight = null) {
        Init(edge, leftRight);
    }
    
    public static Halfedge Create(Edge edge, Nullable<Side> leftRight) {
        if (pool_.Count > 0) {
            return pool_.Pop().Init(edge, leftRight);
        } else {
            return new Halfedge(edge, leftRight);
        }
    }

    public static Halfedge CreateDummy() {
        return Create(null, null);
    }

    Halfedge Init(Edge edge, Nullable<Side> leftRight) {
        this.edge = edge;
        this.leftRight = leftRight;
        nextInPriorityQueue = null;
        vertex = null;
        return this;
    }

    public override string ToString() {
        return "Halfedge (leftRight : " + leftRight.ToString() + ", vertex: " + vertex.ToString() + ")";
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
        Site topSite;
        bool rightOfSite, above, fast;
        float dxp, dyp, dxs, t1, t2, t3, yl;
			
        topSite = edge.RightSite;
        rightOfSite = pos.x > topSite.X;
        if (rightOfSite && leftRight == Side.LEFT) {
            return true;
        }
        if (!rightOfSite && leftRight == Side.RIGHT) {
            return false;
        }
			
        if (edge.a == 1.0) {
            dyp = pos.y - topSite.Y;
            dxp = pos.x - topSite.X;
            fast = false;
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
            if (!fast) {
                dxs = topSite.X - edge.LeftSite.X;
                above = edge.b * (dxp * dxp - dyp * dyp) <
                        dxs * dyp * (1.0 + 2.0 * dxp / dxs + edge.b * edge.b);
                if (edge.b < 0.0) {
                    above = !above;
                }
            }
        } else {  /* edge.b == 1.0 */
            yl = edge.c - edge.a * pos.x;
            t1 = pos.y - yl;
            t2 = pos.x - topSite.X;
            t3 = yl - topSite.Y;
            above = t1 * t1 > t2 * t2 + t3 * t3;
        }
        return leftRight == Side.LEFT ? above : !above;
    }
}
}
