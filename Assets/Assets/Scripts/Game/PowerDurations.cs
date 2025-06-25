public static class PowerDurations {
    public static float GetDuration(MagicCoinType type) {
        switch (type) {
            case MagicCoinType.Shield:        return 6f;
            case MagicCoinType.FireRate:      return 4f;
            case MagicCoinType.SpeedBoost:    return 5f;
            case MagicCoinType.HeavyShot:     return 8f;
            case MagicCoinType.SlipperyBoost: return 7f;
            case MagicCoinType.Confusion:     return 6f;
            case MagicCoinType.GoldFrenzy:    return 10f;
            case MagicCoinType.Ghost:         return 5f;
            case MagicCoinType.MiniMode:      return 4f;
            case MagicCoinType.Overheat:      return 3f;
            default: return 5f;
        }
    }
}

