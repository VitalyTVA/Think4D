using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public class Game : MonoBehaviour {
    public GameObject vertex;
    public GameObject edge;
    public Material faceMaterial;
    public float rotationSpeed = 1;
    RotationHelper rotationHelper;
    RotationHelper rotationHelper2;
    Matrix4x4 polyRotation = Matrix4x4.identity;

    PolyModel polyModel;

    static readonly PolyInfo[] Infos = new PolyInfo[] {
        Polyhedron.Simplex3D.ToPolyInfo(),
        Polyhedron.Simplex4D.ToPolyInfo(),
        Polyhedron.Cube4D.ToPolyInfo(),
        Polyhedron.Cube3D.ToPolyInfo(),
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