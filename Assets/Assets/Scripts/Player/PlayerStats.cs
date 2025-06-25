using UnityEngine;

public class PlayerStats : MonoBehaviour {
    [SerializeField] private int baseBulletDamage = 1;

    public int GetPlayerBulletDamage() {
        return baseBulletDamage;
    }

    public void UpgradeBulletDamage(int amount) {
        baseBulletDamage += amount;
    }
}

