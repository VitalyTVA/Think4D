using UnityEngine;
using System.Collections;
using System;

class RotationHelper {
    Matrix4x4 baseRotation = Matrix4x4.identity, newRotation = Matrix4x4.identity;
    Vector3 basePos;
    readonly Action<Matrix4x4> setRotation;
    readonly Func<Matrix4x4> getRotation;
    readonly Func<Matrix4x4, Matrix4x4> coerceMatrix;
    readonly int button;

    public RotationHelper(Action<Matrix4x4> setRotation, Func<Matrix4x4> getRotation, Func<Matrix4x4, Matrix4x4> coerceMatrix, int button) {
        this.setRotation = setRotation;
        this.getRotation = getRotation;
        this.coerceMatrix = coerceMatrix;
        this.button = button;
    }

    public void Update() {
        if(Input.GetMouseButtonDown(button)) {
            basePos = Input.mousePosition;
            baseRotation = getRotation();
            newRotation = Matrix4x4.identity;
        }
        if(Input.GetMouseButton(button)) {
            var curPos = Input.mousePosition;
            curPos.z = 20;
            var prevPos = basePos;
            prevPos.z = 20;
            Ray ray1 = Camera.main.ScreenPointToRay(prevPos);
            Ray ray2 = Camera.main.ScreenPointToRay(curPos);
            var intersection1 = ray1.NearestIntersectWithOriginsSphere(1);
            var intersection2 = ray2.NearestIntersectWithOriginsSphere(1);
            if(intersection1 != null && intersection2 != null && intersection1.Value != intersection2.Value) {
                var value1 = intersection1.Value;
                //value1.z = 0;
                var value2 = intersection2.Value;
                //value2.z = 0;
                var rotation = coerceMatrix(Matrix4x4.TRS(Vector3.zero, Quaternion.FromToRotation(value1, value2), new Vector3(1, 1, 1)));
                var posDiff = Input.mousePosition - basePos;
                if(posDiff.magnitude < 10) {
                    newRotation = rotation;
                } else {
                    baseRotation = (rotation * baseRotation).Normalize();
                    newRotation = Matrix4x4.identity;
                    basePos = Input.mousePosition;
                }
                setRotation(newRotation * baseRotation);
            }
        }
        if(Input.GetMouseButtonUp(button)) {
            //baseRotation = (newRotation * baseRotation).Normalize();
            //newRotation = Matrix4x4.identity;
        }
    }
    //private Matrix4x4 GetM1() {
    //    var m = Matrix4x4.identity;
    //    var c = Mathf.Cos(Mathf.PI * angleH / 180);
    //    var s = Mathf.Sin(Mathf.PI * angleH / 180);
    //    m[3, 3] = c;
    //    m[1, 1] = c;
    //    m[1, 3] = -s;
    //    m[3, 1] = s;
    //    return m;
    //}
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
    //private Matrix4x4 GetM2() {
    //    var m = Matrix4x4.identity;
    //    var c = Mathf.Cos(Mathf.PI * angleV / 180);
    //    var s = Mathf.Sin(Mathf.PI * angleV / 180);
    //    m[0, 0] = c;
    //    m[1, 1] = c;
    //    m[1, 0] = -s;
    //    m[0, 1] = s;
    //    return m;
    //}
}
