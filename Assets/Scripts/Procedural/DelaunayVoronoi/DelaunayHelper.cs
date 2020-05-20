using System;
using System.Collections.Generic;
using Geometry;
using UnityEngine;

namespace Procedural {
public class Node {
    public static Stack<Node> pool = new Stack<Node>();

    public Node parent;
    public int treeSize;
}

public enum KruskalType {
    MINIMUM = 0,
    MAXIMUM
}

public static class DelaunayHelper {
    public static List<Segment2D> VisibleLineSegments(List<Edge> edges) {
        List<Segment2D> segments = new List<Segment2D> ();
			
        foreach (Edge edge in edges) {
            if (!edge.Visible) continue;
            
            segments.Add (new Segment2D(edge.ClippedEnds[Side.LEFT],edge.ClippedEnds[Side.RIGHT]));
        }
			
        return segments;
    }

    public static List<Edge> SelectEdgesForSitePoint(Vector2 pos, List<Edge> edgesToTest) {
        return edgesToTest.FindAll(edge => edge.LeftSite != null && edge.LeftSite.Position == pos ||
                                           edge.RightSite != null && edge.RightSite.Position == pos);
    }

    public static List<Edge> SelectNonIntersectingEdges(List<Edge> edgesToTest) {
        return edgesToTest;
    }

    public static List<Segment2D> DelaunayLinesForEdges(List<Edge> edges) {
        List<Segment2D> segments = new List<Segment2D>();

        foreach (Edge t in edges) {
            segments.Add(t.DelaunaySegment());
        }

        return segments;
    }

    public static List<Segment2D> Kruskal(List<Segment2D> segments, KruskalType type = KruskalType.MINIMUM) {
        Dictionary<Nullable<Vector2>, Node> nodes = new Dictionary<Nullable<Vector2>, Node>();
        List<Segment2D> mst = new List<Segment2D>();
        Stack<Node> nodePool = Node.pool;

        switch (type) {
            case KruskalType.MINIMUM:
                segments.Sort(Segment2D.CompareLengthsMax);
                break;
            case KruskalType.MAXIMUM:
                segments.Sort(Segment2D.CompareLengths);
                break;
        }

        for (int i = segments.Count; --i > -1;) {
            Segment2D segment2D = segments[i];

            Node node0;
            Node rootOfSet0;
            if (!nodes.ContainsKey(segment2D.p0)) {
                node0 = nodePool.Count > 0 ? nodePool.Pop() : new Node();

                rootOfSet0 = node0.parent = node0;
                node0.treeSize = 1;

                nodes[segment2D.p0] = node0;
            } else {
                node0 = nodes[segment2D.p0];
                rootOfSet0 = Find(node0);
            }

            Node node1;
            Node rootOfSet1;
            if (!nodes.ContainsKey(segment2D.p1)) {
                node1 = nodePool.Count > 0 ? nodePool.Pop() : new Node();

                rootOfSet1 = node1.parent = node1;
                node1.treeSize = 1;

                nodes[segment2D.p1] = node1;
            } else {
                node1 = nodes[segment2D.p1];
                rootOfSet1 = Find(node1);
            }

            if (rootOfSet0 == rootOfSet1) continue;
            
            mst.Add(segment2D);

            int treeSize0 = rootOfSet0.treeSize;
            int treeSize1 = rootOfSet1.treeSize;

            if (treeSize0 >= treeSize1) {
                rootOfSet1.parent = rootOfSet0;
                rootOfSet0.treeSize += treeSize1;
            } else {
                rootOfSet0.parent = rootOfSet1;
                rootOfSet1.treeSize += treeSize0;
            }
        }

        foreach (Node node in nodes.Values) {
            nodePool.Push(node);
        }

        return mst;
    }

    static Node Find(Node node) {
        if (node.parent == node) {
            return node;
        }

        Node root = Find(node.parent);
        node.parent = root;
        return root;
    }
}
}
