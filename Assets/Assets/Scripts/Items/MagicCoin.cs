using UnityEngine;

public class MagicCoin : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            Rocket rocket = other.GetComponent<Rocket>();
            if (rocket != null) {
                rocket.ActivateSuperMode();
            }

            Destroy(gameObject);
        }
    }
}

