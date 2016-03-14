using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public class Game : MonoBehaviour {
    public GameObject vertex;
    public GameObject edge;
    public Material faceMaterial;
    public float rotationSpeed = 1;
    GameObject[] vertexes, edges;
    GameObject root;
    GameObject faces;
    RotationHelper rotationHelper;
    RotationHelper rotationHelper2;
    Matrix4x4 polyRotation = Matrix4x4.identity;
    void Start() {

        var polyhedron = GetPoly(Matrix4x4.identity);

        faces = CreateFaces(faceMaterial, Polyhedron.Cube3D);

        root = new GameObject("Cube");
        root.transform.position = Vector3.zero;

        vertexes = polyhedron.Vertexes.Select(x => AddVertex(vertex, root)).ToArray();
        edges = polyhedron.Edges.Select(x => AddEdge(edge, root)).ToArray();

        rotationHelper = new RotationHelper(x => {
            polyRotation = x;
        }, () => polyRotation, x => x, 0);
        rotationHelper2 = new RotationHelper(x => {
            polyRotation = x;
        }, () => polyRotation, rotation => {
            rotation[3, 0] = rotation[2, 0];
            rotation[3, 1] = rotation[2, 1];
            rotation[0, 3] = rotation[0, 2];
            rotation[1, 3] = rotation[1, 2];
            rotation[3, 3] = rotation[2, 2];

            rotation[2, 0] = 0;
            rotation[2, 1] = 0;
            rotation[0, 2] = 0;
            rotation[1, 2] = 0;
            rotation[2, 2] = 1;

            return rotation;
        }, 1);
    }

    static GameObject CreateFaces(Material material, Polyhedron<Vector3> polyhedron) {
        var faces = new GameObject("Faces", typeof(MeshFilter), typeof(MeshRenderer));
        faces.GetComponent<MeshRenderer>().material = material;
        var mf = faces.GetComponent<MeshFilter>();
        var mesh = new Mesh();
        mf.mesh = mesh;


        var faceMeshes = polyhedron.Faces.Select(face => {
            var vertices = face.Vertexes;

            var triangles = vertices.Skip(1).Take(face.Vertexes.Count - 2).SelectMany((x, i) => new[] { 0, i + 1, i + 2 });

            var normal = Vector3.Cross(vertices[1] - vertices[0], vertices[2] - vertices[0]).normalized;
            var normals = Enumerable.Repeat(normal, face.Vertexes.Count);

            var uvs = Enumerable.Repeat(new Vector2(0, 0), face.Vertexes.Count);
            return new {
                vertices, triangles, normals, uvs
            };
        });

        mesh.vertices = faceMeshes.SelectMany(x => x.vertices).ToArray();
        var trianglesCount = 0;
        mesh.triangles = faceMeshes.SelectMany(x => {
            var result = x.triangles.Concat(x.triangles.Reverse()).Select(i => i + trianglesCount).ToArray();
            trianglesCount += x.vertices.Count;
            return result;
        }).ToArray();
        mesh.normals = faceMeshes.SelectMany(x => x.normals).ToArray();
        mesh.uv = faceMeshes.SelectMany(x => x.uvs).ToArray();

        return faces;
    }

    static GameObject CreateFaces_(Material material, Polyhedron<Vector3> polyhedron) {
        var width = 2;
        var height = 2;
        var faces = new GameObject("Faces", typeof(MeshFilter), typeof(MeshRenderer));
        faces.GetComponent<MeshRenderer>().material = material;
        var mf = faces.GetComponent<MeshFilter>();
        var mesh = new Mesh();
        mf.mesh = mesh;

        var vertices = new Vector3[4];

        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(width, 0, 0);
        vertices[2] = new Vector3(0, height, 0);
        vertices[3] = new Vector3(width, height, 0);

        mesh.vertices = vertices;

        var tri = new int[6];

        tri[0] = 0;
        tri[1] = 2;
        tri[2] = 1;

        tri[3] = 2;
        tri[4] = 3;
        tri[5] = 1;

        mesh.triangles = tri;

        var normals = new Vector3[4];

        normals[0] = -Vector3.forward;
        normals[1] = -Vector3.forward;
        normals[2] = -Vector3.forward;
        normals[3] = -Vector3.forward;

        mesh.normals = normals;

        var uv = new Vector2[4];

        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 1);
        uv[3] = new Vector2(1, 1);

        mesh.uv = uv;

        return faces;
    }

    static Polyhedron<Vector3> GetPoly(Matrix4x4 m) {
        return Polyhedron.Cube4D.FMap(x => m * x).Project(new Vector4(0, 0, 0, 3), new HyperPlane4(Vector4.zero, new Vector4(0, 0, 0, 1)));
    }

    static GameObject AddVertex(GameObject prefab, GameObject parent) {
        var vertex = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
        vertex.transform.parent = parent.transform;
        return vertex;
    }

    static GameObject AddEdge(GameObject prefab, GameObject parent) {
        var edge = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
        edge.transform.parent = parent.transform;
        return edge;
    }

    static void UpdateEdge(GameObject edge, Vector3 from, Vector3 to) {
        var diff = to - from;
        var len = diff.magnitude;
        edge.transform.localPosition = (from + to) / 2;
        edge.transform.localScale = new Vector3(edge.transform.localScale.x, len / 2, edge.transform.localScale.z);
        edge.transform.localRotation = Quaternion.LookRotation(from - to, Vector3.up) * Quaternion.Euler(90, 0, 0);
    }

    void Update () {
        rotationHelper.Update();
        rotationHelper2.Update();

        var rotatedPolyhendron = GetPoly(polyRotation);

        for(int i = 0; i < vertexes.Length; i++) {
            vertexes[i].transform.localPosition = rotatedPolyhendron.Vertexes[i];
        }
        for(int i = 0; i < edges.Length; i++) {
            UpdateEdge(edges[i], rotatedPolyhendron.Edges[i].Vertex1, rotatedPolyhendron.Edges[i].Vertex2);
        }
    }
}
