using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class EnemyClass : MonoBehaviour
{
    [Header("Stats")]
    public int maxHP = 1;            // Lv1:1, Lv2:2, Lv3:3
    public float moveSpeed = 2f;     // Lv1:3? Lv2:2? Lv3:1? (set per prefab)
    public float attackRange = 0.6f;
    public int attackDamage = 1;
    public float attackInterval = 0.5f;

    [Header("Optional Drops")]
    [Range(0,1)] public float dropBatteryChance = 0f;   // e.g. Lv3: 0.5
    [Range(0,1)] public float dropHalfHeartChance = 0f; // e.g. Lv3: 0.2
    public GameObject batteryPickupPrefab;
    public GameObject halfHeartPrefab;

    [Header("Targeting (priority: Companion -> Player)")]
    public float retargetEvery = 0.5f;

    public Action<EnemyClass> OnEnemyDied; // set by spawner
    Rigidbody2D rb;
    GameManager gm;
    Transform target;
    int hp;
    float lastAtk;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        hp = maxHP;
    }

    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        InvokeRepeating(nameof(FindTarget), 0f, retargetEvery);
    }

    void Update()
    {
        if (target == null) return;

        Vector2 dir = (target.position - transform.position);
        float dist = dir.magnitude;
        dir.Normalize();

        rb.velocity = dir * moveSpeed;

        if (dist <= attackRange && Time.time >= lastAtk + attackInterval)
        {
            lastAtk = Time.time;
            rb.velocity = Vector2.zero;
            TryDamageTarget();
        }
    }

    void TryDamageTarget()
    {
        if (target == null) return;

        var comp = target.GetComponent<CompanionController>();
        if (comp != null) { comp.TakeDamage(attackDamage); return; }

        var player = target.GetComponent<PlayerController>();
        if (player != null) { player.TakeDamage(attackDamage); return; }
    }

    void FindTarget()
    {
        // Companion first
        CompanionController[] comps = FindObjectsOfType<CompanionController>();
        Transform closest = null; float best = float.PositiveInfinity;
        foreach (var c in comps)
        {
            if (c == null) continue;
            float d = Vector2.Distance(transform.position, c.transform.position);
            if (d < best) { best = d; closest = c.transform; }
        }
        if (closest != null) { target = closest; return; }

        // Player fallback
        var player = FindObjectOfType<PlayerController>();
        if (player != null) target = player.transform;
    }

    public void TakeDamage(int amount)
    {
        hp -= amount;
        if (hp <= 0) Die();
    }

    void Die()
    {
        gm?.AddScore(); // +50 as in GameManager
        OnEnemyDied?.Invoke(this);

        if (batteryPickupPrefab && UnityEngine.Random.value < dropBatteryChance)
            Instantiate(batteryPickupPrefab, transform.position, Quaternion.identity);
        if (halfHeartPrefab && UnityEngine.Random.value < dropHalfHeartChance)
            Instantiate(halfHeartPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
