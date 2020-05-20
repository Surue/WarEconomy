using System;
using UnityEngine;

namespace Geometry {
public sealed class Segment2D {
    public Nullable<Vector2> p0;
    public Nullable<Vector2> p1;

    public Segment2D(Nullable<Vector2> p0, Nullable<Vector2> p1) {
        this.p0 = p0;
        this.p1 = p1;
    }

    public static int CompareLengthsMax(Segment2D s0, Segment2D s1) {
        float length0 = Vector2.Distance((Vector2)s0.p0, (Vector2)s0.p1);
        float length1 = Vector2.Distance((Vector2)s1.p0, (Vector2)s1.p1);

        if (length0 < length1) {
            return 1;
        }

        if (length0 > length1) {
            return -1;
        }

        return 0;
    }

    public static int CompareLengths(Segment2D s0, Segment2D s1) {
        return -CompareLengthsMax(s0, s1);
    }
}
}
