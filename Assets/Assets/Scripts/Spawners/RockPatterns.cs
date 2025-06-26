using UnityEngine;
using System.Collections;

public interface IRockPattern {
    IEnumerator Execute(RockSpawner spawner, float duration, float baseRate);
}

public class RandomPattern : IRockPattern {
    public IEnumerator Execute(RockSpawner spawner, float dur, float rate) {
        float t = 0f;
        while (t < dur) {
            spawner.SpawnOneRock();
            float wait = 1f / Mathf.Max(rate, 0.01f);
            t += wait;
            yield return new WaitForSeconds(wait);
        }
    }
}

public class LineRainPattern : IRockPattern {
    public IEnumerator Execute(RockSpawner spawner, float dur, float rate) {
        float t = 0f;
        int lanes = 6;
        while (t < dur) {
            for (int i = 0; i < lanes; i++) {
                float lerp = (i + .5f) / lanes;
                var z = spawner.zones[Random.Range(0, spawner.zones.Length)];
                Vector3 pos = Vector3.Lerp(z.pointA.position, z.pointB.position, lerp);
                spawner.SpawnRockAt(pos, z.launchDir, Random.Range(
                     spawner.minLaunchSpeed, spawner.maxLaunchSpeed));
            }
            float wait = 1f / Mathf.Max(rate, 0.01f);
            t += wait;
            yield return new WaitForSeconds(wait);
        }
    }
}

public class DiagonalSweepPattern : IRockPattern {
    public IEnumerator Execute(RockSpawner spawner, float dur, float rate) {
        float t = 0f;
        bool leftToRight = true;
        while (t < dur) {
            var z = spawner.zones[Random.Range(0, spawner.zones.Length)];
            float lerp = leftToRight ? 0f : 1f;
            Vector3 pos = Vector3.Lerp(z.pointA.position, z.pointB.position, lerp);
            spawner.SpawnRockAt(pos, z.launchDir, Random.Range(
                     spawner.minLaunchSpeed, spawner.maxLaunchSpeed));

            leftToRight = !leftToRight;
            float wait = 1f / Mathf.Max(rate, 0.01f);
            t += wait;
            yield return new WaitForSeconds(wait);
        }
    }
}

