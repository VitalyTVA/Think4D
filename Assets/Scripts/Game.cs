﻿using UnityEngine;
using System.Collections;
using System.Linq;

public class Game : MonoBehaviour {
    public GameObject vertex;
    public GameObject edge;
    public float rotationSpeed = 1;
    // Use this for initialization

    //Vector3[] vertexPoints = new Vector3[] {
    //        new Vector3(1, 1, 1),
    //        new Vector3(1, 1, -1),
    //        new Vector3(1, -1, 1),
    //        new Vector3(1, -1, -1),
    //        new Vector3(-1, 1, 1),
    //        new Vector3(-1, 1, -1),
    //        new Vector3(-1, -1, 1),
    //        new Vector3(-1, -1, -1),
    //    };
    GameObject[] vertexes, edges;
    GameObject root;
    void Start() {
        //polyhedron = Polyhedron.Cube3D;
        var polyhedron = GetPoly(Matrix4x4.identity);

        root = new GameObject("Cube");
        root.transform.position = Vector3.zero;
        //cube.transform.rotation = Quaternion.Euler(40, 40, 40);

        vertexes = polyhedron.Vertexes.Select(x => AddVertex(vertex, root)).ToArray();
        edges = polyhedron.Edges.Select(x => AddEdge(edge, root)).ToArray();
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

    float horz, vert, angleH, angleV;

    Quaternion baseRotation, newRotation;
    Vector3 basePos;
    void Update () {
        //var worldPos = Camera.main.ScreenToWorldPoint(mPos);
        //Debug.Log(mPos + " " + worldPos.x.ToString());
        //if(Input.GetMouseButtonDown(0))
        //    Debug.DrawRay(Camera.main.transform.position, worldPos - Camera.main.transform.position, Color.red, 100);

        if(Input.GetMouseButtonDown(0)) {
            baseRotation = root.transform.rotation;
            newRotation = Quaternion.identity;
            basePos = Input.mousePosition;
        }
        if(Input.GetMouseButton(0)) {
            var curPos = Input.mousePosition;
            curPos.z = 20;
            var prevPos = basePos;
            prevPos.z = 20;
            Ray ray1 = Camera.main.ScreenPointToRay(prevPos);
            Ray ray2 = Camera.main.ScreenPointToRay(curPos);
            var intersection1 = ray1.NearestIntersectWithOriginsSphere(1);
            var intersection2 = ray2.NearestIntersectWithOriginsSphere(1);
            if(intersection1 != null && intersection2 != null && intersection1.Value != intersection2.Value) {
                var rotation = Quaternion.FromToRotation(intersection1.Value, intersection2.Value);
                var posDiff = Input.mousePosition - basePos;
                if(posDiff.magnitude < 10) {
                    newRotation = rotation;
                } else {
                    baseRotation = rotation * baseRotation;
                    newRotation = Quaternion.identity;
                    basePos = Input.mousePosition;
                }
                root.transform.rotation = newRotation * baseRotation;
            }
        }
        if(Input.GetMouseButtonUp(0)) {
            root.transform.rotation = newRotation * baseRotation;
        }

        if(Input.GetAxis("Fire1") == 0) {
            horz += rotationSpeed * Input.GetAxis("Horizontal");
            vert += rotationSpeed * Input.GetAxis("Vertical");
        } else {
            angleH += rotationSpeed * Input.GetAxis("Vertical");
            angleV += rotationSpeed * Input.GetAxis("Horizontal");
        }

        //root.transform.localRotation =
        //    Quaternion.Euler(vert, 0, 0)
        //    *
        //    Quaternion.Euler(0, horz, 0);

        Matrix4x4 m = GetM1() * GetM2();

        var rotatedPolyhendron = GetPoly(m);

        for(int i = 0; i < vertexes.Length; i++) {
            vertexes[i].transform.localPosition = rotatedPolyhendron.Vertexes[i];
        }
        for(int i = 0; i < edges.Length; i++) {
            UpdateEdge(edges[i], rotatedPolyhendron.Edges[i].Vertex1, rotatedPolyhendron.Edges[i].Vertex2);
        }
    }

    private Matrix4x4 GetM1() {
        var m = Matrix4x4.identity;
        var c = Mathf.Cos(Mathf.PI * angleH / 180);
        var s = Mathf.Sin(Mathf.PI * angleH / 180);
        m[3, 3] = c;
        m[1, 1] = c;
        m[1, 3] = -s;
        m[3, 1] = s;
        return m;
    }
    //private Matrix4x4 GetM2() {
    //    var m = Matrix4x4.identity;
    //    var c = Mathf.Cos(Mathf.PI * angleV / 180);
    //    var s = Mathf.Sin(Mathf.PI * angleV / 180);
    //    m[3, 3] = c;
    //    m[2, 2] = c;
    //    m[2, 3] = -s;
    //    m[3, 2] = s;
    //    return m;
    //}
    private Matrix4x4 GetM2() {
        var m = Matrix4x4.identity;
        var c = Mathf.Cos(Mathf.PI * angleV / 180);
        var s = Mathf.Sin(Mathf.PI * angleV / 180);
        m[0, 0] = c;
        m[1, 1] = c;
        m[1, 0] = -s;
        m[0, 1] = s;
        return m;
    }
}