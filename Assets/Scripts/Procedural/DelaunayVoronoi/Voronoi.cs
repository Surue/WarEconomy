using System;
using System.Collections.Generic;
using Geometry;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Procedural {
public class Voronoi {
    SiteList sites_;
    Dictionary<Vector3, Site> sitesIndexedByLocation_;
    List<Triangle> triangles_;
    List<Edge> edges_;

    Rect plotBounds_;

    public Rect PlotBounds => plotBounds_;

    public List<Edge> Edges => edges_;

    Site fortunesAlgorithmBottomMostSite_;

    public Voronoi(List<Vector3> points, List<uint> colors, Rect plotBounds) {
        sites_ = new SiteList();
        sitesIndexedByLocation_ = new Dictionary<Vector3, Site>();

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

    void AddSites(List<Vector3> points, List<uint> colors) {
        int length = points.Count;
        for (int i = 0; i < length; ++i) {
            AddSite(points[i], (colors != null) ? colors[i] : 0, i);
        }
    }

    void AddSite(Vector3 pos, uint color, int index) {
        if (sitesIndexedByLocation_.ContainsKey(pos)) {
            return;
        }

        float weight = Random.value * 100f;
        Site site = Site.Create(pos, index, weight, color);
        sites_.Add(site);
        sitesIndexedByLocation_[pos] = site;
    }

    public List<Vector3> Region(Vector3 pos) {
        Site site = sitesIndexedByLocation_[pos];
        if (site == null) {
            return new List<Vector3>();
        }

        return site.Region(plotBounds_);
    }

    public List<Vector3> NeighborSitesForSite(Vector3 pos) {
        List<Vector3> points = new List<Vector3>();

        Site site = sitesIndexedByLocation_[pos];
        if (site == null) {
            return points;
        }

        List<Site> sites = site.NeighborSites();
        Site neighbor;
        for (int i = 0; i < sites.Count; i++) {
            neighbor = sites[i];
            points.Add(neighbor.Position);
        }

        return points;
    }

    public List<Circle> Circles() {
        return sites_.Circles();
    }

    public List<Segment> VoronoiBouondaryForSite(Vector3 pos) {
        return DelaunayHelper.VisibleLineSegments(DelaunayHelper.SelectEdgesForSitePoint(pos, edges_));
    }

    public List<Segment> DelaunayLinesForSite(Vector3 pos) {
        return DelaunayHelper.DelaunayLinesForEdges(DelaunayHelper.SelectEdgesForSitePoint(pos, edges_));
    }

    public List<Segment> VoronoiDiagram() {
        return DelaunayHelper.VisibleLineSegments(edges_);
    }

    public List<Segment> DelaunayTriangulation() {
        return DelaunayHelper.DelaunayLinesForEdges(DelaunayHelper.SelectNonIntersectingEdges(edges_));
    }

    public List<Segment> Hull() {
        return DelaunayHelper.DelaunayLinesForEdges(HullEdges());
    }

    List<Edge> HullEdges() {
        return edges_.FindAll(delegate(Edge edge) { return edge.IsPartOfConvexHull(); });
    }
    
    public List<Vector3> HullPointsInOrder() {
        List<Edge> hullEdge = HullEdges();
        
        List<Vector3> points = new List<Vector3>();

        if (hullEdge.Count == 0) {
            return points;
        }
        
        EdgeReorderer reorderer = new EdgeReorderer(hullEdge, VertexOrSite.SITE);
        hullEdge = reorderer.Edges;
        List<Side> orientations = reorderer.EdgeOrientation;
        reorderer.Dispose();

        Side orientation;

        int n = hullEdge.Count;
        for (int i = 0; i < n; ++i) {
            Edge edge = hullEdge[i];
            orientation = orientations[i];
            points.Add(edge.Site(orientation).Position);
        }

        return points;
    }

    public List<Segment> SpanningTree(KruskalType type = KruskalType.MINIMUM) {
        List<Edge> edges = DelaunayHelper.SelectNonIntersectingEdges(edges_);
        List<Segment> segments = DelaunayHelper.DelaunayLinesForEdges(edges);
        return DelaunayHelper.Kruksal(segments, type);
    }

    public List<List<Vector3>> Regions() {
        return sites_.Regions(plotBounds_);
    }

    public List<uint> SiteColors() {
        return sites_.SiteColor();
    }

    public Nullable<Vector3> NearestSitePoint(float x, float z) {
        return sites_.NearestSitePoint(x, z);
    }

    public List<Vector3> SitePositions() {
        return sites_.SitePositions();
    }

    void FortunesAlgorithm() {
        Site newSite, bottomSite, topSite, tmpSite;
        Vertex v, vertex;
        Vector3 newIntStar = Vector3.zero;
        Side leftRight;
        Halfedge lbnd, rbnd, llbnd, rrbnd, bisector;
        Edge edge;

        Rect dataBounds = sites_.GetSiteBounds();
        
        int sqrtNSite = (int)(Mathf.Sqrt(sites_.Count + 4));
        HalfedgePriorityQueue heap = new HalfedgePriorityQueue(dataBounds.y, dataBounds.height, sqrtNSite);
        EdgeList edgeList = new EdgeList(dataBounds.x, dataBounds.width, sqrtNSite);
        List<Halfedge> halfedges = new List<Halfedge>();
        List<Vertex> vertices = new List<Vertex>();

        fortunesAlgorithmBottomMostSite_ = sites_.Next();
        newSite = sites_.Next();

        for (;;) {
            if (heap.Empty() == false) {
                newIntStar = heap.Min();
            }

            if (newSite != null && (heap.Empty() || CompareByZThenX(newSite, newIntStar) < 0)) {
                lbnd = edgeList.EdgeListLeftNeighbor(newSite.Position);

                rbnd = lbnd.edgeListRightNeighbor;

                bottomSite = FortunesAlgorithmRightRegion(lbnd);

                edge = Edge.CreateBisectingEdge(bottomSite, newSite);

                edges_.Add(edge);

                bisector = Halfedge.Create(edge, Side.LEFT);
                halfedges.Add(bisector);

                edgeList.Insert(lbnd, bisector);

                if ((vertex = Vertex.Intersect(lbnd, bisector)) != null) {
                    vertices.Add(vertex);
                    heap.Remove(lbnd);
                    lbnd.vertex = vertex;
                    lbnd.zStar = vertex.Z + newSite.Dist(vertex.Position);
                    heap.Insert(lbnd);
                }

                lbnd = bisector;
                bisector = Halfedge.Create(edge, Side.RIGHT);
                halfedges.Add(bisector);
                edgeList.Insert(lbnd, bisector);

                if ((vertex = Vertex.Intersect(bisector, rbnd)) != null) {
                    vertices.Add(vertex);
                    bisector.vertex = vertex;
                    bisector.zStar = vertex.Z + newSite.Dist(vertex.Position);
                    heap.Insert(bisector);
                }

                newSite = sites_.Next();
            }else if (!heap.Empty()) {
                lbnd = heap.ExtractMin();
                llbnd = lbnd.edgeListLeftNeighbor;
                rbnd = lbnd.edgeListRightNeighbor;
                rrbnd = rbnd.edgeListRightNeighbor;

                bottomSite = FortunesAlgorithmLeftRegion(lbnd);
                topSite = FortunesAlgorithmLeftRegion(rbnd);

                v = lbnd.vertex;
                v.SetIndex();
                lbnd.edge.SetVertex((Side)lbnd.leftRight, v);
                rbnd.edge.SetVertex((Side) rbnd.leftRight, v);
                edgeList.Remove(lbnd);
                heap.Remove(rbnd);
                edgeList.Remove(rbnd);
                leftRight = Side.LEFT;
                if (bottomSite.Z > topSite.Z) {
                    tmpSite = bottomSite;
                    bottomSite = topSite;
                    topSite = tmpSite;
                    leftRight = Side.RIGHT;
                }

                edge = Edge.CreateBisectingEdge(bottomSite, topSite);
                edges_.Add(edge);
                bisector = Halfedge.Create(edge, leftRight);
                halfedges.Add(bisector);
                edgeList.Insert(llbnd, bisector);
                edge.SetVertex(SideHelper.Other(leftRight), v);
                if ((vertex = Vertex.Intersect(llbnd, bisector)) != null) {
                    vertices.Add(vertex);
                    heap.Remove(llbnd);
                    llbnd.vertex = vertex;
                    llbnd.zStar = vertex.Z + bottomSite.Dist(vertex.Position);
                    heap.Insert(llbnd);
                }
                if ((vertex = Vertex.Intersect(bisector, rrbnd)) != null) {
                    vertices.Add(vertex);
                    bisector.vertex = vertex;
                    bisector.zStar = vertex.Z + bottomSite.Dist(vertex.Position);
                    heap.Insert(bisector);
                }
            } else {
                break;
            }
        }
        
        heap.Dispose();
        edgeList.Dispose();

        for (int i = 0; i < halfedges.Count; i++) {
            Halfedge halfedge = halfedges[i];
            halfedge.HardDispose();
        }
        halfedges.Clear();

        for (int i = 0; i < edges_.Count; i++) {
            edge = edges_[i];
            edge.ClipVertices(plotBounds_);
        }

        for (int i = 0; i < vertices.Count; i++) {
            vertex = vertices[i];
            vertex.Dispose();
        }
        vertices.Clear();
    }

    Site FortunesAlgorithmLeftRegion(Halfedge halfedge) {
        Edge edge = halfedge.edge;
        if (edge == null) {
            return fortunesAlgorithmBottomMostSite_;
        }

        return edge.Site((Side) halfedge.leftRight);
    }
    
    Site FortunesAlgorithmRightRegion(Halfedge halfedge) {
        Edge edge = halfedge.edge;
        if (edge == null) {
            return fortunesAlgorithmBottomMostSite_;
        }

        return edge.Site(SideHelper.Other((Side) halfedge.leftRight));
    }

    public static int CompareByZThenX(Site s1, Site s2) {
        if (s1.Z < s2.Z) {
            return -1;
        }

        if (s1.Z > s2.Z) {
            return 1;
        }

        if (s1.X < s2.X) {
            return -1;
        }

        if (s1.X > s2.X) {
            return 1;
        }

        return 0;
    }
    
    public static int CompareByZThenX(Site s1, Vector3 p1) {
        if (s1.Z < p1.z) {
            return -1;
        }

        if (s1.Z > p1.z) {
            return 1;
        }

        if (s1.X < p1.x) {
            return -1;
        }

        if (s1.X > p1.x) {
            return 1;
        }

        return 0;
    }
}
}
