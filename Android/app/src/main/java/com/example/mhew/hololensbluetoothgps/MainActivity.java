package com.example.mhew.hololensbluetoothgps;

import android.Manifest;
import android.bluetooth.*;
import android.bluetooth.le.*;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.location.*;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.support.v4.app.ActivityCompat;
import android.support.v4.content.ContextCompat;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.widget.TextView;
import android.widget.Toast;

import com.google.android.gms.maps.MapView;

import java.nio.ByteBuffer;
import java.text.SimpleDateFormat;
import java.util.Date;

import org.osgeo.proj4j.CRSFactory;
import org.osgeo.proj4j.CoordinateReferenceSystem;
import org.osgeo.proj4j.CoordinateTransform;
import org.osgeo.proj4j.CoordinateTransformFactory;
import org.osgeo.proj4j.ProjCoordinate;
import org.osgeo.proj4j.datum.*;

public class MainActivity extends AppCompatActivity {
    //TextView textViewTime;
    TextView textViewLatitude;
    TextView textViewLongitude;
    TextView textViewNorth;
    TextView textViewEast;
    //TextView textViewBearing;
    //MapView mapView;

    BluetoothAdapter mBluetoothAdapter;
    BluetoothLeAdvertiser mBLEAdvertiser;
    static final int BEACON_ID = 1775;
    public static final int REQUEST_ENABLE_BT = 1;

    LocationManager mLocationManager;
    static final int PERMISSION_RESULT_CODE = 1;
    Location currentLocation;
    ProjCoordinate currentUTMCoordinates;
    ProjCoordinate utmCoordinates;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        // Setup UI References
        //textViewTime = (TextView) findViewById(R.id.textViewTime);
        textViewLatitude = (TextView) findViewById(R.id.textViewLatitude);
        textViewLongitude = (TextView) findViewById(R.id.textViewLongitude);
        textViewNorth = (TextView) findViewById(R.id.textViewNorth);
        textViewEast = (TextView) findViewById(R.id.textViewEast);
        //textViewBearing = (TextView) findViewById(R.id.textViewBearing);
        //mapView = (MapView) findViewById(R.id.mapView);

