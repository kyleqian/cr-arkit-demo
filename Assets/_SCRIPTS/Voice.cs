using UnityEngine;

[CreateAssetMenu(fileName = "New Voice", menuName = "Voice")]
public class Voice : ScriptableObject
{
    public string signature;
    public AudioClip recording;
    public float[] timestamps;
    public string[] transcriptions;

    public string GetPlayerPrefKey()
    {
        return name + "_KEY";
    }
}
