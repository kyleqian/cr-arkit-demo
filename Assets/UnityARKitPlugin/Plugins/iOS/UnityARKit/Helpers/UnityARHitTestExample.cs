using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace UnityEngine.XR.iOS
{
    public class UnityARHitTestExample : MonoBehaviour
    {
        public static UnityARHitTestExample Instance;

        struct SimpleModelTransform
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 localScale;
        }

        enum RotationState { None, CW, CCW }
        enum ScalingState { None, Up, Down }

        public GameObject HitTestModelPrefab;
        public float maxRayDistance;
        public float rotationSpeed;
        public float scalingSpeed;
        public LayerMask collisionLayer; // ARKitPlane layer

        const string PlayerPrefAnchorIdKey = "AnchorId";
        const string PlayerPrefScaleXKey = "ScaleX";
        const string PlayerPrefScaleYKey = "ScaleY";
        const string PlayerPrefScaleZKey = "ScaleZ";
        SimpleModelTransform originalModelTransform;
        RotationState rotationState = RotationState.None;
        ScalingState scalingState = ScalingState.None;

        GameObject hitTestModelInst;

        public void RecordModelScale()
        {
            if (!hitTestModelInst.activeSelf)
            {
                return;
            }
            PlayerPrefs.SetFloat(PlayerPrefScaleXKey, hitTestModelInst.transform.localScale.x);
            PlayerPrefs.SetFloat(PlayerPrefScaleYKey, hitTestModelInst.transform.localScale.y);
            PlayerPrefs.SetFloat(PlayerPrefScaleZKey, hitTestModelInst.transform.localScale.z);
        }

        public void ResetModel()
        {
            hitTestModelInst.SetActive(false);
            //m_HitTransform.position = originalModelTransform.position;
            //m_HitTransform.rotation = originalModelTransform.rotation;
            //m_HitTransform.localScale = originalModelTransform.localScale;
        }

        public void StartCWRotation()
        {
            rotationState = RotationState.CW;
        }

        public void StartCCWRotation()
        {
            rotationState = RotationState.CCW;
        }

        public void ResetRotationState()
        {
            rotationState = RotationState.None;
        }

        public void StartScalingUp()
        {
            scalingState = ScalingState.Up;
        }

        public void StartScalingDown()
        {
            scalingState = ScalingState.Down;
        }

        public void ResetScalingState()
        {
            scalingState = ScalingState.None;
        }

        void RotateModel(bool clockwise)
        {
            hitTestModelInst.transform.Rotate((clockwise ? 1 : -1) * rotationSpeed * Vector3.up * Time.deltaTime);
        }

        void ScaleModel(bool scaleUp)
        {
            Vector3 newScale = hitTestModelInst.transform.localScale + ((scaleUp ? 1 : -1) * scalingSpeed * Vector3.one * Time.deltaTime);
            if (newScale.x < 0 || newScale.y < 0 || newScale.z < 0)
            {
                return;
            }
            hitTestModelInst.transform.localScale = newScale;
        }

        bool IsPointerOverUIObject()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }

        bool HitTestWithResultType(ARPoint point, ARHitTestResultType resultTypes)
        {
            List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface().HitTest(point, resultTypes);
            if (hitResults.Count > 0)
            {
                foreach (var hitResult in hitResults)
                {
                    if (PlayerPrefs.HasKey(PlayerPrefAnchorIdKey))
                    {
                        UnityARSessionNativeInterface.GetARSessionNativeInterface().RemoveUserAnchor(PlayerPrefs.GetString(PlayerPrefAnchorIdKey));
                        PlayerPrefs.DeleteKey(PlayerPrefAnchorIdKey);
                    }
                    hitTestModelInst.transform.position = UnityARMatrixOps.GetPosition(hitResult.worldTransform);
                    hitTestModelInst.transform.rotation = UnityARMatrixOps.GetRotation(hitResult.worldTransform);
                    hitTestModelInst.SetActive(true);
                    PlayerPrefs.SetString(PlayerPrefAnchorIdKey, UnityARSessionNativeInterface.GetARSessionNativeInterface().AddUserAnchorFromGameObject(hitTestModelInst).identifierStr);
                    return true;
                }
            }
            return false;
        }

        void UnityARSessionNativeInterface_ARUserAnchorAddedEvent(ARUserAnchor anchorData)
        {
            hitTestModelInst.transform.position = UnityARMatrixOps.GetPosition(anchorData.transform);
            hitTestModelInst.transform.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);
            hitTestModelInst.SetActive(true);

            Debug.Log("Added anchor: " + anchorData.identifier + " " + hitTestModelInst.transform.position.ToString("F2"));

            if (PlayerPrefs.HasKey(PlayerPrefScaleXKey) && PlayerPrefs.HasKey(PlayerPrefScaleYKey) && PlayerPrefs.HasKey(PlayerPrefScaleZKey))
            {
                hitTestModelInst.transform.localScale = new Vector3(PlayerPrefs.GetFloat(PlayerPrefScaleXKey), PlayerPrefs.GetFloat(PlayerPrefScaleYKey), PlayerPrefs.GetFloat(PlayerPrefScaleZKey));
            }
        }

        void UnityARSessionNativeInterface_ARUserAnchorUpdatedEvent(ARUserAnchor anchorData)
        {
            hitTestModelInst.transform.position = UnityARMatrixOps.GetPosition(anchorData.transform);
            hitTestModelInst.transform.rotation = UnityARMatrixOps.GetRotation(anchorData.transform);

            Debug.Log("Updated anchor: " + anchorData.identifier + " " + hitTestModelInst.transform.position.ToString("F2"));
        }

        void UnityARSessionNativeInterface_ARUserAnchorRemovedEvent(ARUserAnchor anchorData)
        {
            hitTestModelInst.SetActive(false);

            Debug.Log("Removed anchor: " + hitTestModelInst.transform.position.ToString("F2"));
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            hitTestModelInst = Instantiate(HitTestModelPrefab);
            hitTestModelInst.SetActive(false);
            originalModelTransform = new SimpleModelTransform
            {
                position = hitTestModelInst.transform.position,
                rotation = hitTestModelInst.transform.rotation,
                localScale = hitTestModelInst.transform.localScale
            };
            UnityARSessionNativeInterface.ARUserAnchorAddedEvent += UnityARSessionNativeInterface_ARUserAnchorAddedEvent;
            UnityARSessionNativeInterface.ARUserAnchorUpdatedEvent += UnityARSessionNativeInterface_ARUserAnchorUpdatedEvent;
            UnityARSessionNativeInterface.ARUserAnchorRemovedEvent += UnityARSessionNativeInterface_ARUserAnchorRemovedEvent;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            Destroy(hitTestModelInst);
            UnityARSessionNativeInterface.ARUserAnchorAddedEvent -= UnityARSessionNativeInterface_ARUserAnchorAddedEvent;
            UnityARSessionNativeInterface.ARUserAnchorUpdatedEvent -= UnityARSessionNativeInterface_ARUserAnchorUpdatedEvent;
            UnityARSessionNativeInterface.ARUserAnchorRemovedEvent -= UnityARSessionNativeInterface_ARUserAnchorRemovedEvent;
        }

        void Update()
        {
            if (Input.touchCount > 0 && !IsPointerOverUIObject())
			{
				var touch = Input.GetTouch(0);
				if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
				{
					var screenPosition = Camera.main.ScreenToViewportPoint(touch.position);
					ARPoint point = new ARPoint {
						x = screenPosition.x,
						y = screenPosition.y
					};

                    // prioritize reults types
                    ARHitTestResultType[] resultTypes = {
						ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingGeometry,
                        //ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent, 
                        // if you want to use infinite planes use this:
                        //ARHitTestResultType.ARHitTestResultTypeExistingPlane,
                        ARHitTestResultType.ARHitTestResultTypeEstimatedHorizontalPlane, 
						//ARHitTestResultType.ARHitTestResultTypeEstimatedVerticalPlane, 
						//ARHitTestResultType.ARHitTestResultTypeFeaturePoint
                    }; 
					
                    foreach (ARHitTestResultType resultType in resultTypes)
                    {
                        if (HitTestWithResultType (point, resultType))
                        {
                            return;
                        }
                    }
				}
			}

            switch (rotationState)
            {
                case RotationState.None:
                    break;
                case RotationState.CW:
                    RotateModel(true);
                    break;
                case RotationState.CCW:
                    RotateModel(false);
                    break;
            }

            switch (scalingState)
            {
                case ScalingState.None:
                    break;
                case ScalingState.Up:
                    ScaleModel(true);
                    break;
                case ScalingState.Down:
                    ScaleModel(false);
                    break;
            }
        }
    }
}
