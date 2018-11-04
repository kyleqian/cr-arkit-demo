using UnityEngine;
using UnityEngine.UI;

public class ToggleColorControl : MonoBehaviour
{
    Toggle toggle;
    ColorBlock onColorBlock;
    ColorBlock offColorBlock;

    void Start()
    {
        toggle = GetComponent<Toggle>();
        onColorBlock = toggle.colors;
        offColorBlock = toggle.colors;
        offColorBlock.normalColor /= 2;
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
    }

    void OnToggleValueChanged(bool isOn)
    {
        toggle.colors = isOn ? onColorBlock : offColorBlock;
    }
}
