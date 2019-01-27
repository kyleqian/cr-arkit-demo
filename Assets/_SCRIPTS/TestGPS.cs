using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TestGPS : MonoBehaviour
{
    [SerializeField] Text latitudeText;
    [SerializeField] Text longitudeText;

    int timesToUpdate = 20;

    IEnumerator UpdateCoordinates()
    {
        while (timesToUpdate > 0)
        {
            latitudeText.text = Input.location.lastData.latitude.ToString();
            longitudeText.text = Input.location.lastData.longitude.ToString();
            timesToUpdate--;
            yield return new WaitForSeconds(2);
        }
    }

    IEnumerator Start()
    {
        // First, check if user has location service enabled
        //if (!Input.location.isEnabledByUser)
        //{
        //    Debug.Log("LocationService: Location not enabled");
        //    yield break;
        //}

        // Start service before querying location
        Input.location.Start();

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            Debug.Log("LocationService: Timed out");
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("LocationService: Unable to determine device location");
            yield break;
        }

        StartCoroutine(UpdateCoordinates());

        // Access granted and location value could be retrieved
        //print("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);

        // Stop service if there is no need to query location updates continuously
        //Input.location.Stop();
    }
}
