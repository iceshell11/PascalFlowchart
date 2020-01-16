using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public static InputController Instance { get; private set; }
    public Camera Camera;
    public float ScrollSpeed = 10;
    public void Awake()
    {
        Instance = this;
    }
    public static Vector2 GetWorldPoint(Vector2 screenPoint)
    {
        return Instance.Camera.ScreenToWorldPoint(screenPoint);
    }
    public static Vector2? GetMousePosition()
    {
        if (Input.GetMouseButton(0))
        {
            return Instance.Camera.ScreenToWorldPoint(Input.mousePosition);
        }

        return null;
    }

    void FixedUpdate()
    {
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            Camera.orthographicSize = Mathf.Clamp(Camera.orthographicSize += scroll * ScrollSpeed, 10, 1400);
        }
    }
}
