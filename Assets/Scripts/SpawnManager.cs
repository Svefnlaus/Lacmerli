using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header ("Armament Spawner Settings")]
    [Range(1, 100)] [SerializeField] protected int spawnCount;
    [SerializeField] private GameObject[] spawnObject;

    private GameObject[] spawnedObjects;

    protected virtual void Awake()
    {
        spawnedObjects = new GameObject[spawnCount];
        for (int current = 0; current < spawnedObjects.Length; current++)
        {
            // spawn every assets on awake
            spawnedObjects[current] = Instantiate(spawnObject[Random.Range(0, spawnObject.Length)], this.transform);

            // despawn them if they're not gonna be used
            spawnedObjects[current].gameObject.SetActive(false);
        }
    }

    public GameObject GetClone()
    {
        for (int current = 0; current < spawnedObjects.Length; current++)
        {
            // get the first inactive object in the pool
            if (!spawnedObjects[current].activeSelf && spawnedObjects[current] != null) return spawnedObjects[current];
        }
        return null;
    }
}
