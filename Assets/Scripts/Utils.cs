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
    public const float SimplexSize = CubeSize * 2;
    public const float OrthoplexSize = 2;

    public static readonly Polyhedron<Void> Point
        = Create(Void.Instance.Yield(), Enumerable.Empty<Edge<Void>>(), Enumerable.Empty<Face<Void>>());

    #region cube
    public static readonly Polyhedron<float> Cube1D
        = MakePrism<Void, float>(Point, Expand0, CubeSize);

    public static readonly Polyhedron<Vector2> Cube2D
        = MakePrism<float, Vector2>(Cube1D, Expand1, CubeSize);

    public static readonly Polyhedron<Vector3> Cube3D
        = MakePrism<Vector2, Vector3>(Cube2D, Expand2, CubeSize);

    public static readonly Polyhedron<Vector4> Cube4D
        = MakePrism<Vector3, Vector4>(Cube3D, Expand3, CubeSize);

    static Polyhedron<TNPlus1> MakePrism<TN, TNPlus1>(this Polyhedron<TN> polyhedron, Func<TN, float, TNPlus1> addDimension, float newDimensionSize) {
        var bottom = polyhedron.FMap((TN x) => addDimension(x, -newDimensionSize / 2));
        var top = polyhedron.FMap((TN x) => addDimension(x, newDimensionSize / 2));
        var newEdges = bottom.Vertexes.Zip(top.Vertexes, (v1, v2) => new Edge<TNPlus1>(v1, v2));
        var faces = CombinePrismFaces(bottom, top);
        return Create(
            bottom.Vertexes.Concat(top.Vertexes),
            bottom.Edges.Concat(top.Edges).Concat(newEdges),
            faces
            );
    }
    static IEnumerable<Face<T>> CombinePrismFaces<T>(Polyhedron<T> top, Polyhedron<T> bottom) {
        var newFaces = top.Edges.Zip(bottom.Edges,
            (x, y) => Face.Create(new[] { x.Vertex1, x.Vertex2, y.Vertex2, y.Vertex1 }));
        return top.Faces.Concat(bottom.Faces).Concat(newFaces);
    }
    #endregion

    #region simplex
    public static readonly Polyhedron<float> Simplex1D
        = MakeSimplex<Void, float>(Point, Expand0, SimplexSize);

    public static readonly Polyhedron<Vector2> Simplex2D
        = MakeSimplex<float, Vector2>(Simplex1D, Expand1, SimplexSize);

    public static readonly Polyhedron<Vector3> Simplex3D
        = MakeSimplex<Vector2, Vector3>(Simplex2D, Expand2, SimplexSize);

    public static readonly Polyhedron<Vector4> Simplex4D
        = MakeSimplex<Vector3, Vector4>(Simplex3D, Expand3, SimplexSize);

    static Polyhedron<TNPlus1> MakeSimplex<TN, TNPlus1>(this Polyhedron<TN> simplex, Func<TN, float, TNPlus1> addDimension, float edgeSize) {
        var n = (float)simplex.Vertexes.Count;
        var r = edgeSize * Mathf.Sqrt(n / (2 * (n + 1)));
        var d = r / n;

        var bottom = simplex.FMap((TN x) => addDimension(x, -d));
        var top = addDimension(default(TN), r);
        var newFaces = bottom.Vertexes.Zip(bottom.Vertexes.HeadToTail(), (x, y) => Face.Create(new[] { top, x, y }));
        return Create(
            bottom.Vertexes.Concat(top.Yield()),
            bottom.Edges.Concat(bottom.Vertexes.Select(x => new Edge<TNPlus1>(x, top))),
            bottom.Faces.Concat(newFaces)
            );
    }
    #endregion

    #region orthoplex
    public static readonly Polyhedron<float> Orthoplex1D
        = MakeSimplex<Void, float>(Point, Expand0, OrthoplexSize * Mathf.Sqrt(2));

    public static readonly Polyhedron<Vector2> Orthoplex2D
        = MakeOrthoplex<float, Vector2>(Orthoplex1D, Expand1, OrthoplexSize);

    public static readonly Polyhedron<Vector3> Orthoplex3D
        = MakeOrthoplex<Vector2, Vector3>(Orthoplex2D, Expand2, OrthoplexSize);

    public static readonly Polyhedron<Vector4> Orthoplex4D
        = MakeOrthoplex<Vector3, Vector4>(Orthoplex3D, Expand3, OrthoplexSize);

    static Polyhedron<TNPlus1> MakeOrthoplex<TN, TNPlus1>(this Polyhedron<TN> orthoplex, Func<TN, float, TNPlus1> addDimension, float edgeSize) {
        var r = edgeSize / Mathf.Sqrt(2);
        var top = addDimension(default(TN), r);
        var bottom = addDimension(default(TN), -r);

        var baseOrthoplex = orthoplex.FMap<TNPlus1>(x => addDimension(x, 0));
        var oldEdges = baseOrthoplex.Vertexes.Count > 2
            //? baseOrthoplex.Vertexes.Zip(baseOrthoplex.Vertexes.HeadToTail(), (x, y) => new Edge<TNPlus1>(x, y)) 
            ? baseOrthoplex.Edges
            : Enumerable.Empty<Edge<TNPlus1>>();
        var newTopEdges = baseOrthoplex.Vertexes.Select(x => new Edge<TNPlus1>(x, top));
        var newBottomEdges = baseOrthoplex.Vertexes.Select(x => new Edge<TNPlus1>(x, bottom));
        var newFaces = Enumerable.Empty<Face<TNPlus1>>();
        return Create(
            baseOrthoplex.Vertexes.Concat(new[] { top, bottom }),
            oldEdges.Concat(newTopEdges).Concat(newBottomEdges),
            newFaces
            );
    }
    #endregion


    public static PolyInfo ToPolyInfo(this Polyhedron<Vector3> poly) {
        return new PolyInfo(m => poly.FMap(x => (m * x.Expand3()).Reduce4()), m => m);
    }
    public static PolyInfo ToPolyInfo(this Polyhedron<Vector4> poly, Func<float> getD4Zoom) {
        return new PolyInfo(
            m => poly.FMap(x => m * x).Project(new Vector4(0, 0, 0, getD4Zoom()), new HyperPlane4(Vector4.zero, new Vector4(0, 0, 0, 1))),
            m => {
                m[3, 0] = m[2, 0];
                m[3, 1] = m[2, 1];
                m[0, 3] = m[0, 2];
                m[1, 3] = m[1, 2];
                m[3, 3] = m[2, 2];

                m[2, 0] = 0;
                m[2, 1] = 0;
                m[0, 2] = 0;
                m[1, 2] = 0;
                m[2, 2] = 1;
                return m;
            });
    }

    public static Polyhedron<T> Create<T>(IEnumerable<T> vertexes, IEnumerable<Edge<T>> edges, IEnumerable<Face<T>> faces) {
        return new Polyhedron<T>(vertexes.ToReadOnly(), edges.ToReadOnly(), faces.ToReadOnly());
    }


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
    public readonly Func<Matrix4x4, Polyhedron<Vector3>> GetPoly;
    public readonly Func<Matrix4x4, Matrix4x4> AlternateRotationMatrix;
    public PolyInfo(Func<Matrix4x4, Polyhedron<Vector3>> getPoly, Func<Matrix4x4, Matrix4x4> alternateRortationMatrix) {
        GetPoly = getPoly;
        AlternateRotationMatrix = alternateRortationMatrix;
    }
}
