using UnityEngine;
using System.Collections;

public class EngineSound : MonoBehaviour {
    public AK.Wwise.Event motorLoopEvent;
    public AK.Wwise.Event motorStopEvent;

    void OnEnable() {
        StartCoroutine(DelayedPost());
    }

    IEnumerator DelayedPost() {
        yield return null;
        motorLoopEvent.Post(gameObject);
    }

    void OnDisable() {
        motorStopEvent.Post(gameObject);
    }
}

