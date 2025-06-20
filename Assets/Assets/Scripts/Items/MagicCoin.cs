using UnityEngine;

public enum  MagicCoinType {
    Default,
    Shield,
    FireRate,
    SpeedBoost,
}

public class MagicCoin : MonoBehaviour {
    public AK.Wwise.Event coinGrab;
    public MagicCoinType coinType = MagicCoinType.Default;

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            coinGrab.Post(gameObject);
            RocketPowerModule powerModule = other.GetComponent<RocketPowerModule>();
            if (powerModule != null) {
                powerModule.AddSuperCharge(coinType);
            }

            Destroy(gameObject);
        }
    }
}

