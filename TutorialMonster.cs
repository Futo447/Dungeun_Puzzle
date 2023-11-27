using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TutorialMonster : MonoBehaviour
{
    Animator anim;
    private NavMeshAgent nav;
    private Transform target;
    private Rigidbody rb; // Rigidbody 컴포넌트 추가

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
        rb = GetComponent<Rigidbody>(); // Rigidbody 컴포넌트 가져오기
        rb.isKinematic = true; // isKinematic 속성을 true로 설정
        nav.speed = speedRun; // NavMeshAgent의 속도를 speedRun 값으로 설정
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
            // 플레이어를 바라보도록 회전
            Vector3 directionToPlayer = target.position - transform.position;
            directionToPlayer.y = 0; // y 축 회전 방지
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
                    // 몬스터의 체력이 0 이하일 때 Dead 애니메이션을 실행합니다.
                    isDead = true;
                    anim.SetTrigger("Dead");
                }
                else
                {
                    // 아직 몬스터가 살아있을 때 Hit 애니메이션을 실행합니다.
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

