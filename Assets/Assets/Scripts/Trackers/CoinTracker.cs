using UnityEngine;
using System;

public class CoinTracker : MonoBehaviour {
    private Action onDestroyed;

    public void Init(Action callback) {
        onDestroyed = callback;
    }

    void OnDestroy() {
        onDestroyed?.Invoke();
    }
}

