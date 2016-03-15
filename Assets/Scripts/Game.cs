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

    void Start() {
        polyModel = new PolyModel(this, Polyhedron.Cube4D.ToPolyInfo());


        rotationHelper = new RotationHelper(x => {
            polyRotation = x;
        }, () => polyRotation, x => x, 0);
        rotationHelper2 = new RotationHelper(x => {
            polyRotation = x;
        }, () => polyRotation, polyModel.polyInfo.AlternateRotationMatrix, 1);

    }

    
    void Update () {
        rotationHelper.Update();
        rotationHelper2.Update();

        polyModel.Update(polyRotation);

    }
}