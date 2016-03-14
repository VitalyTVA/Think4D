using UnityEngine;
using System.Collections;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;

public class Void {
    public static readonly Void Instance = new Void();
    Void() { }
}
public class Edge<T> {
    public readonly T Vertex1, Vertex2;
    public Edge(T vertex1, T vertex2) {
        Vertex1 = vertex1;
        Vertex2 = vertex2;
    }
    public Edge<TNew> FMap<TNew>(Func<T, TNew> map) {
        return new Edge<TNew>(map(Vertex1), map(Vertex2));
    }
}
public class Face<T> {
    public readonly ReadOnlyCollection<T> Vertexes;
    public Face(ReadOnlyCollection<T> vertexes) {
        Vertexes = vertexes;
    }
    public Face<TNew> FMap<TNew>(Func<T, TNew> map) {
        return new Face<TNew>(Vertexes.Select(map).ToReadOnly());
    }
}
public static class Face {
    public static Face<T> Create<T>(IEnumerable<T> vertexes) {
        return new Face<T>(vertexes.ToReadOnly());
    }
}
public class Polyhedron<T> {
    public readonly ReadOnlyCollection<T> Vertexes;
    public readonly ReadOnlyCollection<Edge<T>> Edges;
    public readonly ReadOnlyCollection<Face<T>> Faces;
    public Polyhedron(ReadOnlyCollection<T> vertexes, ReadOnlyCollection<Edge<T>> edges, ReadOnlyCollection<Face<T>> faces) {
        Vertexes = vertexes;
        Edges = edges;
        Faces = faces;
    }
    public Polyhedron<TNew> FMap<TNew>(Func<T, TNew> map) {
        return Polyhedron.Create(Vertexes.Select(map), Edges.Select(x => x.FMap(map)), Faces.Select(x => x.FMap(map)));
    }
}
public static class Polyhedron {
    public const float CubeSize = 1;

    public static readonly Polyhedron<Void> Cube0D 
        = Create(Void.Instance.Yield(), Enumerable.Empty<Edge<Void>>(), Enumerable.Empty<Face<Void>>());
   
    public static readonly Polyhedron<float> Cube1D 
        = MakePrism(Cube0D, (x, nextCoord) => nextCoord, CubeSize, (top, bottom) => Enumerable.Empty<Face<float>>());

    public static readonly Polyhedron<Vector2> Cube2D 
        = MakePrism(Cube1D, (x, nextCoord) => new Vector2(x, nextCoord), CubeSize, (top, bottom) => Face.Create(top.Vertexes.Concat(bottom.Vertexes.Reverse())).Yield());

    public static readonly Polyhedron<Vector3> Cube3D 
        = MakePrism(Cube2D, (x, nextCoord) => new Vector3(x.x, x.y, nextCoord), CubeSize, CombinePrismFaces<Vector3>);

    public static readonly Polyhedron<Vector4> Cube4D 
        = MakePrism(Cube3D, (x, nextCoord) => new Vector4(x.x, x.y, x.z, nextCoord), CubeSize, CombinePrismFaces<Vector4>);


    static IEnumerable<Face<T>> CombinePrismFaces<T>(Polyhedron<T> top, Polyhedron<T> bottom) {
        return top.Faces.Concat(bottom.Faces);
    }

    public static Polyhedron<T> Create<T>(IEnumerable<T> vertexes, IEnumerable<Edge<T>> edges, IEnumerable<Face<T>> faces) {
        return new Polyhedron<T>(vertexes.ToReadOnly(), edges.ToReadOnly(), faces.ToReadOnly());
    }

    public static Polyhedron<TNPlus1> MakePrism<TN, TNPlus1>(this Polyhedron<TN> polyhedron, Func<TN, float, TNPlus1> addDimension, float newDimensionSize, 
        Func<Polyhedron<TNPlus1>, Polyhedron<TNPlus1>, IEnumerable<Face<TNPlus1>>> getFaces) {
        var bottom = polyhedron.FMap((TN x) => addDimension(x, -newDimensionSize / 2));
        var top = polyhedron.FMap((TN x) => addDimension(x, newDimensionSize / 2));
        var newEdges = bottom.Vertexes.Zip(top.Vertexes, (v1, v2) => new Edge<TNPlus1>(v1, v2));
        var faces = getFaces(bottom, top);
        return Create(
            bottom.Vertexes.Concat(top.Vertexes),
            bottom.Edges.Concat(top.Edges).Concat(newEdges),
            faces
            );
    }

    public static Vector3 Reduce(this Vector4 p) {
        return new Vector3(p.x, p.y, p.z);
    }

    public static Polyhedron<Vector3> Project(this Polyhedron<Vector4> polyhedron, Vector4 projectionPoint, HyperPlane4 plane) {
        return polyhedron.FMap(x => x.Project(projectionPoint, plane));
    }
    public static Vector3 Project(this Vector4 point, Vector4 projectionPoint, HyperPlane4 plane) {
        var line = Line4.LineFromTo(point, projectionPoint);
        var intersection = line.IntersectWith(plane);
        return intersection.Reduce();
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
    public static Quaternion Normalize(this Quaternion q) {
        Quaternion result;
        float sq = q.x * q.x;
        sq += q.y * q.y;
        sq += q.z * q.z;
        sq += q.w * q.w;
        //detect badness
        //assert(sq > 0.1f);
        float inv = 1.0f / Mathf.Sqrt(sq);
        result.x = q.x * inv;
        result.y = q.y * inv;
        result.z = q.z * inv;
        result.w = q.w * inv;
        return result;
    }
    public static Matrix4x4  Normalize(this Matrix4x4 m) {
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
