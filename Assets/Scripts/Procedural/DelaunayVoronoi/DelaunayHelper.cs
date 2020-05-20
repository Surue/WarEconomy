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
    public static List<Segment> VisibleLineSegments(List<Edge> edges) {
        List<Segment> segments = new List<Segment>();

        for (int i = 0; i < edges.Count; i++) {
            Edge edge = edges[i];

            if (edge.Visible) {
                Nullable<Vector3> p1 = edge.clippedEnds[Side.LEFT];
                Nullable<Vector3> p2 = edge.clippedEnds[Side.RIGHT];
                
                segments.Add(new Segment(p1, p2));
            }
        }

        return segments;
    }

    public static List<Edge> SelectEdgesForSitePoint(Vector3 pos, List<Edge> edgesToTest) {
        return edgesToTest.FindAll(edge => edge.LeftSite != null && edge.LeftSite.Position == pos ||
                                           edge.RightSite != null && edge.RightSite.Position == pos);
    }

    public static List<Edge> SelectNonIntersectingEdges(List<Edge> edgesToTest) {
        return edgesToTest;
    }

    public static List<Segment> DelaunayLinesForEdges(List<Edge> edges) {
        List<Segment> segments = new List<Segment>();

        Edge edge;
        for (int i = 0; i < edges.Count; i++) {
            edge = edges[i];
            segments.Add(edge.DelaunaySegment());
        }

        return segments;
    }

    public static List<Segment> Kruksal(List<Segment> segments, KruskalType type = KruskalType.MINIMUM) {
        Dictionary<Nullable<Vector3>, Node> nodes = new Dictionary<Nullable<Vector3>, Node>();
        List<Segment> mst = new List<Segment>();
        Stack<Node> nodePool = Node.pool;

        switch (type) {
            case KruskalType.MINIMUM:
                segments.Sort(delegate(Segment s1, Segment s2) { return Segment.CompareLengthsMax(s1, s2); });
                break;
            case KruskalType.MAXIMUM:
                segments.Sort(delegate(Segment s1, Segment s2) { return Segment.CompareLengths(s1, s2); });
                break;
        }

        for (int i = segments.Count; --i > -1;) {
            Segment segment = segments[i];

            Node node0 = null;
            Node rootOfSet0;
            if (!nodes.ContainsKey(segment.p0)) {
                node0 = nodePool.Count > 0 ? nodePool.Pop() : new Node();

                rootOfSet0 = node0.parent = node0;
                node0.treeSize = 1;

                nodes[segment.p0] = node0;
            } else {
                node0 = nodes[segment.p0];
                rootOfSet0 = Find(node0);
            }

            Node node1 = null;
            Node rootOfSet1;
            if (!nodes.ContainsKey(segment.p1)) {
                node1 = nodePool.Count > 0 ? nodePool.Pop() : new Node();

                rootOfSet1 = node1.parent = node1;
                node1.treeSize = 1;

                nodes[segment.p1] = node1;
            } else {
                node1 = nodes[segment.p1];
                rootOfSet1 = Find(node1);
            }

            if (rootOfSet0 != rootOfSet1) {
                mst.Add(segment);

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
        }

        foreach (Node node in nodes.Values) {
            nodePool.Push(node);
        }

        return mst;
    }

    static Node Find(Node node) {
        if (node.parent == node) {
            return node;
        } else {
            Node root = Find(node.parent);
            node.parent = root;
            return root;
        }
    }
}
}
