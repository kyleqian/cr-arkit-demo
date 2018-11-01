using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace UnityEngine.XR.iOS
{
    
	public class UnityARHitTestExample : MonoBehaviour
    {
        enum RotationState { None, CW, CCW }
        enum ScalingState { None, Up, Down }

        public Transform m_HitTransform;
        public float maxRayDistance;
        public float rotationSpeed;
        public LayerMask collisionLayer; // ARKitPlane layer

        RotationState rotationState = RotationState.None;
        ScalingState scalingState = ScalingState.None;

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
            m_HitTransform.Rotate((clockwise ? 1 : -1) * rotationSpeed * Vector3.up * Time.deltaTime);
        }

        void ScaleModel(bool scaleUp)
        {
            m_HitTransform.localScale = m_HitTransform.localScale + ((scaleUp ? 1 : -1) * Vector3.one * Time.deltaTime);
        }

        bool IsPointerOverUIObject()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }

        bool HitTestWithResultType (ARPoint point, ARHitTestResultType resultTypes)
        {
            List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface ().HitTest (point, resultTypes);
            if (hitResults.Count > 0) {
                foreach (var hitResult in hitResults) {
                    Debug.Log ("Got hit!");
                    m_HitTransform.position = UnityARMatrixOps.GetPosition (hitResult.worldTransform);
                    //m_HitTransform.rotation = UnityARMatrixOps.GetRotation (hitResult.worldTransform);
                    m_HitTransform.gameObject.SetActive(true);
                    Debug.Log (string.Format ("x:{0:0.######} y:{1:0.######} z:{2:0.######}", m_HitTransform.position.x, m_HitTransform.position.y, m_HitTransform.position.z));
                    return true;
                }
            }
            return false;
        }

        void Update ()
        {
			#if UNITY_EDITOR //we will only use this script on the editor side, though there is nothing that would prevent it from working on device
			if (Input.GetMouseButtonDown (0)) {
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				
				//we'll try to hit one of the plane collider gameobjects that were generated by the plugin
				//effectively similar to calling HitTest with ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent
				if (Physics.Raycast (ray, out hit, maxRayDistance, collisionLayer)) {
					//we're going to get the position from the contact point
					m_HitTransform.position = hit.point;
					Debug.Log (string.Format ("x:{0:0.######} y:{1:0.######} z:{2:0.######}", m_HitTransform.position.x, m_HitTransform.position.y, m_HitTransform.position.z));

					//and the rotation from the transform of the plane collider
					m_HitTransform.rotation = hit.transform.rotation;
				}
			}
			#else
            if (Input.touchCount > 0 && !IsPointerOverUIObject() && m_HitTransform != null)
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
			#endif

            switch (rotationState) {
                case RotationState.None:
                    break;
                case RotationState.CW:
                    RotateModel(true);
                    break;
                case RotationState.CCW:
                    RotateModel(false);
                    break;
            }

            switch (scalingState) {
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
