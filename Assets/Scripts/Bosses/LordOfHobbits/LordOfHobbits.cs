using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NUnit.Framework;

public class LordOfHobbits : Enemy
{
    [Header("Lord of Hobbits Settings")]
    [SerializeField] private float padding = 10f;
    [SerializeField] private float hobbitSpacing = 3f;//Min distance between hobbits on arc
    [SerializeField] private float ringCooldown = 8f;
    [SerializeField] private float arcDegrees = 360f; //size of semicircle
    [SerializeField] private float _attackCooldownTimer = 0f;
    [SerializeField] private float hobbitLifetime = 10f;

    private List<GameObject> _spawnedHobbits = new List<GameObject>();
    private Transform childTransform;
    private bool _isAttacking = false;
    
    
    

    //Enums representing boss attacks.
    private enum BossAttack {RingOfHobbits}

    protected override void Awake()
    {
        base.Awake();
        //For some reason the sprite is flipping in the wrong direction during update. THis corrects that.
        invertSprite = true;

    }

        
    
    protected override void MoveTowardPlayer()
    {
        if (Player == null || _isAttacking) return;

        
        Vector2 Direction = ((Vector2)Player.position - (Vector2)_rb.position).normalized;
        _rb.linearVelocity = Direction * Data.lordOfHobbitsSpeed;
        if (_attackCooldownTimer > 0f)
        {
            _attackCooldownTimer -= Time.fixedDeltaTime;
        }
        else if (!_isAttacking)
        {
            BossAttack nextAttack = (BossAttack)Random.Range(0, 1);
            StartCoroutine(ExecuteAttack(nextAttack));
        }


    }

    //Called by TakeDamage() to spawn a hobbit near the boss after being hit by the player.
    /*
    private void SpawnHobbitNearBoss()
    {
        if (Data.explodingHobbitPrefab == null)
        {
            return; 
        }
        Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * 1.5f;
        GameObject hobbit = Instantiate(Data.explodingHobbitPrefab, spawnPos, Quaternion.identity);
        
    }
    */

    private IEnumerator DestroyHobbitAfterDelay(GameObject hobbit, float delay)
    {
        yield return new WaitForSeconds(delay);
        if(hobbit != null)
        {
            _spawnedHobbits.Remove(hobbit);
            Destroy(hobbit);
        }
    }

    //As the hobbit lord takes dammage, hobbits will spawn in a random location around the boss.
    //These hobbits are not destroyed via a timer (hobbit lifetime) and so must be handled by the
    //player.(I've taken out the spawning mechanic for now. I think it makes the fight too hard.
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        
    }

    private IEnumerator ExecuteAttack(BossAttack nextAttack)
    {
        _isAttacking = true;
        switch (nextAttack)
        {
            case BossAttack.RingOfHobbits:
                yield return StartCoroutine(AttackHalfCircle());
                break;
        }
        _isAttacking = false;
        _attackCooldownTimer = ringCooldown;
    }


    //Spawns a ring of hobbits around the player using the LordofHobbits as the center and the
    //distance between the player and the boss plus some padding as the radius of the circle.
    //These hobbits are mostly to coral the player closer to the boss so they are destroyed after
    //a few seconds, hobbitLifetime, to prevent too many hobbits from spawning and consuming memory.
    private IEnumerator AttackHalfCircle()
    {
        if(Data.explodingHobbitPrefab == null || Player == null)
        {
            yield break;
        }
        
        //Calculate radius - distance from boss to player + padding.
        float radius = Vector2.Distance(transform.position, Player.position) + padding;

        //Direction from boss to player - Hobbits spawn in a Concave manner with
        //boss position as center.
        Vector2 directionToPlayer = ((Vector2)Player.position - (Vector2)transform.position).normalized;

        //Angle pointing away from boss (behind the player)
        float centerAngle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

        //Calculate how many hobbits fit along the arc
        float arcRadians = arcDegrees * Mathf.Deg2Rad;
        float arcLength = arcRadians * radius;
        int hobbitCount = Mathf.Max(3, Mathf.FloorToInt(arcLength / hobbitSpacing));

        float angleStep = arcDegrees / (hobbitCount - 1);
        float startAngle = centerAngle - arcDegrees * 0.5f;

        for(int i = 0; i < hobbitCount; i++)
        {
            float angle = (startAngle + angleStep * i) * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Vector2 spawnPos = (Vector2)transform.position + offset;

            GameObject hobbit = Instantiate(Data.explodingHobbitPrefab, spawnPos, Quaternion.identity);
            _spawnedHobbits.Add(hobbit);
            StartCoroutine(DestroyHobbitAfterDelay(hobbit, hobbitLifetime));

            //Delay to cause hobbits to spawn in a staggered manner.
            yield return new WaitForSeconds(0.05f);
          
        }
        yield return new WaitForSeconds(1f);
        
    }


}