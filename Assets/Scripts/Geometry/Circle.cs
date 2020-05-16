using UnityEngine;

namespace Geometry {
public sealed class Circle {
    public Vector2 position;
    public float radius;

    public Circle(float x, float y, float r) {
        position = new Vector2(x, y);
        radius = r;
    }

    public Circle(Vector2 pos, float r) {
        position = pos;
        radius = r;
    }

    public override string ToString() {
        return "Circle (pos = " + position + ", radius = " + radius + ")";
    }
}
}
