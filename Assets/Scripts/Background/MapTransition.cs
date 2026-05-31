using UnityEngine;
using UnityEngine.Tilemaps;

// Removes the starter wall tilemap and enables the infinite map
// when the player completes round 1.
public class MapTransition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Tilemap wallTilemap;        // the walls around the starting area
    [SerializeField] private WaveSpawner waveSpawner;

    private void Start()
    {
        if (waveSpawner == null)
            waveSpawner = FindFirstObjectByType<WaveSpawner>();

        if (waveSpawner == null)
        {
            Debug.LogError("MapTransition: WaveSpawner not found.");
            return;
        }
        
        waveSpawner.OnRoundCompleted.AddListener(OnRoundCompleted);
    }

    private void OnRoundCompleted(int round)
    {
        if (round != 1) return;

        // Remove the starter walls
        if (wallTilemap != null)
        {
            wallTilemap.gameObject.SetActive(false);
            Debug.Log("MapTransition: walls removed.");
        }
    }

    private void OnDestroy()
    {
        if (waveSpawner != null)
            waveSpawner.OnRoundCompleted.RemoveListener(OnRoundCompleted);
    }
}