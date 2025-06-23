using UnityEngine;

public enum MagicCoinType {
    Default,
    Shield,
    FireRate,
    SpeedBoost,
    HeavyShot,
    SlipperyBoost,
    Confusion,
    GoldFrenzy,
    Ghost,
    MiniMode,
    Overheat,
    Random,
}

public class MagicCoin : MonoBehaviour {
    public AK.Wwise.Event coinGrab;
    public MagicCoinType coinType = MagicCoinType.Default;

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            coinGrab.Post(gameObject);

            RocketPowerModule powerModule = other.GetComponent<RocketPowerModule>();
            if (powerModule != null) {
                MagicCoinType actualType = coinType;

                if (coinType == MagicCoinType.Random) {
                    var values = System.Enum.GetValues(typeof(MagicCoinType));
                    MagicCoinType newType;

                    do {
                        newType = (MagicCoinType)values.GetValue(Random.Range(0, values.Length));
                    } while (newType == MagicCoinType.Default || newType == MagicCoinType.Random);

                    actualType = newType;
                }

                powerModule.AddSuperCharge(actualType);
            }

            Destroy(gameObject);
        }
    }
}

