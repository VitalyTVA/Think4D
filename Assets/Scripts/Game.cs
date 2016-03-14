using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public class Game : MonoBehaviour {
    public GameObject vertex;
    public GameObject edge;
    public float rotationSpeed = 1;
    GameObject[] vertexes, edges;
    GameObject root;
    RotationHelper rotationHelper;
    RotationHelper rotationHelper2;
    Matrix4x4 polyRotation = Matrix4x4.identity;
    void Start() {
        var polyhedron = GetPoly(Matrix4x4.identity);

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
