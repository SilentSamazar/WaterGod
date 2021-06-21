using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomByScroll : MonoBehaviour
{

    public float ScrollSpeed = 10;

    void Update()
    {
        if(this.gameObject.GetComponent<Camera>().orthographicSize > 1)
        {
            this.gameObject.GetComponent<Camera>().orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * ScrollSpeed;
        }
        else
        {
            if(Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                this.gameObject.GetComponent<Camera>().orthographicSize -= Input.GetAxis("Mouse ScrollWheel") * ScrollSpeed;
            }
        }
    }
}
