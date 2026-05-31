using UnityEngine;

public class DamageNumberSpawner : MonoBehaviour
{
    public static DamageNumberSpawner Instance;

    [SerializeField] private GameObject prefab;

    private void Awake()
    {
        Instance = this;
    }

    public void Spawn(int damage, Vector3 position)
    {
        if (prefab == null) return;
        GameObject go = Instantiate(prefab, position, Quaternion.identity);
        go.GetComponent<DamageNumber>().Setup(damage, position);
    }

    // Spawns a damage number with a forced colour instead of the default damage-based colour
    public void SpawnWithColor(int damage, Vector3 position, Color color)
    {
        if (prefab == null) return;

        GameObject go = Instantiate(prefab, position + Vector3.up * 0.3f, Quaternion.identity);
        DamageNumber dn = go.GetComponent<DamageNumber>();
        if (dn != null) dn.Setup(damage, color);
    }
}