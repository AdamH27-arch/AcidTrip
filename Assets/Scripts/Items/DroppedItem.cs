using System.Collections;
using UnityEngine;

// When the player walks over this item the effect is applied immediately.
public class DroppedItem : MonoBehaviour
{
    [SerializeField] private ItemData _data;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField][Range(0f, 1f)] private float pickupSoundVolume = 1f;

    private float _bobOffset;

    public static void Spawn(ItemData data, Vector3 position)
    {
        GameObject go = new GameObject("Drop_" + data.itemName);
        go.transform.position = position;
        go.AddComponent<DroppedItem>().Setup(data);
    }

    private void Setup(ItemData data)
    {
        _data = data;

        if (GetComponent<CircleCollider2D>() == null)
        {
            CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;
        }
    }

    private void Start()
    {
        _bobOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        float bob = Mathf.Sin(Time.time * 2f + _bobOffset) * 0.12f;
        transform.position += new Vector3(0f, bob * Time.deltaTime, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // PlayClipAtPoint so the sound survives the Destroy call below
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position,
                pickupSoundVolume * SettingsMenu.SFXVolume);

        ApplyEffect(other);
        Destroy(gameObject);
    }

    private void ApplyEffect(Collider2D player)
    {
        if (_data == null) return;
        ItemEffectManager.Instance?.Apply(_data, player);
    }
}