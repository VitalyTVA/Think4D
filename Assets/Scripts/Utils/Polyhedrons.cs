using System.Collections;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Polyhedrons {
    public const float CubeSize = 1;
    public const float SimplexSize = CubeSize * 2;
    public const float OrthoplexSize = 2;

    public static readonly Polyhedron<Void> Point
        = Polyhedron.CreatePolyhedron(Void.Instance.Yield(), Enumerable.Empty<Edge<Void>>(), Enumerable.Empty<Face<Void>>());

    #region cube
    public static readonly Polyhedron<float> Cube1D
        = MakePrism<Void, float>(Point, Polyhedron.Expand0, CubeSize);

    public static readonly Polyhedron<Vector2> Cube2D
        = MakePrism<float, Vector2>(Cube1D, Polyhedron.Expand1, CubeSize);

    public static readonly Polyhedron<Vector3> Cube3D
        = MakePrism<Vector2, Vector3>(Cube2D, Polyhedron.Expand2, CubeSize);

    public static readonly Polyhedron<Vector4> Cube4D
        = MakePrism<Vector3, Vector4>(Cube3D, Polyhedron.Expand3, CubeSize);

    static Polyhedron<TNPlus1> MakePrism<TN, TNPlus1>(this Polyhedron<TN> polyhedron, Func<TN, float, TNPlus1> addDimension, float newDimensionSize) {
        var bottom = polyhedron.FMap((TN x) => addDimension(x, -newDimensionSize / 2));
        var top = polyhedron.FMap((TN x) => addDimension(x, newDimensionSize / 2));
        var newEdges = bottom.Vertexes.Zip(top.Vertexes, (v1, v2) => new Edge<TNPlus1>(v1, v2));
        var faces = CombinePrismFaces(bottom, top);
        return Polyhedron.CreatePolyhedron(
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
        = MakeSimplex<Void, float>(Point, Polyhedron.Expand0, SimplexSize);

    public static readonly Polyhedron<Vector2> Simplex2D
        = MakeSimplex<float, Vector2>(Simplex1D, Polyhedron.Expand1, SimplexSize);

    public static readonly Polyhedron<Vector3> Simplex3D
        = MakeSimplex<Vector2, Vector3>(Simplex2D, Polyhedron.Expand2, SimplexSize);

    public static readonly Polyhedron<Vector4> Simplex4D
        = MakeSimplex<Vector3, Vector4>(Simplex3D, Polyhedron.Expand3, SimplexSize);

    static Polyhedron<TNPlus1> MakeSimplex<TN, TNPlus1>(this Polyhedron<TN> simplex, Func<TN, float, TNPlus1> addDimension, float edgeSize) {
        var n = (float)simplex.Vertexes.Count;
        var r = edgeSize * Mathf.Sqrt(n / (2 * (n + 1)));
        var d = r / n;

        var bottom = simplex.FMap((TN x) => addDimension(x, -d));
        var top = addDimension(default(TN), r);
        var newFaces = bottom.Vertexes.Zip(bottom.Vertexes.HeadToTail(), (x, y) => Face.Create(new[] { top, x, y }));
        return Polyhedron.CreatePolyhedron(
            bottom.Vertexes.Concat(top.Yield()),
            bottom.Edges.Concat(bottom.Vertexes.Select(x => new Edge<TNPlus1>(x, top))),
            bottom.Faces.Concat(newFaces)
            );
    }
    #endregion

    #region orthoplex
    public static readonly Polyhedron<float> Orthoplex1D
        = MakeSimplex<Void, float>(Point, Polyhedron.Expand0, OrthoplexSize * Mathf.Sqrt(2));

    public static readonly Polyhedron<Vector2> Orthoplex2D
        = MakeOrthoplex<float, Vector2>(Orthoplex1D, Polyhedron.Expand1, OrthoplexSize);

    public static readonly Polyhedron<Vector3> Orthoplex3D
        = MakeOrthoplex<Vector2, Vector3>(Orthoplex2D, Polyhedron.Expand2, OrthoplexSize);

    public static readonly Polyhedron<Vector4> Orthoplex4D
        = MakeOrthoplex<Vector3, Vector4>(Orthoplex3D, Polyhedron.Expand3, OrthoplexSize);

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
        return Polyhedron.CreatePolyhedron(
            baseOrthoplex.Vertexes.Concat(new[] { top, bottom }),
            oldEdges.Concat(newTopEdges).Concat(newBottomEdges),
            newFaces
            );
    }
    #endregion
}
