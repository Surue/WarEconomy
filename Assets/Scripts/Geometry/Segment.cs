using System;
using UnityEngine;

namespace Geometry {
public sealed class Segment {
    public Nullable<Vector3> p0;
    public Nullable<Vector3> p1;

    public Segment(Nullable<Vector3> p0, Nullable<Vector3> p1) {
        this.p0 = p0;
        this.p1 = p1;
    }

    public static int CompareLengthsMax(Segment s0, Segment s1) {
        float length0 = Vector3.Distance((Vector3)s0.p0, (Vector3)s0.p1);
        float length1 = Vector3.Distance((Vector3)s1.p0, (Vector3)s1.p1);

        if (length0 < length1) {
            return 1;
        }

        if (length0 > length1) {
            return -1;
        }

        return 0;
    }

    public static int CompareLengths(Segment s0, Segment s1) {
        return -CompareLengthsMax(s0, s1);
    }
    
    public override string ToString() {
        return "Segment (p0 = " + p0 + ", p1 = " + p1 + ")";
    }
}
}
