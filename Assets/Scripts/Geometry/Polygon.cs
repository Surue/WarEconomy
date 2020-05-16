using System.Collections.Generic;
using UnityEngine;

namespace Geometry {
public sealed class Polygon {
    public List<Vector2> vertices;

    public Polygon(List<Vector2> vertices) {
        this.vertices = vertices;
    }

    public float Area() {
        int index;
        int n = vertices.Count;
        float signedDoubleArea = 0; 
        
        for (index = 0; index < n; ++index) {
            int nextIndex = (index + 1) % n;
            Vector2 point = vertices [index];
            Vector2 next = vertices [nextIndex];
            signedDoubleArea += point.x * next.y - next.x * point.y;
        }
        
        return signedDoubleArea;
    }

    public bool IsClockwise() {
        return Area() < 0;
    }
}
}
