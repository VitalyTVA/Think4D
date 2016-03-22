using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public class Game : MonoBehaviour {
    public GameObject vertex;
    public GameObject edge;
    public Material faceMaterial;
    public float zoomSpeed = 1;
    RotationHelper rotationHelper3D;
    RotationHelper rotationHelper;
    RotationHelper rotationHelper2;
    Matrix4x4 rotation4D = Matrix4x4.identity;
    Matrix4x4 rotation3D = Matrix4x4.identity;

    PolyModel polyModel;
    float D4Zoom = 1.8f; //TODO not static

    float GetD4Zoom() {
        return D4Zoom;
    }

    PolyInfo[] Infos;

    int currentIndex = 0;

    void Start() {
        Infos = new PolyInfo[] {
            Polyhedrons.Orthoplex2D.FMap(x => x.Expand2()).ToPolyInfo(),
            Polyhedrons.Orthoplex3D.ToPolyInfo(),
            Polyhedrons.Orthoplex4D.ToPolyInfo(GetD4Zoom),
            Polyhedrons.Simplex3D.ToPolyInfo(),
            Polyhedrons.Simplex4D.ToPolyInfo(GetD4Zoom),
            Polyhedrons.Cube3D.ToPolyInfo(),
            Polyhedrons.Cube4D.ToPolyInfo(GetD4Zoom),
        };


        polyModel = CreateNextModel();

        rotationHelper3D = new RotationHelper(x => {
            rotation3D = x;
        }, () => rotation3D, x => x, 0);
        rotationHelper = new RotationHelper(x => {
            rotation4D = x;
        }, () => rotation4D, x => x, 0);
        rotationHelper2 = new RotationHelper(x => {
            rotation4D = x;
        }, () => rotation4D, m => polyModel.PolyInfo.AlternateRotationMatrix(m), 1);

    }

    
    void Update () {
        if(Input.GetMouseButtonDown(2)) {
            polyModel.Destroy();
            polyModel = CreateNextModel();
            //rotation4D = Matrix4x4.identity;
        }

        var zoom = zoomSpeed * Input.GetAxis("Mouse ScrollWheel");
        if(Input.GetButton("Fire1"))
            D4Zoom += zoom;
        else
            Camera.main.transform.Translate(0, 0, zoom);

        rotationHelper3D.Update();
        rotationHelper.Update();
        rotationHelper2.Update();

        polyModel.Update(polyModel.PolyInfo.Id4D ? rotation4D : rotation3D);

    }

    private PolyModel CreateNextModel() {
        var index = currentIndex % Infos.Length;
        currentIndex++;
        return new PolyModel(this, Infos[index]);
    }
}