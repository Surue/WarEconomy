using System;
using System.Collections.Generic;
using Geometry;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Procedural {
public class Voronoi {
    SiteList sites_;
    Dictionary<Vector2, Site> sitesIndexedByLocation_;
    List<Triangle> triangles_;
    List<Edge> edges_;

    Rect plotBounds_;

    public Rect PlotBounds => plotBounds_;

    public List<Edge> Edges => edges_;

    Site fortunesAlgorithmBottomMostSite_;

    public Voronoi(List<Vector2> points, List<uint> colors, Rect plotBounds) {
        sites_ = new SiteList();
        sitesIndexedByLocation_ = new Dictionary<Vector2, Site>();

        AddSites(points, colors);
        plotBounds_ = plotBounds;
        triangles_ = new List<Triangle>();
        edges_ = new List<Edge>();
        FortunesAlgorithm();
    }

    public void Dispose() {
        if (sites_ != null) {
            sites_.Dispose();
            sites_ = null;
        }

        int n;

        if (triangles_ != null) {
            n = triangles_.Count;
            for (int i = 0; i < n; ++i) {
                triangles_[i].Dispose();
            }

            triangles_.Clear();
            triangles_ = null;
        }

        if (edges_ != null) {
            n = edges_.Count;
            for (int i = 0; i < n; ++i) {
                edges_[i].Dispose();
            }

            edges_.Clear();
            edges_ = null;
        }

        sitesIndexedByLocation_ = null;
    }

    void AddSites(List<Vector2> points, List<uint> colors) {
        int length = points.Count;
        for (int i = 0; i < length; ++i) {
            AddSite(points[i], colors != null ? colors[i] : 0, i);
        }
    }

    void AddSite(Vector2 pos, uint color, int index) {
        if (sitesIndexedByLocation_.ContainsKey(pos)) {
            return;
        }

        float weight = Random.value * 100f;
        Site site = Site.Create(pos, (uint)index, weight, color);
        sites_.Add(site);
        sitesIndexedByLocation_[pos] = site;
    }

    public List<Vector2> Region(Vector2 pos) {
        Site site = sitesIndexedByLocation_[pos];
        return site == null ? new List<Vector2>() : site.Region(plotBounds_);
    }

    public List<Vector2> NeighborSitesForSite(Vector2 pos) {
        List<Vector2> points = new List<Vector2>();

        Site site = sitesIndexedByLocation_[pos];
        if (site == null) {
            return points;
        }

        List<Site> sites = site.NeighborSites();
        foreach (Site neighbor in sites) {
            points.Add(neighbor.Position);
        }

        return points;
    }

    public List<Circle2D> Circles() {
        return sites_.Circles();
    }

    public List<Segment2D> VoronoiBoundaryForSite(Vector2 pos) {
        return DelaunayHelper.VisibleLineSegments(DelaunayHelper.SelectEdgesForSitePoint(pos, edges_));
    }

    public List<Segment2D> DelaunayLinesForSite(Vector2 pos) {
        return DelaunayHelper.DelaunayLinesForEdges(DelaunayHelper.SelectEdgesForSitePoint(pos, edges_));
    }

    public List<Segment2D> VoronoiDiagram() {
        return DelaunayHelper.VisibleLineSegments(edges_);
    }

    public List<Segment2D> DelaunayTriangulation() {
        return DelaunayHelper.DelaunayLinesForEdges(DelaunayHelper.SelectNonIntersectingEdges(edges_));
    }

    public List<Segment2D> Hull() {
        return DelaunayHelper.DelaunayLinesForEdges(HullEdges());
    }

    List<Edge> HullEdges() {
        return edges_.FindAll(edge => edge.IsPartOfConvexHull());
    }

    public List<Vector2> HullPointsInOrder() {
        List<Edge> hullEdge = HullEdges();

        List<Vector2> points = new List<Vector2>();

        if (hullEdge.Count == 0) {
            return points;
        }

        EdgeReorderer reorderer = new EdgeReorderer(hullEdge, VertexOrSite.SITE);
        hullEdge = reorderer.Edges;
        List<Side> orientations = reorderer.EdgeOrientation;
        reorderer.Dispose();

        int n = hullEdge.Count;
        for (int i = 0; i < n; ++i) {
            Edge edge = hullEdge[i];
            Side orientation = orientations[i];
            points.Add(edge.Site(orientation).Position);
        }

        return points;
    }

    public List<Segment2D> SpanningTree(KruskalType type = KruskalType.MINIMUM) {
        List<Edge> edges = DelaunayHelper.SelectNonIntersectingEdges(edges_);
        List<Segment2D> segments = DelaunayHelper.DelaunayLinesForEdges(edges);
        return DelaunayHelper.Kruskal(segments, type);
    }

    public List<List<Vector2>> Regions() {
        return sites_.Regions(plotBounds_);
    }

    public List<uint> SiteColors() {
        return sites_.SiteColor();
    }

    public Nullable<Vector2> NearestSitePoint(float x, float z) {
        return sites_.NearestSitePoint(x, z);
    }

    public List<Vector2> SitePositions() {
        return sites_.SitePositions();
    }

    void FortunesAlgorithm() {
        Vertex vertex;
        Vector2 newIntStar = Vector2.zero;
        Edge edge;

        Rect dataBounds = sites_.GetSiteBounds();

        int sqrtNsites = (int) (Mathf.Sqrt(sites_.Count + 4));
        HalfEdgePriorityQueue heap = new HalfEdgePriorityQueue(dataBounds.y, dataBounds.height, sqrtNsites);
        EdgeList edgeList = new EdgeList(dataBounds.x, dataBounds.width, sqrtNsites);
        List<HalfEdge> halfEdges = new List<HalfEdge>();
        List<Vertex> vertices = new List<Vertex>();

        fortunesAlgorithmBottomMostSite_ = sites_.Next();
        Site newSite = sites_.Next();

        for (;;) {
            if (heap.Empty() == false) {
                newIntStar = heap.Min();
            }

            Site bottomSite;
            HalfEdge lbnd;
            HalfEdge rbnd;
            HalfEdge bisector;
            if (newSite != null && (heap.Empty() || CompareByYThenX(newSite, newIntStar) < 0)) {
                lbnd = edgeList.EdgeListLeftNeighbor(newSite.Position);
                rbnd = lbnd.edgeListRightNeighbor;
                bottomSite = FortunesAlgorithmRightRegion(lbnd);
                edge = Edge.CreateBisectingEdge(bottomSite, newSite);
                edges_.Add(edge);

                bisector = HalfEdge.Create(edge, Side.LEFT);
                halfEdges.Add(bisector);
                edgeList.Insert(lbnd, bisector);

                // first half of Step 11:
                if ((vertex = Vertex.Intersect(lbnd, bisector)) != null) {
                    vertices.Add(vertex);
                    heap.Remove(lbnd);
                    lbnd.vertex = vertex;
                    lbnd.yStar = vertex.Y + newSite.Dist(vertex.Position);
                    heap.Insert(lbnd);
                }

                lbnd = bisector;
                bisector = HalfEdge.Create(edge, Side.RIGHT);
                halfEdges.Add(bisector);
                edgeList.Insert(lbnd, bisector);

                if ((vertex = Vertex.Intersect(bisector, rbnd)) != null) {
                    vertices.Add(vertex);
                    bisector.vertex = vertex;
                    bisector.yStar = vertex.Y + newSite.Dist(vertex.Position);
                    heap.Insert(bisector);
                }

                newSite = sites_.Next();
            } else if (heap.Empty() == false) {
                lbnd = heap.ExtractMin();
                HalfEdge llbnd = lbnd.edgeListLeftNeighbor;
                rbnd = lbnd.edgeListRightNeighbor;
                HalfEdge rrbnd = rbnd.edgeListRightNeighbor;
                bottomSite = FortunesAlgorithmLeftRegion(lbnd);
                Site topSite = FortunesAlgorithmRightRegion(rbnd);

                Vertex v = lbnd.vertex;
                v.SetIndex();
                lbnd.edge.SetVertex((Side) lbnd.leftRight, v);
                rbnd.edge.SetVertex((Side) rbnd.leftRight, v);
                edgeList.Remove(lbnd);
                heap.Remove(rbnd);
                edgeList.Remove(rbnd);
                Side leftRight = Side.LEFT;
                if (bottomSite.Y > topSite.Y) {
                    Site tmpSite = bottomSite;
                    bottomSite = topSite;
                    topSite = tmpSite;
                    leftRight = Side.RIGHT;
                }

                edge = Edge.CreateBisectingEdge(bottomSite, topSite);
                edges_.Add(edge);
                bisector = HalfEdge.Create(edge, leftRight);
                halfEdges.Add(bisector);
                edgeList.Insert(llbnd, bisector);
                edge.SetVertex(SideHelper.Other(leftRight), v);
                if ((vertex = Vertex.Intersect(llbnd, bisector)) != null) {
                    vertices.Add(vertex);
                    heap.Remove(llbnd);
                    llbnd.vertex = vertex;
                    llbnd.yStar = vertex.Y + bottomSite.Dist(vertex.Position);
                    heap.Insert(llbnd);
                }

                if ((vertex = Vertex.Intersect(bisector, rrbnd)) == null) continue;
                
                vertices.Add(vertex);
                bisector.vertex = vertex;
                bisector.yStar = vertex.Y + bottomSite.Dist(vertex.Position);
                heap.Insert(bisector);
            } else {
                break;
            }
        }

        // heap should be empty now
        heap.Dispose();
        edgeList.Dispose();

        foreach (HalfEdge halfEdge in halfEdges) {
            halfEdge.HardDispose();
        }

        halfEdges.Clear();

        foreach (Edge t in edges_) {
            edge = t;
            edge.ClipVertices(plotBounds_);
        }

        foreach (Vertex t in vertices) {
            vertex = t;
            vertex.Dispose();
        }

        vertices.Clear();
    }

    Site FortunesAlgorithmLeftRegion(HalfEdge halfEdge) {
        Edge edge = halfEdge.edge;
        return edge == null ? fortunesAlgorithmBottomMostSite_ : edge.Site((Side) halfEdge.leftRight);
    }

    Site FortunesAlgorithmRightRegion(HalfEdge halfEdge) {
        Edge edge = halfEdge.edge;
        return edge == null ? fortunesAlgorithmBottomMostSite_ : edge.Site(SideHelper.Other((Side) halfEdge.leftRight));
    }

    public static int CompareByYThenX(Site s1, Site s2) {
        if (s1.Y < s2.Y) {
            return -1;
        }

        if (s1.Y > s2.Y) {
            return 1;
        }

        if (s1.X < s2.X) {
            return -1;
        }

        return s1.X > s2.X ? 1 : 0;
    }

    public static int CompareByYThenX(Site s1, Vector2 p1) {
        if (s1.Y < p1.y) {
            return -1;
        }

        if (s1.Y > p1.y) {
            return 1;
        }

        if (s1.X < p1.x) {
            return -1;
        }

        return s1.X > p1.x ? 1 : 0;
    }
}
}