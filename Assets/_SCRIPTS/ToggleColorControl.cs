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
        offColorBlock = toggle.colors;
        onColorBlock = toggle.colors;
        onColorBlock.normalColor /= 2;

        toggle.colors = toggle.isOn ? onColorBlock : offColorBlock;
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
