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
        Polyhedrons.Orthoplex2D.FMap(x => x.Expand2()).ToPolyInfo(),
        Polyhedrons.Orthoplex3D.ToPolyInfo(),
        Polyhedrons.Orthoplex4D.ToPolyInfo(GetD4Zoom),
        Polyhedrons.Simplex3D.ToPolyInfo(),
        Polyhedrons.Simplex4D.ToPolyInfo(GetD4Zoom),
        Polyhedrons.Cube3D.ToPolyInfo(),
        Polyhedrons.Cube4D.ToPolyInfo(GetD4Zoom),
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