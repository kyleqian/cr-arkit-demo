using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseScale : MonoBehaviour {
    // Update is called once per frame
    void Update () {
        transform.localScale = new Vector3(0.142155f + Mathf.PingPong (Time.time / 8f, 0.05f), 0.142155f + Mathf.PingPong (Time.time / 8f, 0.05f), 0.142155f + Mathf.PingPong (Time.time / 8f, 0.05f));
    }
}
