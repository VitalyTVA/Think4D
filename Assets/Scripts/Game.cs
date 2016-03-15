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

    PolyInfo polyInfo = Polyhedron.Cube4D.ToPolyInfo();
    //PolyInfo polyInfo = Polyhedron.Cube3D.ToPolyInfo();
    void Start() {

        var polyhedron = GetPoly(Matrix4x4.identity);

        faces = CreateFaces(faceMaterial);

        root = new GameObject("Cube");
        root.transform.position = Vector3.zero;

        vertexes = polyhedron.Vertexes.Select(x => AddVertex(vertex, root)).ToArray();
        edges = polyhedron.Edges.Select(x => AddEdge(edge, root)).ToArray();

        rotationHelper = new RotationHelper(x => {
            polyRotation = x;
        }, () => polyRotation, x => x, 0);
        rotationHelper2 = new RotationHelper(x => {
            polyRotation = x;
        }, () => polyRotation, polyInfo.AlternateRortationMatrix, 1);
    }

    static GameObject CreateFaces(Material material) {
        var faces = new GameObject("Faces", typeof(MeshFilter), typeof(MeshRenderer));
        faces.GetComponent<MeshRenderer>().material = material;
        var mf = faces.GetComponent<MeshFilter>();
        var mesh = new Mesh();
        mf.mesh = mesh;
        return faces;
    }

    static void UpdateFaces(MeshFilter mf, Polyhedron<Vector3> polyhedron) {
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
    }

    Polyhedron<Vector3> GetPoly(Matrix4x4 m) {
        return polyInfo.GetPoly(m);
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

        UpdateFaces(faces.GetComponent<MeshFilter>(), rotatedPolyhendron);
        //var mf = faces.GetComponent<MeshFilter>();
        //mf.mesh.vertices = mf.mesh.vertices.Select(x => (polyRotation * (Vector4)x).Reduce()).ToArray();
        //mf.mesh.normals = mf.mesh.vertices.Select(x => (polyRotation * (Vector4)x).Reduce()).ToArray();
    }
}