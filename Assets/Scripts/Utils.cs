using UnityEngine;
using System.Collections;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;

public static class Polyhedron {
    public static Polyhedron<T> CreatePolyhedron<T>(IEnumerable<T> vertexes, IEnumerable<Edge<T>> edges, IEnumerable<Face<T>> faces) {
        return new Polyhedron<T>(vertexes.ToReadOnly(), edges.ToReadOnly(), faces.ToReadOnly());
    }

    public static Polyhedron<Vector3> Project(this Polyhedron<Vector4> polyhedron, Vector4 projectionPoint, HyperPlane4 plane) {
        return polyhedron.FMap(x => x.Project(projectionPoint, plane));
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
public static class LinqExtensions {
    public static ReadOnlyCollection<T> ToReadOnly<T>(this IEnumerable<T> source) {
        return new ReadOnlyCollection<T>(source.ToArray());
    }
    public static IEnumerable<T> Yield<T>(this T value) {
        return new[] { value };
    }
    public static IEnumerable<TNew> Zip<T1, T2, TNew>(this IEnumerable<T1> en1, IEnumerable<T2> en2, Func<T1, T2, TNew> zip) {
        var x1 = en1.GetEnumerator();
        var x2 = en2.GetEnumerator();
        while(true) {
            var next1 = x1.MoveNext();
            var next2 = x2.MoveNext();
            if(next1 && next1) {
                yield return zip(x1.Current, x2.Current);
            } else if(!next1 && !next1) {
                yield break;
            } else {
                throw new InvalidOperationException();
            }
        }
    }
    public static IEnumerable<T> HeadToTail<T>(this IEnumerable<T> source) {
        return source.Skip(1).Concat(source.Take(1));
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
    public readonly Func<Polyhedron<Vector3>> GetPoly;
    public readonly Func<Matrix4x4, Matrix4x4> AlternateRotationMatrix;

    public PolyInfo(Func<Polyhedron<Vector3>> getPoly, Func<Matrix4x4, Matrix4x4> alternateRotationMatrix) {
        GetPoly = getPoly;
        AlternateRotationMatrix = alternateRotationMatrix;
    }
}
