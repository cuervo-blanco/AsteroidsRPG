using UnityEngine;

public class EngineSound : MonoBehaviour {
    public AK.Wwise.Event motorLoopEvent;
    public AK.Wwise.Event motorStopEvent;

    void Start() {
        motorLoopEvent.Post(gameObject);
    }

    void OnDisable() {
        motorStopEvent.Post(gameObject);
    }
}

