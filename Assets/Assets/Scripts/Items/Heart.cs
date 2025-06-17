using UnityEngine;

public class Heart : MonoBehaviour {
    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            RocketController rocket = other.GetComponent<RocketController>();
            if (rocket != null) {
                rocket.GiveLife();
            }

            Destroy(gameObject);
        }
    }
}


