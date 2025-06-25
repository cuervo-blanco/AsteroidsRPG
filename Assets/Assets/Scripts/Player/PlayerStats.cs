public class PlayerStats {
    public float MovementLevel { get; private set; }

    public float GetAcceleration() => 10f + MovementLevel * 1.5f;
    public float GetDrag() => 2f + MovementLevel * 0.2f;

    public void UpgradeMovement() => MovementLevel += 1f;
}