        if (savedInstanceState == null) {
            // Use this check to determine whether BLE is supported on the device. Then you can
            // selectively disable BLE-related features.Only needed if required=false in manifest e.g.:
            // <uses-feature android:name="android.hardware.bluetooth_le" android:required="false"/>
            if (!getPackageManager().hasSystemFeature(PackageManager.FEATURE_BLUETOOTH_LE)) {
                Toast.makeText(this, R.string.ble_not_supported, Toast.LENGTH_SHORT).show();
                finish();
            }

            // Is Bluetooth supported on this device?
            mBluetoothAdapter = ((BluetoothManager) getSystemService(Context.BLUETOOTH_SERVICE))
                    .getAdapter();
            if (mBluetoothAdapter != null) {
                // Is Bluetooth turned on?
                if (mBluetoothAdapter.isEnabled()) {
                    // Are Bluetooth Advertisements supported on this device?
                    if (mBluetoothAdapter.isMultipleAdvertisementSupported()) {
                        // Everything is supported and enabled, so proceed.
                        mBLEAdvertiser = mBluetoothAdapter.getBluetoothLeAdvertiser();
                    } else {
                        // Bluetooth Advertisements are not supported.
                        Toast.makeText(this, R.string.bt_ads_not_supported, Toast.LENGTH_SHORT).show();
                        finish();
                    }
                } else {
                    // Prompt user to turn on Bluetooth (logic continues in onActivityResult()).
                    Intent enableBtIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
                    startActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);
                }
            } else {
                Toast.makeText(this, R.string.ble_not_supported, Toast.LENGTH_SHORT).show();
                finish();
            }
        }
    }

    @Override
    protected void onResume() {
        super.onResume();

        // only process GPS when activity is running.
        int permissionCheck = ContextCompat.checkSelfPermission((Context) this, Manifest.permission.ACCESS_FINE_LOCATION);
        if (permissionCheck == PackageManager.PERMISSION_GRANTED) {
            startGPS();
        } else {
            // No explanation needed, we can request the permission.
            ActivityCompat.requestPermissions(this,
                    new String[]{Manifest.permission.ACCESS_FINE_LOCATION},
                    PERMISSION_RESULT_CODE);
        }
    }

    @Override
    protected void onPause() {
        super.onPause();
        stopGPS();
        stopAdvertising();
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        switch (requestCode) {
            case REQUEST_ENABLE_BT:
                if (resultCode == RESULT_OK) {
                    // Bluetooth is now Enabled, are Bluetooth Advertisements supported on
                    // this device?
                    if (mBluetoothAdapter.isMultipleAdvertisementSupported()) {
                        // Everything is supported and enabled, so proceed.
                        mBLEAdvertiser = mBluetoothAdapter.getBluetoothLeAdvertiser();
                    } else {
                        // Bluetooth Advertisements are not supported.
                        Toast.makeText(this, R.string.bt_ads_not_supported_leaving, Toast.LENGTH_SHORT).show();
                        finish();
                    }
                } else {
                    // User declined to enable Bluetooth, exit the app.
                    Toast.makeText(this, R.string.bt_not_enabled_leaving,
                            Toast.LENGTH_SHORT).show();
                    finish();
                }

            default:
                super.onActivityResult(requestCode, resultCode, data);
        }
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] grantResults) {
        switch (requestCode) {
            case PERMISSION_RESULT_CODE:
                if (grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                    // Permission Granted
                    startGPS();
                } else {
                    // Permission Denied
                    Toast.makeText(MainActivity.this, "ACCESS_FINE_LOCATION Denied. Quitting!", Toast.LENGTH_SHORT)
                            .show();
                    finish();
                }
                break;
            default:
                super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

    private void startAdvertising(int manufactureId, byte[] manufactureData) {
        if (mBLEAdvertiser == null) return;
        AdvertiseSettings settings = new AdvertiseSettings.Builder()
                .setAdvertiseMode(AdvertiseSettings.ADVERTISE_MODE_BALANCED)
                .setConnectable(false)
                .setTimeout(0)
                .setTxPowerLevel(AdvertiseSettings.ADVERTISE_TX_POWER_MEDIUM)
                .build();
        AdvertiseData data = new AdvertiseData.Builder()
                .addManufacturerData(manufactureId, manufactureData)
                .build();
        mBLEAdvertiser.startAdvertising(settings, data, mAdvertiseCallback);
    }

    private void stopAdvertising() {
        if (mBLEAdvertiser == null) return;
        mBLEAdvertiser.stopAdvertising(mAdvertiseCallback);
    }

    private void restartAdvertising(int manufactureId, byte[] manufactureData) {
        stopAdvertising();
        startAdvertising(manufactureId, manufactureData);
    }

    private AdvertiseCallback mAdvertiseCallback = new AdvertiseCallback() {
        @Override
        public void onStartSuccess(AdvertiseSettings settingsInEffect) {
            super.onStartSuccess(settingsInEffect);
            String msg = "Service Running";
            mHandler.sendMessage(Message.obtain(null, 0, msg));
        }

        @Override
        public void onStartFailure(int errorCode) {
            if (errorCode != ADVERTISE_FAILED_ALREADY_STARTED) {
                String msg = "Service failed to start: " + errorCode;
                mHandler.sendMessage(Message.obtain(null, 0, msg));
            } else {
                //restartAdvertising();
            }
        }
    };
    private Handler mHandler = new Handler() {
        @Override
        public void handleMessage(Message msg) {
            super.handleMessage(msg);
            //Toast.makeText(MainActivity.this, new SimpleDateFormat("HH.mm.ss").format(new Date()) +
            //        ": " + (CharSequence) msg.toString(), Toast.LENGTH_SHORT).show();
        }
    };

    //region GPS

    LocationListener listener = new LocationListener() {
        @Override
        public void onLocationChanged(Location location) {
            UpdatePosition(location);
        }

        @Override
        public void onStatusChanged(String provider, int status, Bundle extras) {
        }

        @Override
        public void onProviderEnabled(String provider) {
        }

        @Override
        public void onProviderDisabled(String provider) {
        }
    };

    public void startGPS() {
        //Execute location service call if user has explicitly granted ACCESS_FINE_LOCATION..
        mLocationManager = (LocationManager) getSystemService(Context.LOCATION_SERVICE);
        try {
            mLocationManager.requestLocationUpdates(LocationManager.GPS_PROVIDER, 0, 0, listener);
            mLocationManager.requestLocationUpdates(LocationManager.NETWORK_PROVIDER, 0, 0, listener);
            currentLocation = mLocationManager.getLastKnownLocation(LocationManager.GPS_PROVIDER);
            UpdatePosition(currentLocation);
            Log.i("GPS_Receiver", "startGPS: GPS Started..");
        } catch (SecurityException e) {
        }
    }

    public void stopGPS() {
        if (mLocationManager == null) return;
        mLocationManager.removeUpdates(listener);
    }

    public void UpdatePosition(Location location) {
        currentLocation = location;
        currentUTMCoordinates = utmCoordinates;
        convertToUTM(currentLocation);
        if((currentLocation != null) && (currentUTMCoordinates != null)) {
//            textViewTime.setText(new SimpleDateFormat("HH.mm.ss").format(new Date()));
            textViewLatitude.setText(Double.toString(currentLocation.getLatitude()));
            textViewLongitude.setText(Double.toString(currentLocation.getLongitude()));
            textViewNorth.setText(Double.toString(currentUTMCoordinates.y));
            textViewEast.setText(Double.toString(currentUTMCoordinates.x));
//        textViewBearing.setText(Float.toString(currentLocation.getBearing()));
//        double[] utm = convertToUTM(currentLocation);
//            textViewBearing.setText(Double.toString(utmCoordinates[0]) + " " + Double.toString(utmCoordinates[1]));
            restartAdvertising(BEACON_ID, buildGPSPacket());
        }
    }

    private byte[] buildGPSPacket() {
        Location location = mLocationManager.getLastKnownLocation(LocationManager.GPS_PROVIDER);
        byte[] packet = new byte[24];

        if((location != null) && (utmCoordinates != null)) {
            double latitude = utmCoordinates.x;
            double longitude = utmCoordinates.y;

            try {
//                double latitude = location.getLatitude();
//                HUSK AA ENDRE VARIABELNAVN
//                double latitude = utmCoordinates[0];
                byte[] buffer = ByteBuffer.allocate(8).putDouble(latitude).array();
                for (int i = 0, j =7; i < 8; i++, j--) packet[i] = buffer[j];
//                double longitude = location.getLongitude();
//                HUSK AA ENDRE VARIABELNAVN
//                double longitude = utmCoordinates[1];
                buffer = ByteBuffer.allocate(8).putDouble(longitude).array();
                for (int i = 8, j =7; i < 16; i++, j--) packet[i] = buffer[j];
                float bearing = 0;
                bearing = location.getBearing();
                Log.i("BEARING: ", Float.toString(bearing));
                buffer = ByteBuffer.allocate(4).putFloat(bearing).array();
                for (int i = 16, j =3; i < 20; i++, j--) packet[i] = buffer[j];
                float speed = 0;
                speed = location.getSpeed();
                buffer = ByteBuffer.allocate(4).putFloat(speed).array();
                for (int i = 20, j =3; i < 24; i++, j--) packet[i] = buffer[j];
            } catch (NumberFormatException e) {
                packet = new byte[24];
            }
        }
        return packet;
    }
    //endregion GPS

//    public static void convertToUTM(Location location) throws FactoryException, TransformException {
//        double lat = location.getLatitude();
//        double lon = location.getLongitude();
//
//        GeometryFactory gf = new GeometryFactory();
//        Coordinate c = new Coordinate(lat, lon);
//
//        Point p = gf.createPoint(c);
//        CoordinateReferenceSystem targetCRS = CRS.decode("EPSG:5972"); //UTM zone 32
//        CoordinateReferenceSystem sourceCRS = CRS.decode("EPSG:4326"); //WGS84
//
//        MathTransform mathTransform = CRS.findMathTransform(sourceCRS, targetCRS, false);
//        Point p1 = (Point) JTS.transform(p, mathTransform);
//
//        Log.d("UTM-coordinate", p1.getCoordinate().toString());
//
//    }

    public void convertToUTM(Location location){
        String wgsName = "EPSG:4326";
        String utmName = "EPSG:32632";

        CoordinateTransformFactory ctFactory = new CoordinateTransformFactory();
        CRSFactory csFactory = new CRSFactory();


        CoordinateReferenceSystem wgs = csFactory.createFromName(wgsName);
        CoordinateReferenceSystem utm = csFactory.createFromName(utmName);

        CoordinateTransform trans = ctFactory.createTransform(wgs, utm);

        ProjCoordinate pWgs = new ProjCoordinate();
        ProjCoordinate pUtm = new ProjCoordinate();

        pWgs.y = location.getLatitude();
        pWgs.x = location.getLongitude();

        trans.transform(pWgs, pUtm);


        Log.d("transform", pUtm.toString());
        Log.d("transformWgs", pWgs.toString());

        utmCoordinates = pUtm;


//        CoordinateConversion c = new CoordinateConversion();
//        double lat = location.getLatitude();
//        double lon = location.getLongitude();
//        String utmCoords = c.latLon2UTM(lat, lon);
//        Log.d("UTM???:", utmCoords);
//        String[] split = utmCoords.split("\\s+");
//
//        double[] finalUTMCoords = {Float.parseFloat(split[2]),Float.parseFloat(split[3])};
//
//
//        Log.d("nr0:", split[0]);
//        Log.d("nr1:", split[1]);
//        Log.d("East:", split[2]);
//        Log.d("North:", split[3]);
//        Log.d("finalUTMCoords: ", Double.toString(finalUTMCoords[0]));


//        utmCoordinates = finalUTMCoords;
    }

}
