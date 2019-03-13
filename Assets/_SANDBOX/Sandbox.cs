using UnityEngine;
using UnityEngine.UI;

public class Sandbox : MonoBehaviour
{
    [SerializeField] GameObject debugCanvas;
    Text debugText;
    int meshTouches;

    void Start()
    {
        debugText = debugCanvas.GetComponentInChildren<Text>();
    }

    void Update()
    {
        DetectLetterTouch();
    }

    void DetectLetterTouch()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
        Ray raycast = Camera.main.ScreenPointToRay(Input.mousePosition);
#else
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray raycast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
#endif
            RaycastHit raycastHit;
            if (Physics.Raycast(raycast, out raycastHit))
            {
                if (raycastHit.collider.tag.StartsWith("Letter"))
                {
                    ++meshTouches;
                    debugText.text = "TOUCHES: " + meshTouches.ToString();
                }
            }
        }
    }
}
