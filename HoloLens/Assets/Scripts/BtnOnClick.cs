using HoloToolkit.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BtnOnClick : MonoBehaviour {

    public Button startPositionBtn;
    public Button endPositionBtn;
    public Text holoStartTxt;
    public Text holoEndTxt;
    public Text globalStartTxt;
    public Text globalEndTxt;
    public Text azimuthTxt;
    public Text northTxt;
    public Text eastTxt;
    public Text globalRelativeTxt;
    public Text holoRelativeTxt;
    public Text debugTxt;
    public GameObject pyramid;
    public GameObject refObject;
    public GameObject mainObject;
    public Vector2 globalPositonRefObject;

    private Vector3 holoStartPosition;
    private Vector3 holoEndPosition;
    private Vector3 holoNorth;
    private Vector3 holoEast;
    private Vector3 camPos;
    private Vector2 globalNorth;
    private Vector2 globalStartPosition;
    private Vector2 globalEndPosition;

    private bool debug = true;

    // Initialization
    void Start () {
        Button startBtn = startPositionBtn.GetComponent<Button>();
        Button endBtn = endPositionBtn.GetComponent<Button>();

        startBtn.onClick.AddListener(delegate { SetGlobalAndholoPosition(true); });
        endBtn.onClick.AddListener(SetEndPosition);
    }
	
	// Update is called once per frame
	void Update () {
        if(holoNorth != Vector3.zero){
            //make pyramid point at north
            pyramid.transform.rotation = Quaternion.LookRotation(holoNorth);
        }
    }

    void SetGlobalAndholoPosition(bool isStart) {
        camPos = Camera.main.transform.position;
        if (isStart)
        {
            if (debug)
            {
                globalStartPosition = new Vector2( 70, 50 );
                globalPositonRefObject = new Vector2(71, 50);
                holoStartPosition = new Vector3(0, 0, 0);
            }
            else
            {
                globalStartPosition = GetGlobalPosition();
                holoStartPosition = new Vector3(camPos.x, 0, camPos.z);
            }

            Debug.Log("pressed Start");
            holoStartTxt.text = "holoStart: " + holoStartPosition.ToString();
            globalStartTxt.text = "GlobalStart: " + globalStartPosition.ToString();
        }
        else
        {
            if (debug)
            {
                globalEndPosition = new Vector2( 69, 51 );
                holoEndPosition = new Vector3(1, 0, 1);
            }
            else
            {
                globalEndPosition = GetGlobalPosition();
                holoEndPosition = new Vector3(camPos.x, 0, camPos.z);
            }

            Debug.Log("pressed End");
            holoEndTxt.text = "holoEnd: " + holoEndPosition.ToString();
            globalEndTxt.text = "GlobalEnd: " + globalEndPosition.ToString();


        }
    }

    //uses knowledge about end point to define North in respect to holoLens's coordinate system
    //and places the hologram
    void SetEndPosition()
    {
        Vector3 direction;
        Vector3 heading;

        //Sets global and holo end position
        SetGlobalAndholoPosition(false);

        //The distanse walked from holo start position to holo end position
        heading = holoEndPosition - holoStartPosition;

        //normalizing
        direction = heading / heading.magnitude;
        holoRelativeTxt.text = (heading.ToString() + " " + direction.ToString());

        //which direction we moved relative to north
        float angle = DefineNorth();

        azimuthTxt.text = "Azimuth: " + angle.ToString();

        //holo north: the direction we moved rotated by the angle we moved relative to north
        holoNorth = Quaternion.Euler(0, angle, 0)*direction;
        holoEast = Quaternion.Euler(0, angle + 90, 0) * direction;
        Debug.Log("holoNort: " + holoNorth);

        Debug.DrawLine(holoStartPosition, holoEndPosition, Color.blue, 1000000, false);//walked
        Debug.DrawRay(holoStartPosition, holoNorth, Color.white, 1000000, false);
        Debug.DrawRay(holoStartPosition, holoEast, Color.black, 1000000, false);
        Debug.DrawRay(holoStartPosition, transform.right, Color.red, 1000000, false);//x-axis?
        
        Debug.Log("Sat end position: " + heading);

        PlaceHologram();
    }

    private float DefineNorth()
    {
        //vector from start to end point
        Vector2 start2end = new Vector2(globalEndPosition[0] - globalStartPosition[0], globalEndPosition[1] - globalStartPosition[1]);

        //moved globalEndPosition[0] - globalStartPosition[0] along north
        globalNorth = new Vector2(0, globalEndPosition[0] - globalStartPosition[0]);
        

        Debug.Log("global startP to endP: " + start2end);
        globalRelativeTxt.text = (start2end.ToString());
        start2end.Normalize();
        globalRelativeTxt.text = (globalRelativeTxt.text + " " + start2end.ToString());
        globalNorth.Normalize();

        Debug.Log("global north: " + globalNorth);

        //Finds smallest angle
        float angle = Vector2.Angle(start2end, globalNorth);
        Debug.Log("vinkel mellom global start2end og globalNorth: " + angle);

        //-east
        if (start2end.y < 0)
        {
            //-north
            if(start2end.x < 0)
            {
                //Debug.Log("-e, -n " + angle);
                angle = 180 - angle;
                debugTxt.text = debugTxt.text + "  moved SW";
            }
            else if (start2end.x > 0)
            {
                //Debug.Log("-e, +n " + angle);
                angle = 270 + angle;
                debugTxt.text = debugTxt.text + "  moved NW";
            }
            else
            {
                //Debug.Log("rett vest? " + angle);
                debugTxt.text = debugTxt.text + "  moved W";
            }
        }
        //+east
        else
        {
            //-north
            if (start2end.x < 0)
            {
               // Debug.Log("+e, -n " + angle);
                angle = 90 + angle;
                debugTxt.text = debugTxt.text + "  moved SE";
            }
            //+north
            else if ( start2end.x > 0)
            {
                //Debug.Log("+e, +n " + angle);
                //angle = 180 + angle;
                angle = 270 + angle;
                debugTxt.text = debugTxt.text + "  moved NE";
            }
            else
            {
                //Debug.Log("rett øst? " + angle);
                angle = 180 + angle;
                debugTxt.text = debugTxt.text + "  moved E";
            }
        }
        return angle;
    }


    //returns UTM-coordinates for current HoloLens position
    private Vector2 GetGlobalPosition()
    {
        //DO A CHECK WHETHER THIS IS NUMBERS AND OR VALID
        float e = float.Parse(eastTxt.text);
        float n = float.Parse(northTxt.text);
        Debug.Log(eastTxt.text);
        Vector2 globalPosition = new Vector2( n, e );
        return globalPosition;
    }

    private void PlaceHologram()
    {
        Debug.Log("globalPositionRefObject " + globalPositonRefObject);
        Debug.Log("globalEndPosition " + globalEndPosition);
        Debug.Log("globalStartPosition " + globalStartPosition);

        //Find relative UTM coordinates of refObject, where GlobalEndPoint is zero
        Vector3 relativeUTMCoordinatesRefObject = new Vector3(globalPositonRefObject[1]- globalEndPosition[1], 0, globalPositonRefObject[0]-globalEndPosition[0]);
        Debug.Log("relativeUTMCoordinatesRefObject " + relativeUTMCoordinatesRefObject);

        //Vector that points from refObject to mainObject, how mainObject is located in respect to refObject
        Vector3 refObject2mainObject = mainObject.transform.localPosition - refObject.transform.localPosition;
        Debug.Log("realativeUTMCoordinatesMainObject " + refObject2mainObject + relativeUTMCoordinatesRefObject);
        Debug.Log("refObject2mainObject: " + refObject2mainObject);
        
        //Transforms from realitveGlobal to holo
        Vector3 holoPositionRefObject = GetPositionInholoCoordSys(relativeUTMCoordinatesRefObject);
        Vector3 holoPositionMainObject = GetPositionInholoCoordSys((refObject2mainObject+relativeUTMCoordinatesRefObject));
        Debug.Log("refObject2mainObject magnitude: " + refObject2mainObject.magnitude);
        Debug.Log("refObject2mainObject magnitude AFTER: " + (holoPositionMainObject-holoPositionRefObject).magnitude);

        //instansiates objects and rotates mainObject towards north
        GameObject mainObj = Instantiate(mainObject, holoPositionMainObject, Quaternion.FromToRotation(transform.forward, holoNorth));
        Instantiate(refObject, holoPositionRefObject, Camera.main.transform.rotation);
        PlaceOnGround(mainObj);

        //Adds world anchor to object
        WorldAnchorManager.Instance.AttachAnchor(mainObj, "mainObj");
        Debug.Log("Instansiate refObject at: " + holoPositionRefObject);
    }

    // Transformations that preserves length and angle (position and orientation)
    private Vector3 GetPositionInholoCoordSys(Vector3 position)
    {

        Quaternion q = Quaternion.FromToRotation(transform.forward, holoNorth);

        //rotate
        position = Quaternion.Euler(q.eulerAngles) * position;
        Debug.Log("Rotert posisjon: " + position);

        //translate
        position = new Vector3(holoEndPosition[0] + position[0], 0, holoEndPosition[2] + position[2]);
        Debug.Log("Translert posisjon: " + position);

        return position;
    }

    private float PlaceOnGround(GameObject go)
    {
        RaycastHit hit;
        float yOffset;
        //int savedLayer;
        //bool AlignNormals;
        //Vector3 UpVector = new Vector3(0, 90, 0);
        debugTxt.text = debugTxt.text + " PlaceOnGround";

        // get the bounds of the GameObejct
        Bounds bounds = go.GetComponent<Renderer>().bounds;                    
        Debug.Log("PlaceOnGround");
        String rayCast = Physics.Raycast(go.transform.position, -Vector3.up, out hit).ToString();

        debugTxt.text = debugTxt.text + " RayCast: " + rayCast;

        // check if raycast hits a collider
        if (Physics.Raycast(go.transform.position, -Vector3.up, out hit))
        {
            
            yOffset = go.transform.position.y - bounds.min.y;
            go.transform.position = new Vector3 (go.transform.position.x, -(hit.distance - yOffset), go.transform.position.z);
            debugTxt.text = debugTxt.text + " new position: " + go.transform.position.ToString();
            Debug.Log("y-bounds: " + yOffset);
            Debug.Log("hit distance: " + hit.distance);
            Debug.Log("new Position: " + go.transform.position);
            Debug.DrawLine(go.transform.position, hit.point);
        }
        else
        {
            yOffset = 0;
        }

        return yOffset;
    }
}
