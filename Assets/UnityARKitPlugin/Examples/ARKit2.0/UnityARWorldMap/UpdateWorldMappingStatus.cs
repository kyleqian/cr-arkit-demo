using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;

public class UpdateWorldMappingStatus : MonoBehaviour 
{
	public Text MappingStatus;
	public Text TrackingState;

	void Start() 
	{
		UnityARSessionNativeInterface.ARFrameUpdatedEvent += CheckWorldMapStatus;
	}

	void CheckWorldMapStatus(UnityARCamera cam)
	{
        MappingStatus.text = "Mapping: " + cam.worldMappingStatus.ToString().Substring(20);
        TrackingState.text = "Tracking: " + cam.trackingState.ToString().Substring(15) + " / " + cam.trackingReason.ToString().Substring(21);
	}

	void OnDestroy()
	{
		UnityARSessionNativeInterface.ARFrameUpdatedEvent -= CheckWorldMapStatus;
	}
}
