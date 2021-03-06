﻿using UnityEngine;
using System.Collections;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;

public static class Polytope {
    public static Polytope<T> Create<T>(IEnumerable<T> vertexes, IEnumerable<Edge<T>> edges, IEnumerable<Face<T>> faces) {
        return new Polytope<T>(vertexes.ToReadOnly(), edges.ToReadOnly(), faces.ToReadOnly());
    }

    public static Polytope<Vector3> Project(this Polytope<Vector4> polychoron, Vector4 projectionPoint, HyperPlane4 plane) {
        return polychoron.FMap(x => x.Project(projectionPoint, plane));
    }
    public static Vector3 Project(this Vector4 point, Vector4 projectionPoint, HyperPlane4 plane) {
        var line = Line4.LineFromTo(point, projectionPoint);
        var intersection = line.IntersectWith(plane);
        return intersection.Reduce4();
    }
    public static Vector3? NearestIntersectWithOriginsSphere(this Ray ray, float r) {
        var a = ray.direction.sqrMagnitude;
        var b = 2 * Vector3.Dot(ray.origin, ray.direction);
        var c = ray.origin.sqrMagnitude - r * r;
        var t = SolveQuadratic(1, -7, 12);
        var roots = SolveQuadratic(a, b, c);
        if(roots == null)
            return null;
        return ray.GetPoint(roots.Value.x);
    }
    static Vector2? SolveQuadratic(float a, float b, float c) {
        var d = b * b - 4 * a * c;
        if(d < 0)
            return null;
        return new Vector2((-b - Mathf.Sqrt(d)) / (2 * a), (-b + Mathf.Sqrt(d)) / (2 * a));
    }
    public static Matrix4x4 Normalize(this Matrix4x4 m) {
        //var det = m.determinant;
        //for(int i = 0; i < 16; i++) {
        //    m[i] = m[i] / det;
        //}
        return m;
    }

}
public struct Line4 {
    public static Line4 LineFromTo(Vector4 from, Vector4 to) {
        return new Line4(from, from - to);
    }

    //Point + Direction * t
    public readonly Vector4 Point;
    public readonly Vector4 Direction;

    public Line4(Vector4 point, Vector4 direction) {
        Point = point;
        Direction = direction;
    }
    public Vector4 IntersectWith(HyperPlane4 plane) {
        var t = Vector4.Dot((plane.Point - Point), plane.Normal) / Vector4.Dot(Direction, plane.Normal);
        return Point + Direction * t;
    }
}
public struct HyperPlane4 {
    //((x - Point), Normal) = 0
    public readonly Vector4 Point;
    public readonly Vector4 Normal;

    public HyperPlane4(Vector4 point, Vector4 normal) {
        Point = point;
        Normal = normal;
    }
}
public class PolyInfo {
    public readonly Func<Polytope<Vector3>> GetPoly;
    public readonly Func<Matrix4x4, Matrix4x4> AlternateRotationMatrix;

    public PolyInfo(Func<Polytope<Vector3>> getPoly, Func<Matrix4x4, Matrix4x4> alternateRotationMatrix) {
        GetPoly = getPoly;
        AlternateRotationMatrix = alternateRotationMatrix;
    }
}
