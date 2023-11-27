using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TutorialMonster : MonoBehaviour
{
    Animator anim;
    private NavMeshAgent nav;
    private Transform target;
    private Rigidbody rb; // Rigidbody ������Ʈ �߰�

    public float speedRun = 1.5f;
    public Transform[] waypoints;
    public float trackingRange = 5.0f;
    public float runSpeedThreshold = 1.0f;
    public float attackRange = 1.5f;
    public float punchRange = 3f;
    public float viewRadius = 1.5f;
    public float viewAngle = 90;
    public float meshResolution = 1.0f;
    public int edgeIterations = 4;
    public float edgeDistance = 0.5f;
    public float startWaitTime = 4;
    public float timeToRotate = 2;
    public float punchforce = 3f;
    private bool isAttacking = false;


    //static int MonsterCount =0;

    public static int MonsterCount { get; private set; }
    private void Awake()
    {
        ++MonsterCount;
    }
    private void OnDestroy()
    {
        --MonsterCount;
    }
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        nav = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>(); // Rigidbody ������Ʈ ��������
        rb.isKinematic = true; // isKinematic �Ӽ��� true�� ����
        nav.speed = speedRun; // NavMeshAgent�� �ӵ��� speedRun ������ ����
        target = GameObject.FindGameObjectWithTag("Player").transform;

    }

    // Update is called once per frame
    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        if (distanceToPlayer < trackingRange)
        {
            nav.SetDestination(target.position);

            if (distanceToPlayer <= nav.stoppingDistance)
            {
                anim.SetBool("Walk", false);
                anim.SetTrigger("attack");
                int value = Random.Range(0, 2);
                anim.SetInteger("Random", value);
            }
            else
            {
                anim.SetBool("Walk", true);
            }
        }
        else
        {
            anim.SetBool("Walk", false);
        }

        if (isAttacking)
        {
            // �÷��̾ �ٶ󺸵��� ȸ��
            Vector3 directionToPlayer = target.position - transform.position;
            directionToPlayer.y = 0; // y �� ȸ�� ����
            Quaternion newRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = newRotation;
        }
        if (isDead)
        {

        }
    }

    void Die()
    {
        Destroy(gameObject, 3f);
    }

    public bool isDead = false;
    public float attackTime = 10f;
    public float delayTime = 0f;
    public int monsterHP = 30;
    public int Gdamage { get { return _Gdamage; } }
    private int _Gdamage = 5;

    Collider attackcol;
    private void OnTriggerEnter(Collider other)
    {
        if (isDead)
            return;

        if (other.CompareTag("Knife"))
        {
            TutorialPlayer player = other.GetComponentInParent<TutorialPlayer>();
            //if (player != null)
            {
                monsterHP -= player.damage;
                Debug.Log(monsterHP);
                if (monsterHP <= 0)
                {
                    Collider[] colliders = GetComponents<Collider>();
                    foreach (Collider col in colliders)
                    {
                        Destroy(col);
                    }
                    nav.enabled = false;
                    // ������ ü���� 0 ������ �� Dead �ִϸ��̼��� �����մϴ�.
                    isDead = true;
                    anim.SetTrigger("Dead");
                }
                else
                {
                    // ���� ���Ͱ� ������� �� Hit �ִϸ��̼��� �����մϴ�.
                    anim.SetTrigger("Hit");
                }
            }
        }
    }
    public Collider trigger;
    void attack()
    {
        trigger.enabled = true;
    }
    void disable()
    {
        trigger.enabled = false;
    }


}

