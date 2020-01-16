using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionDebug : MonoBehaviour
{
    public Vector3 position;
    void Update()
    {
        position = transform.position;
    }
}
