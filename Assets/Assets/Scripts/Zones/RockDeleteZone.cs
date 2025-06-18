using UnityEngine;

public class RockDeleteZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Rock") || other.CompareTag("Coin")) {
            Destroy(other.gameObject);
        }
    }
}

