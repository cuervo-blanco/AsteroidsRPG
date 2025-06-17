using System.Collections;
using UnityEngine;

public class RockSpawner : MonoBehaviour
{
    [Header("Rock prefab")]
    public GameObject rockPrefab;

    [System.Serializable]
    public class SpawnZone
    {
        [Tooltip("Two corner points that define the line segment where rocks appear")]
        public Transform pointA;
        public Transform pointB;

        [Tooltip("Main direction rocks should move (normalized)")]
        public Vector2 launchDir = Vector2.down;
    }

    [Header("Spawn Zones (add as many as you need)")]
    public SpawnZone[] zones;
    public float spawnRadiusBuffer = 2f;
    Camera cam;

    [Header("Difficulty")]
    [Tooltip("Rocks per second at t = 0")]
    public float startRate  = 0.5f;
    [Tooltip("Rocks per second after difficultyDuration seconds")]
    public float finalRate  = 3f;
    [Tooltip("How long it takes to reach finalRate (seconds)")]
    public float difficultyDuration = 120f;

    [Header("Launch Speed")]
    public float minLaunchSpeed = 0.5f;
    public float maxLaunchSpeed = 2.5f;

    Coroutine loop;

    public void Begin()
    {
        if (loop == null) loop = StartCoroutine(SpawnLoop());
    }

    public void Stop()
    {
        if (loop != null)
        {
            StopCoroutine(loop);
            loop = null;
        }
    }

    void Awake()
    {
        cam = Camera.main;
    }

    IEnumerator SpawnLoop()
    {
        float startTime = Time.time;

        while (true)
        {
            float elapsed = Time.time - startTime;
            float t       = Mathf.Clamp01(elapsed / difficultyDuration);
            float rate    = Mathf.Lerp(startRate, finalRate, t);
            float wait    = 1f / rate;

            SpawnOneRock();
            yield return new WaitForSeconds(wait);
        }
    }

    void SpawnOneRock()
    {
        if (zones == null || zones.Length == 0) return;

        SpawnZone z = zones[Random.Range(0, zones.Length)];

        Vector3 pos = Vector3.Lerp(z.pointA.position, z.pointB.position, Random.value);

        Rock rock = Instantiate(rockPrefab, pos, Quaternion.identity).GetComponent<Rock>();

        Vector2 dir = (z.launchDir + Random.insideUnitCircle * 0.15f).normalized;
        float   spd = Random.Range(minLaunchSpeed, maxLaunchSpeed);
        rock.GetComponent<Rigidbody2D>().linearVelocity = dir * spd;
    }
}

