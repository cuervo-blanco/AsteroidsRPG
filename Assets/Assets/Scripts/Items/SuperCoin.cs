using UnityEngine;

public struct SuperCoin {
    public float duration;
    public MagicCoinType type;

    public SuperCoin(MagicCoinType type, float duration) {
        this.type = type;
        this.duration = duration;
    }
}
