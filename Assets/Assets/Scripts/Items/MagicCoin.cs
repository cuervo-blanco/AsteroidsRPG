using UnityEngine;

public class MagicCoin : MonoBehaviour {
    public AK.Wwise.Event coinGrab;

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            coinGrab.Post(gameObject);
            Rocket rocket = other.GetComponent<Rocket>();
            if (rocket != null) {
                rocket.ActivateSuperMode();
            }

            Destroy(gameObject);
        }
    }
}

