using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EventProcessor : MonoBehaviour
{
    //public Text TextTime;
    //NORTH AND EAST, NOT LAT AND LON
    public Text TextLatitude;
    public Text TextLongitude;
    //public Text TextHeading;
    //public Image Renderer;

    private System.Object _queueLock = new System.Object();
    List<byte[]> _queuedData = new List<byte[]>();
    List<byte[]> _processingData = new List<byte[]>();

    public void QueueData(byte[] data)
    {
        //TextLongitude.text = "QueueData";
        lock (_queueLock)
        {
            _queuedData.Add(data);
        }
    }

    public void DoThisFunction()
    {
        TextLongitude.text = "Actually accessed EventProcessor";
    }



    void Update()
    {
        MoveQueuedEventsToExecuting();
        while (_processingData.Count > 0)
        {
            var byteData = _processingData[0];
            _processingData.RemoveAt(0);
            try
            {
                var gpsData = GPS_DataPacket.ParseDataPacket(byteData);
                //TextTime.text = DateTime.Now.ToString("t");
                TextLatitude.text = gpsData.Latitude.ToString();
                TextLongitude.text = gpsData.Longitude.ToString();
                //TextHeading.text = gpsData.Heading.ToString();
            }
            catch (Exception e)
            {
                TextLatitude.text = "Error: " + e.Message;
            }
        }
    }


    private void MoveQueuedEventsToExecuting()
    {
        lock (_queueLock)
        {
            while (_queuedData.Count > 0)
            {
                byte[] data = _queuedData[0];
                _processingData.Add(data);
                _queuedData.RemoveAt(0);
            }
        }
    }
}