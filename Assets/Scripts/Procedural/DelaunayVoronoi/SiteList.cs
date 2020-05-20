﻿using System;
using System.Collections.Generic;
using Geometry;
using UnityEngine;

namespace Procedural {
public sealed class SiteList {
    List<Site> sites_;
    int currentIndex_;
    bool sorted_;

    public int Count => sites_.Count;

    public SiteList() {
        sites_ = new List<Site>();
        sorted_ = false;
    }

    public void Dispose() {
        if (sites_ == null) return;

        for (int i = 0; i < sites_.Count; i++) {
            Site site = sites_[i];
            site.Dispose();
        }
        
        sites_.Clear();
        sites_ = null;
    }

    public int Add(Site site) {
        sorted_ = false;
        sites_.Add(site);
        
        return sites_.Count;
    }

    public Site Next() {
        if (sorted_ == false) {
            Debug.LogError("SiteList not sorted");
        }

        if (currentIndex_ < sites_.Count) {
            return sites_[currentIndex_++];
        } else {
            return null;
        }
    }

    internal Rect GetSiteBounds() {
        if (sorted_ == false) {
            Site.SortSites(sites_);
            currentIndex_ = 0;
            sorted_ = true;
        }

        if (sites_.Count == 0) {
            return new Rect(0, 0, 0, 0);
        }
        
        float xMin, xMax, yMin, yMax;
        xMin = float.MaxValue;
        xMax = float.MinValue;
        for (int i = 0; i < sites_.Count; i++) {
            Site site = sites_[i];
            if (site.X < xMin) {
                xMin = site.X;
            }

            if (site.X > xMax) {
                xMax = site.X;
            }
        }

        yMin = sites_[0].Y;
        yMax = sites_[sites_.Count - 1].Y;
        
        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    public List<uint> SiteColor() {
        List<uint> colors = new List<uint>();
        Site site;
        for (int i = 0; i < sites_.Count; i++) {
            site = sites_[i];
            colors.Add(site.color);
        }

        return colors;
    }

    public List<Vector2> SitePositions() {
        List<Vector2> positions = new List<Vector2>();

        Site site;
        for (int i = 0; i < sites_.Count; i++) {
            site = sites_[i];
            positions.Add(site.Position);
        }

        return positions;
    }

    public List<Circle> Circles() {
        List<Circle> circles = new List<Circle>();

        Site site;
        for (int i = 0; i < sites_.Count; i++) {
            site = sites_[i];
            float radius = 0.0f;
            Edge nearestEdge = site.NearestEdge();

            if (!nearestEdge.IsPartOfConvexHull()) {
                radius = nearestEdge.SitesDistance() * 0.5f;
            }
            
            circles.Add(new Circle(site.X, site.Y, radius));
        }

        return circles;
    }

    public List<List<Vector2>> Regions(Rect plotBounds) {
        List<List<Vector2>> regions = new List<List<Vector2>>();

        Site site;
        for (int i = 0; i < sites_.Count; i++) {
            site = sites_[i];
            regions.Add(site.Region(plotBounds));
        }

        return regions;
    }

    public Nullable<Vector2> NearestSitePoint(float x, float y) {
        return null;
    }
}
}
