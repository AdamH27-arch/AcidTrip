using UnityEngine;

public enum ItemType { HealthPotion, Bomb, Shield, SpeedBoost, DamageBoost, TimeHiccup, GravityFlip, XPVacuum, Paranoia }
[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]

public class ItemData : ScriptableObject
{
    [Header("Info")]
    public string itemName;
    public string description;
    public ItemType type;

    [Header("Effect Values")]
    public float primaryValue;   // heal amount / bomb damage / boost duration in seconds
    public float secondaryValue; // bomb radius / boost multiplier

    [Header("World Drop")]
    public GameObject droppedPrefab; // assign your prefab here — leave empty to use auto-generated fallback
}