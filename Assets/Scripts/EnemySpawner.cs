using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public DayNightCycle timeSystem;
    public Transform player;
    public Transform spawnPoint;

    public int totalNights = 4;
    public int baseEnemiesPerNight = 3;
    public int enemiesIncrementPerNight = 2;

    public float spawnRadius = 5f;
    public float navMeshSampleRadius = 10f;

    int currentNightIndex;
    bool wasNight;
    List<GameObject> aliveEnemies = new List<GameObject>();

    [System.Obsolete]
    void Start()
    {
        if (timeSystem == null)
            timeSystem = FindObjectOfType<DayNightCycle>();

        // Find the player 
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }

        // Default spawn point
        if (spawnPoint == null)
            spawnPoint = transform;
    }

    void Update()
    {
        if (timeSystem == null || player == null || spawnPoint == null) return;

        bool isNight = timeSystem.IsNight;

        // Detect night starting
        if (isNight && !wasNight)
            OnNightStarted();

        // Detect night ending
        if (!isNight && wasNight)
            OnNightEnded();

        wasNight = isNight;
    }

    void OnNightStarted()
    {
        // Stop spawning if we reached the final night
        if (currentNightIndex >= totalNights)
            return;

        currentNightIndex++;

        // Increase enemy count each night
        int count = baseEnemiesPerNight + enemiesIncrementPerNight * (currentNightIndex - 1);

        // Spawn enemies for this night
        SpawnEnemies(count);
    }

    void OnNightEnded()
    {
        // When morning comes, all enemies to flee and despawn
        foreach (var enemy in aliveEnemies)
        {
            if (enemy == null) continue;

            EnemyAI ai = enemy.GetComponent<EnemyAI>();
            if (ai != null)
                ai.RunAwayAndDespawn();
            else
                Destroy(enemy);
        }

        // Clear the list for the next night
        aliveEnemies.Clear();
    }

    void SpawnEnemies(int count)
    {
        if (enemyPrefab == null || spawnPoint == null) return;

        Vector3 center = spawnPoint.position;

        // Spawn a number of enemies randomly within a radius
        for (int i = 0; i < count; i++)
        {
            Vector2 rand2 = Random.insideUnitCircle * spawnRadius;
            Vector3 candidate = new Vector3(center.x + rand2.x, center.y, center.z + rand2.y);

            // Try to place enemy on the NavMesh if possible
            NavMeshHit hit;
            Vector3 spawnPos = candidate;

            if (NavMesh.SamplePosition(candidate, out hit, navMeshSampleRadius, NavMesh.AllAreas))
                spawnPos = hit.position;

            // Create the enemy and track it
            GameObject obj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            aliveEnemies.Add(obj);
        }
    }

    public void NotifyEnemyDied(GameObject enemy)
    {
        // Remove enemy from tracking list on death
        if (aliveEnemies.Contains(enemy))
            aliveEnemies.Remove(enemy);
    }
}
