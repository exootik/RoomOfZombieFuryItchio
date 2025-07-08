using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    public NavMeshAgent navAgent;
    public enum ZombieState { Idle, Chase, Attack, Dead }
    public ZombieState currentState = ZombieState.Idle;
    public Transform player;
    public Animator animator;
    public float chaseDistance = 300f;
    public float attackDistance = 2f;
    public float attackCooldown = 2f;
    public float attackDelay = 1.5f;
    public int damage = 1;
    public int health = 5;
    public int maxHealth = 5;
    public int currentHealth;

    private CapsuleCollider capsuleCollider;
    private bool isAttacking;
    private float lastAttackTime;

    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        lastAttackTime = -attackCooldown;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.Log("joueur pas trouve");
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case ZombieState.Idle:
                animator.SetBool("IsRunning", false);
                animator.SetBool("IsAttacking", false);
                if (Vector3.Distance(transform.position, player.position) <= chaseDistance)
                    currentState = ZombieState.Chase;
                break;
            case ZombieState.Chase:
                animator.SetBool("IsRunning", true);
                animator.SetBool("IsAttacking", false);
                navAgent.SetDestination(player.position);
                if (Vector3.Distance(transform.position, player.position) <= attackDistance)
                    currentState = ZombieState.Attack;
                break;
            case ZombieState.Attack:
                animator.SetBool("IsAttacking", true);
                navAgent.SetDestination(player.position);
                if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
                {
                    StartCoroutine(AttackWithDelay());
                }
                if (Vector3.Distance(transform.position, player.position) > attackDistance)
                    currentState = ZombieState.Chase;
                break;
            case ZombieState.Dead:
                animator.SetBool("IsRunning", false);
                animator.SetBool("IsAttacking", false);
                animator.SetBool("IsDead", true);
                navAgent.enabled = false;
                capsuleCollider.enabled = false;
                enabled = false;
                break;
        }
    }

    private IEnumerator AttackWithDelay()
    {
        isAttacking = true;

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null) 
        {
            playerController.TakeDamage(damage);
        }

        yield return new WaitForSeconds(attackDelay);
        isAttacking = false;
        lastAttackTime = Time.time;
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentState == ZombieState.Dead)
            return;

        health -= damageAmount;
        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }

    private void Die()
    {
        currentState = ZombieState.Dead;
    }
}
