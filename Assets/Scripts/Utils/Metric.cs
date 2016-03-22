using System.Collections;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Metrics {
    public static float Expand0(Void @void, float nextCoord) {
        return nextCoord;
    }
    public static Vector2 Expand1(float x, float nextCoord = 0) {
        return new Vector2(x, nextCoord);
    }
    public static Vector3 Expand2(this Vector2 x, float nextCoord = 0) {
        return new Vector3(x.x, x.y, nextCoord);
    }
    public static Vector3 Reduce4(this Vector4 p) {
        return new Vector3(p.x, p.y, p.z);
    }
    public static Vector4 Expand3(this Vector3 p, float next = 0) {
        return new Vector4(p.x, p.y, p.z, next);
    }
}
