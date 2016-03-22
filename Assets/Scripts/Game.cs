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
    float d4Zoom = 1.8f;

    PolyInfo[] Infos;

    int currentIndex = 0;

    void Start() {
        Infos = new PolyInfo[] {
            ToPolyInfo(Polytopes.Orthoplex3D),
            ToPolyInfo(Polytopes.Orthoplex4D),
            ToPolyInfo(Polytopes.Simplex3D),
            ToPolyInfo(Polytopes.Simplex4D),
            ToPolyInfo(Polytopes.Cube3D),
            ToPolyInfo(Polytopes.Cube4D),
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
            d4Zoom += zoom;
        else
            Camera.main.transform.Translate(0, 0, zoom);

        rotationHelper3D.Update();
        rotationHelper.Update();
        rotationHelper2.Update();

        polyModel.Update();

    }

    private PolyModel CreateNextModel() {
        var index = currentIndex % Infos.Length;
        currentIndex++;
        return new PolyModel(this, Infos[index]);
    }

    PolyInfo ToPolyInfo(Polytope<Vector3> poly) {
        return new PolyInfo(() => poly.FMap(x => (rotation3D * x.Expand3()).Reduce4()), m => m);
    }
    PolyInfo ToPolyInfo(Polytope<Vector4> poly) {
        return new PolyInfo(
            () => poly.FMap(x => rotation4D * x).Project(new Vector4(0, 0, 0, d4Zoom), new HyperPlane4(Vector4.zero, new Vector4(0, 0, 0, 1))),
            m => {
                m[3, 0] = m[2, 0];
                m[3, 1] = m[2, 1];
                m[0, 3] = m[0, 2];
                m[1, 3] = m[1, 2];
                m[3, 3] = m[2, 2];

                m[2, 0] = 0;
                m[2, 1] = 0;
                m[0, 2] = 0;
                m[1, 2] = 0;
                m[2, 2] = 1;
                return m;
            });
    }
}