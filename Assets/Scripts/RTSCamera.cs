using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTSCamera : MonoBehaviour {

    // Attach to a camera

    float distpixels = 20; // Pixels. The width border at the edge in which the movement work
    float mSpeed = 20; // Scale. Speed of the movement
    float mSpeedZoom = 10;
    float rotationspeed = 10f;


    private Vector3 mRightDirection = Vector3.right; // Direction the camera should move when on the right edge
    private Vector3 mUpDirection = Vector3.forward;
    //private Vector3 zoomValue = new Vector3(0f, 2.0f, 0);
    private Vector3 mUpDir = Vector3.up;

    void Update()
    {
        // Check if on the right edge
        if (Input.mousePosition.x >= Screen.width - distpixels)
        {
            // Move the camera right
            transform.position += mRightDirection * Time.deltaTime * mSpeed;
        }
        else if(Input.mousePosition.x <= 0 - distpixels)
        {
            //muove a sx
            transform.position -= mRightDirection * Time.deltaTime * mSpeed;
        }

        //muove su e giu
        if (Input.mousePosition.y >= Screen.height - distpixels)
        {
            // muove avanti
            transform.position += mUpDirection * Time.deltaTime * mSpeed;
        }
        else if (Input.mousePosition.y <= 0 - distpixels)
        {
            //muove indietro
            transform.position -= mUpDirection * Time.deltaTime * mSpeed;
        }



        //zoom in avanti/indietro ----> DA SISTEMARE
        if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
        {
            //minimap.orthographicSize += Input.GetAxis("Mouse ScrollWheel");
            transform.position += (Vector3.up * 5.0f) * Time.deltaTime * mSpeedZoom;

        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f) // forward
        {
            //minimap.orthographicSize += Input.GetAxis("Mouse ScrollWheel");
            transform.position -= (Vector3.up * 5.0f) * Time.deltaTime * mSpeedZoom;
        }



        //FARE LA ROTAZIONE DELLA CAMERA


        /*else if(Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            float var =100f;
            Vector3 ciao = new Vector3(0, var, 0);
            transform.position -= ciao * Time.deltaTime * mSpeedZoom;
        }*/

    }
}
