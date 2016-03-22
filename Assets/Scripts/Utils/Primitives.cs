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
        return Polyhedron.CreatePolyhedron(Vertexes.Select(map), Edges.Select(x => x.FMap(map)), Faces.Select(x => x.FMap(map)));
    }
}
