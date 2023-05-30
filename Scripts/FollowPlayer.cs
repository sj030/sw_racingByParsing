/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform target;
    //public float dist = 10.0f;
    //public float height = 5.0f;

    public float dist = 0.0f;
    public float height = 0.0f;

    public float smoothRotate = 5.0f;
    private Transform tr;

    void Start()
    {
        tr = GetComponent<Transform>();
    }
    void LateUpdate()
    {
        float currTAngle = Mathf.LerpAngle(tr.eulerAngles.y, target.eulerAngles.y, smoothRotate * Time.deltaTime);
        Quaternion rot = Quaternion.Euler(0, currTAngle, 0);
        tr.position = target.position - (rot * Vector3.forward * dist) + (Vector3.up * height);
        tr.LookAt(target);
    }
}*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform target;
    public float height = 1.5f;
    //public float smoothRotate = 5.0f;
    private Transform tr;

    void Start()
    {
        tr = GetComponent<Transform>();
    }

    void LateUpdate()
    {
        // x, y는 자동차랑 똑같음
        // y는 1.5
        tr.position = target.position + new Vector3(0, 1.5f, 0); 
        tr.rotation = target.rotation;


    }
}
