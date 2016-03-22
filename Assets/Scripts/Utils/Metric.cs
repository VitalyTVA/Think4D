using System.Collections;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Metric<T> {
    public readonly Func<T, T, float> Dot;
    public readonly Func<T, float[]> Components;
    public readonly int N;
    public Metric(Func<T, T, float> dot, Func<T, float[]> components, int n) {
        Dot = dot;
        Components = components;
        N = n;
    }
}

public static class Metric {
    public static readonly Metric<float> D1 
        = new Metric<float>((x, y) => x * y, x => new[] { x }, 1);
    public static readonly Metric<Vector2> D2 
        = new Metric<Vector2>((x, y) => Vector2.Dot(x, y), x => new[] { x.x, x.y }, 2);
    public static readonly Metric<Vector3> D3
        = new Metric<Vector3>((x, y) => Vector3.Dot(x, y), x => new[] { x.x, x.y, x.z }, 3);
    public static readonly Metric<Vector4> D4
        = new Metric<Vector4>((x, y) => Vector4.Dot(x, y), x => new[] { x.x, x.y, x.z, x.w }, 4);

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

    public static float SqrMagnitude<T>(this Metric<T> m, T v) {
        return m.Dot(v, v);
    }
    public static float Magnitude<T>(this Metric<T> m, T v) {
        return Mathf.Sqrt(m.Magnitude(v));
    }
}
