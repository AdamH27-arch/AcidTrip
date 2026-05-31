using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/Enemy Data")]

[System.Serializable]
public class ItemDrop
{
    public ItemData item;

    [Range(0f, 100f)]
    public float dropChance; // percentage 0-100
}

public class EnemyData : ScriptableObject
{
    [Header("Item Drops")]
    public List<ItemDrop> possibleDrops = new List<ItemDrop>();
    [Header("Behavior")]
    public bool dealsContactDamage = true;

    [Header("Identity")]
    public string enemyName = "Enemy";

    [Header("Movement")]
    public float moveSpeed = 2.5f;

    [Header("Health")]
    public int maxHealth = 30;

    [Header("Damage")]
    public int contactDamage = 10;
    public float damageCooldown = 0.8f;

    [Header("Rewards")]
    public int xpValue = 10;
    public GameObject xpOrbPrefab;

    [Header("Visuals")]
    public Color color = Color.white;
    public Vector2 scale = Vector2.one;

    [Header("Ranged Attack")]
    public bool canDoRangedAttack = false;
    public GameObject bulletPrefab;
    public float fireRate = 1.2f;
    public float attackRange = 7f;
    public float stopRange = 4f;

    [Header("Movement Pattern")]
    public MovementPattern movementPattern = MovementPattern.Chase;
    public float preferredRange = 5f;
    public float holdDuration = 2f;
    public float advanceDuration = 2f;
    public float retreatDuration = 1.5f;
    public float zigzagFrequency = 3f;
    public float zigzagAmplitude = 1.5f;

    [Header("Orbiter")]
    public float orbitRadius = 4f;
    public float orbitSpeed = 2.5f;

    [Header("Pulsator")]
    public float sprintDuration = 0.4f;
    public float freezeDuration = 0.5f;

    [Header("Coward")]
    public float fleeRange = 5f;
    public float fleeSpeed = 4f;

    //The weeping enemy will move twice as fast towards the player when the
    //player crosshair isn't facing the enemy;
    [Header("Weeping")]
    public float chaseSpeed = 6f;
    public float detectionAngle = 45f;

    //Teleporter enemy will teleport within a certain distance of enemy, wait a few
    //seconds, fire a bullet ring, wait a few more seconds and then teleport to a random
    //angle around the player.
    [Header("Teleporter")]
    public float teleportDistance = 5f;
    public float waitBeforeFire = 2f;
    public float waitAfterFire = 3f;
    public int projectileCount = 8;
    public float bulletSpeed = 5f;
    public GameObject TeleporterBullet;

    //Chases the player and fires a projectile that slows the player for a small time frame.
    [Header("Witch")]
    public float timeTillBlast = 5f;
    public GameObject witchBlastPrefab;
    public int witchBlastDamage = 5;
    


    //The exploding hobbit will try to run into the player. Upon being killed he will explode.
    [Header("ExplodingHobbit")]
    public bool ExplodesOnDeath = false;
    public GameObject ExplosionPrefab;
    public float HobbitSpeed = 3f;


    //IronGolemBoss Data
    [Header("IronGolemBoss")]
    public GameObject IronGolemSpearPrefab;
    public float GolemSpeed = 3f;
    public float spearCooldown = 2f;
    public float golemChargeSpeed = 10f;
    public float golemChargeDuration = 4f;


    //LordOfHobbits Data
    [Header("LordOfHobbits")]
    public GameObject explodingHobbitPrefab;
    public float lordOfHobbitsSpeed;


    //Final boss Data
    [Header("FinalBoss")]
    public GameObject lightningBoltPrefab;
    public GameObject lightningSpearPrefab;
    public GameObject lightningArrowPrefab;
    public int lightningBoltCount = 6;
    public int spearCount = 10;
    public float attackDuration = 3f;
    public float attackCooldown = 2f;
    public float spearBarrageDiagonalChance = 0.5f;


    
    
    
    
  
    public enum MovementPattern { Chase, Ranged, Zigzag, Orbiter, Pulsator, Coward, Weeping, Teleporter, ExplodingHobbit, Witch}

}