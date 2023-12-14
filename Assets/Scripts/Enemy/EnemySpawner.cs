using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour {
    [SerializeField] private List<GameObject> enemyLibrary = new List<GameObject>();

    [SerializeField]
    private List<GameObject> spawnRegions = new List<GameObject>();

    /// <summary>
    /// the rate at which the spawner will acrew more monster points
    /// </summary>
    [SerializeField] private float spawnInterval;
    /// <summary>
    /// 
    /// the upper bound of the variation in the rate at which the spawner will acrew more monster points.
    /// </summary>
    [SerializeField] private float randomVariation;

    [SerializeField] private int monsterPoints = 5;

    [SerializeField] private int growthRate = 1;

    private int cheapestCost;

    private void Start() {
        ValidateEnemyList();
        ValidateSpawnRegions();
        cheapestCost = FindCheapestCost();

        StartCoroutine(incrementMonsterPoints());
    }

    private void Update() {
        while(monsterPoints > cheapestCost){
            TrySpawnMonster();
        }
    }

    public void TrySpawnMonster(){
        if(enemyLibrary?.Count < 1) {
            Debug.LogWarning("[EnemySpawner > findCheapestEnmey] EnemyLibrary is Empty");
            return;
        }

        List<GameObject> availableEnemies = GetAvailableEnemies();

        GameObject enemy = availableEnemies[Random.Range(0, availableEnemies.Count)];
        BoxCollider area = spawnRegions[Random.Range(0, spawnRegions.Count)].GetComponent<BoxCollider>();

        Spawn(enemy, area.bounds);
        monsterPoints -= enemy.GetComponent<EnemyStats>().getCost();

    }

    private void ValidateEnemyList() {
        List<GameObject> temp = new List<GameObject>();

        foreach (GameObject obj in enemyLibrary) {
            if (obj.GetComponent<EnemyStats>())
                temp.Add(obj);
        }

        enemyLibrary = temp;
    }

    private void ValidateSpawnRegions() {
        List<GameObject> temp = new List<GameObject>();

        foreach (GameObject obj in spawnRegions) {
            if (obj.GetComponent<BoxCollider>())
                temp.Add(obj);
        }

        spawnRegions = temp;
    }

    private int FindCheapestCost() {
        if(enemyLibrary?.Count < 1) {
            Debug.LogError("[EnemySpawner > findCheapestEnmey] EnemyLibrary is empty");
            return -1;
        }

        int cheapestCost = enemyLibrary[0].GetComponent<EnemyStats>().getCost();

        foreach (GameObject enemy in enemyLibrary) {
            if (enemy.GetComponent<EnemyStats>().getCost() < cheapestCost)
                cheapestCost = enemy.GetComponent<EnemyStats>().getCost();
        }

        return cheapestCost;
    }

    private List<GameObject> GetAvailableEnemies() {
        List<GameObject> ret = new List<GameObject>();

        foreach (GameObject enemy in enemyLibrary) {
            if (enemy.GetComponent<EnemyStats>().getCost() <= monsterPoints){
                ret.Add(enemy);
            }
        }
        return ret;
    }

    public void SetGrowthRate(int newRate){
        growthRate = newRate;
    }

    private void Spawn(GameObject enemy, Bounds bounds){
        Debug.Log("[EnemySpawner] Spawn!");
        Vector3 randPoint = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
        GameObject newEnemy = Instantiate(enemy, randPoint, Quaternion.identity);

    }

    private IEnumerator incrementMonsterPoints() {
        float randf = Random.Range(0f, randomVariation);
        yield return new WaitForSeconds(spawnInterval + randf);
        monsterPoints += growthRate;
        StartCoroutine(incrementMonsterPoints());
    }
}
