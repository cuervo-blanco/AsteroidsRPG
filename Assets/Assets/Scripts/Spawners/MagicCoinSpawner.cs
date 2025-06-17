using System.Collections;
using UnityEngine;

public class MagicCoinSpawner : MonoBehaviour {
    [Header("Coin Prefab")]
    public GameObject magicCoinPrefab;

    [Header("Spawn Timing")]
    public float minDelay = 5f;
    public float maxDelay = 15f;

    [Header("Spawn Area (World Space)")]
    public Vector2 spawnMin = new Vector2(-8f, -4f);
    public Vector2 spawnMax = new Vector2(8f, 4f);

    [Header("Optional Limit")]
    public int maxCoinsOnScreen = 1;
    private int currentCoins = 0;

    void Start() {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop() {
        while (true) {
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            if (currentCoins < maxCoinsOnScreen) {
                Vector2 spawnPos = new Vector2(
                    Random.Range(spawnMin.x, spawnMax.x),
                    Random.Range(spawnMin.y, spawnMax.y)
                );

                GameObject coin = Instantiate(magicCoinPrefab, spawnPos, Quaternion.identity);
                currentCoins++;

                coin.AddComponent<CoinTracker>().Init(() => currentCoins--);
            }
        }
    }
}

