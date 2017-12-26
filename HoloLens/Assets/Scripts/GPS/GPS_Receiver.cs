using UnityEngine;
using UnityEngine.UI;
#if NETFX_CORE
using Windows.Devices.Bluetooth.Advertisement;
using System.Runtime.InteropServices.WindowsRuntime;
#endif

public class GPS_Receiver : MonoBehaviour
{
#if NETFX_CORE
    BluetoothLEAdvertisementWatcher watcher;
    public static ushort BEACON_ID = 1775;
#endif
    public static GPS_Receiver instance = null;
    public EventProcessor eventProcessor;
    public Button reactToThisBtn;
    public Text debugTxt;

    void Awake()
    {
        //reactToThisBtn.onClick.AddListener(delegate { TestFunction(); });
#if NETFX_CORE
        Debug.Log("NETFX_CORE");
        reactToThisBtn.onClick.AddListener(delegate { StartWatcher(); });
#endif
    }

    void TestFunction()
    {
        Debug.Log("pressed button");
        eventProcessor.DoThisFunction();
        debugTxt.text = "TRØKKET PÅ KNAPPEN!!";
        byte[] data = { 10, 20 };
        eventProcessor.QueueData(data);

    }

#if NETFX_CORE
    void StartWatcher()
    {
        Debug.Log("pressed button");
        if (watcher != null)
        {
            watcher.Stop();
            debugTxt.text = debugTxt.text + " stopped watcher ";
            Debug.Log("STOPPED");
        }
        watcher = new BluetoothLEAdvertisementWatcher();
        var manufacturerData = new BluetoothLEManufacturerData
        {
            CompanyId = BEACON_ID
        };
        watcher.AdvertisementFilter.Advertisement.ManufacturerData.Add(manufacturerData);
        watcher.Received += Watcher_Received;
        watcher.Start();
        debugTxt.text = debugTxt.text  + " Started Watcher ";

    }
#endif

#if NETFX_CORE
    private async void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
    {
        //debugTxt.text = debugTxt.text  + " WATCHER RECEIVED ";
        //Debug.Log(args);
        //debugTxt.text = args.ToString();
        ushort identifier = args.Advertisement.ManufacturerData[0].CompanyId;
        //debugTxt.text = debugTxt.text  + " " + identifier + " ";
        byte[] data = args.Advertisement.ManufacturerData[0].Data.ToArray();
        // Updates to Unity UI don't seem to work nicely from this callback so just store a reference to the data for later processing.
        eventProcessor.QueueData(data);
    }
#endif
}