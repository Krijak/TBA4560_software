using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class getPositionTxt : MonoBehaviour{
    private Vector3 camPos;

    public Text positionTxt;
    //public Text positionTxtX;
    //public Text positionTxtY;
    //public Text positionTxtZ;



    private void Start()
    {
        //position = GetComponent<Rigidbody>();
        SetPositionTxt();

    }

    // called before rendering a frame. Game code
    private void Update()
    {

    }
    // called before any physics is calculated. Physics code
    private void FixedUpdate()
    {
        SetPositionTxt();

       // Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

    }

    void SetPositionTxt()
    {
        camPos = Camera.main.transform.position;
        positionTxt.text = "Pos: " + camPos.ToString();
        //positionTxtX.text = "X: " + camPos.x.ToString();
        //positionTxtY.text = "Y: " + camPos.y.ToString();
        //positionTxtZ.text = "Z: " + camPos.z.ToString();


    }
}