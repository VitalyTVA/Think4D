using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public class Game : MonoBehaviour {
    public GameObject vertex;
    public GameObject edge;
    public Material faceMaterial;
    public float zoomSpeed = 1;
    RotationHelper rotationHelper;
    RotationHelper rotationHelper2;
    Matrix4x4 polyRotation = Matrix4x4.identity;

    PolyModel polyModel;
    static float D4Zoom = 1.8f; //TODO not static

    static float GetD4Zoom() {
        return D4Zoom;
    }

    static readonly PolyInfo[] Infos = new PolyInfo[] {
        Polyhedron.Orthoplex2D.FMap(x => x.Expand2()).ToPolyInfo(),
        Polyhedron.Orthoplex3D.ToPolyInfo(),
        Polyhedron.Orthoplex4D.ToPolyInfo(GetD4Zoom),
        Polyhedron.Simplex3D.ToPolyInfo(),
        Polyhedron.Simplex4D.ToPolyInfo(GetD4Zoom),
        Polyhedron.Cube3D.ToPolyInfo(),
        Polyhedron.Cube4D.ToPolyInfo(GetD4Zoom),
    };

    int currentIndex = 0;

    void Start() {
        polyModel = CreateNextModel();

        rotationHelper = new RotationHelper(x => {
            polyRotation = x;
        }, () => polyRotation, x => x, 0);
        rotationHelper2 = new RotationHelper(x => {
            polyRotation = x;
        }, () => polyRotation, m => polyModel.polyInfo.AlternateRotationMatrix(m), 1);

    }

    
    void Update () {
        if(Input.GetMouseButtonDown(2)) {
            polyModel.Destroy();
            polyModel = CreateNextModel();
            polyRotation = Matrix4x4.identity;
        }

        var zoom = zoomSpeed * Input.GetAxis("Mouse ScrollWheel");
        if(Input.GetButton("Fire1"))
            D4Zoom += zoom;
        else
            Camera.main.transform.Translate(0, 0, zoom);

        rotationHelper.Update();
        rotationHelper2.Update();

        polyModel.Update(polyRotation);

    }

    private PolyModel CreateNextModel() {
        var index = currentIndex % Infos.Length;
        currentIndex++;
        return new PolyModel(this, Infos[index]);
    }
}