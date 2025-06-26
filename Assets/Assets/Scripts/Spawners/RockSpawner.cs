using System.Collections;
using UnityEngine;

public class RockSpawner : MonoBehaviour {
    [Header("Rock prefab")]
    public GameObject rockPrefab;

    [System.Serializable]
    public class SpawnZone {
        [Tooltip("Two corner points that define the line segment where rocks appear")]
        public Transform pointA;
        public Transform pointB;

        [Tooltip("Main direction rocks should move (normalized)")]
        public Vector2 launchDir = Vector2.down;
    }

    [Header("Spawn Zones (add as many as you need)")]
    public SpawnZone[] zones;
    public float spawnRadiusBuffer = 2f;

    [Header("Wave shape ADSR")]
    [Tooltip("Rocks per second at t = 0")]
    public float startRate = 0.5f;
    public float peakRate = 10f;
    public float endRate = 5f;
    public float restDuration = 10f;
    public float attackDuration = 20f;
    public float sustainDuration = 20f;
    public float releaseDuration = 20f;

    [Header("Difficulty ramp (wave-to-wave)")]
    [Tooltip("Percent increase per wave, e.g. 0.07 = +7 %")]
    public float difficultyStep = 0.07f;
    public float peakRateCap = 25f;
    public float endRateCap = 15f;

    [Header("Burst / Meteor Shower")]
    [Tooltip("Chance (0-1) that a burst will occur each wave")]
    [Range(0,1)] public float burstChance = 0.3f;
    public float burstMultiplier = 3f;
    public Vector2 burstDurationRange = new Vector2(4f, 8f);

    [Header("Launch Speed")]
    public float minLaunchSpeed = 0.5f;
    public float maxLaunchSpeed = 2.5f;

    Coroutine loop;
    Camera cam;

    void Awake() => cam = Camera.main;

    public void Begin() { if (loop == null) loop = StartCoroutine(SpawnLoop()); }
    public void Stop() { if (loop != null) { StopCoroutine(loop); loop = null; } }

    IEnumerator SpawnLoop() {
        int waveIndex = 0;

        while (true) {
            waveIndex++;

            float thisStart = startRate * Mathf.Pow(1f + difficultyStep, waveIndex - 1);
            float thisPeak  = Mathf.Min( peakRate  * Mathf.Pow(1f + difficultyStep, waveIndex - 1), peakRateCap);
            float thisEnd   = Mathf.Min( endRate   * Mathf.Pow(1f + difficultyStep, waveIndex - 1), endRateCap);

            bool   willBurst = Random.value < burstChance;
            float  burstStartTime = 0f, burstEndTime = 0f;
            if (willBurst) {
                float waveLen = attackDuration + sustainDuration + releaseDuration;
                float burstLen = Random.Range(burstDurationRange.x, burstDurationRange.y);
                burstStartTime = Random.Range(0f, waveLen - burstLen);
                burstEndTime   = burstStartTime + burstLen;
            }

            float waveStartTime = Time.time;
            float waveDuration  = attackDuration + sustainDuration + releaseDuration;

            while (Time.time - waveStartTime < waveDuration) {
                float tWave = Time.time - waveStartTime;

                float rate;
                if (tWave < attackDuration) {
                    rate = Mathf.Lerp(thisStart, thisPeak, tWave / attackDuration);
                } else if (tWave < attackDuration + sustainDuration) {
                    rate = thisPeak;
                } else {
                    rate = Mathf.Lerp(thisPeak, thisEnd,
                             (tWave - attackDuration - sustainDuration) / releaseDuration);
                }

                if (willBurst && tWave >= burstStartTime && tWave <= burstEndTime) {
                    rate *= burstMultiplier;
                }

                float wait = 1f / Mathf.Max(rate, 0.01f);
                SpawnOneRock();
                yield return new WaitForSeconds(wait);
            }

            yield return new WaitForSeconds(restDuration);
        }
    }

    void SpawnOneRock() {
        if (zones == null || zones.Length == 0) return;

        var z   = zones[Random.Range(0, zones.Length)];
        Vector3 pos = Vector3.Lerp(z.pointA.position, z.pointB.position, Random.value);

        var rock = Instantiate(rockPrefab, pos, Quaternion.identity).GetComponent<Rock>();

        Vector2 dir = (z.launchDir + Random.insideUnitCircle * 0.15f).normalized;
        float   spd = Random.Range(minLaunchSpeed, maxLaunchSpeed);
        rock.GetComponent<Rigidbody2D>().linearVelocity = dir * spd;
    }
}

