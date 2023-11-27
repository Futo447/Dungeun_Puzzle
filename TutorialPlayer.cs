using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class TutorialPlayer : MonoBehaviour
{
    float hAxis;
    float vAxis;
    public float speed;
    float originalSpeed;

    public float raycastDistance;
    public LayerMask raycastLayerMask;

    bool wDown;
    bool jDown;
    bool rDown;
    bool sDown;
    bool aDown;
    bool eDown;
    bool fightMode = false;
    bool nomallMode = false;
    public Transform claymore1;
    public Transform claymore2;

    bool isJump;
    bool isDodge;
    bool isInvincible = false;
    public bool isDead = false;

    public bool toggleCameraRotation;
    public float smoothness = 10f;

    bool isClimbing = false;
    bool IsClimbingUp = false;
    bool IsClimbingExit = false;
    bool IsOverClimbUp = false;
    bool IsFall = false;

    bool canAttack = true;
    float attackCooldown = 0f;

    public float maxHP = 100f;
    public float HP;
    public float maxMP = 100f;
    public float MP;
    public Slider hpSlider;
    public Slider mpSlider;
    public float hpRegenPerSecond = 10f;
    public float mpRegenPerSecond = 5f;
    private float mpRegenTimer = 0f;
    float timeSinceLastDamage = 0f;
    bool isRegeneratingHP = false;
    bool swaping = false;


    public Image BloodImage;
    private Color originalBloodImageColor;
    public GameObject deathUI;

    float PreMp = 0;
    float Mptimer = 0;

    public float fallDamageHeightThreshold = 3.0f; // �������� ���� �ּ� ������ ����
    public float fallDamageMultiplier = 30.0f; // ���̿� ���� ������ ����
    bool isFalling = false;
    float fallingStartHeight = 0f;

    public GameObject speechBubble;
    bool isInteractingWithMapNPC = false;

    public GameObject wall1;
    public GameObject CutSc1;
    public GameObject Option;

    public int damage = 10;
    private TutorialMonster tutorialMonster;



    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rigid;
    Animator anim;
    Camera _camera;


    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        originalSpeed = speed;
        _camera = Camera.main;
        nomallMode = true;
        HP = DataManager.instance.GetSaveData().HP;
        MP = DataManager.instance.GetSaveData().MP;
        // transform.position = DataManager.instance.GetSaveData().position;
        originalBloodImageColor = BloodImage.color;
        UpdateUISliders();
        deathUI.SetActive(false);
        tutorialMonster = FindObjectOfType<TutorialMonster>();

    }

    public void Save()
    {
        DataManager.instance.SaveDataSetting(HP, MP, SceneManager.GetActiveScene().name /*, transform.position*/);
    }


    float downPower = 0;
    void FixedUpdate()
    {
        if (eDown)
        {
            InteractWithMapNPC();
        }


        if (isDead)
            return;

        if (HP <= 0)
        {
            isDead = true;
            Die();
        }

        if (MP < maxMP)
        {
            mpRegenTimer += Time.fixedDeltaTime;
            if (mpRegenTimer >= 1f) // 1�ʸ��� MP�� 5�� ȸ���ϵ��� ����
            {
                RegenMP(5); // MP ȸ������ 5�� ����
                mpRegenTimer = 0f; // Ÿ�̸� �ʱ�ȭ
            }
        }

        if (mpSlider != null)
        {
            float mpPercentage = (float)MP / maxMP;
            Mptimer += Time.fixedDeltaTime * 2f;
            mpSlider.value = Mathf.Lerp(PreMp, mpPercentage, Mptimer);
        }

        if (HP < maxHP)
        {
            //  if (isRegeneratingHP)
            {
                timeSinceLastDamage += Time.deltaTime;
                // �������� ���� �� 10���� �������� ���������� ȸ��
                if (timeSinceLastDamage >= 10f)
                {
                    RegenHP();
                }
            }
        }



        if (IsClimbingUp)
            return;

        if (IsClimbingExit)
            return;

        if (IsOverClimbUp)
            return;

        GetInput();
        if (fightMode)
        {
            Move();
            Jump();
            Dodge();
            Swap();
            Attack();
        }
        else if (nomallMode)
        {
            if (isClimbing)
            {
                HandleClimbingInput();
            }
            else
            {
                Move();
                Jump();
                Dodge();
                Swap();
            }

            if (IsFall == false)
                RaycastDetection();

            OverClimb();
        }

        if (Input.GetKey(KeyCode.LeftAlt))
        {
            toggleCameraRotation = true;
        }
        else
        {
            toggleCameraRotation = false;
        }

        if (rigid.velocity.y < -0.1f)
        {
            downPower = rigid.velocity.y;
        }


        if (!isJump && rigid.velocity.y < -0.1f)
        {
            isFalling = true;
            fallingStartHeight = transform.position.y;
        }

        if (HP <= 30)
        {
            SetBloodImageAlpha(0.3f);
        }
        else
        {
            SetBloodImageAlpha(0.0f);
        }


    }

    void LateUpdate()
    {
        if (isDead)
        {
            return;
        }

        if (toggleCameraRotation != true && !isClimbing)
        {
            Vector3 playerRotate = Vector3.Scale(_camera.transform.forward, new Vector3(1, 0, 1));
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(playerRotate), Time.deltaTime * smoothness);
        }
        if (isJump && anim.GetInteger("IsMoveType") == 1)
        {
            if (jDown)
            {
                anim.SetTrigger("JumpWhileRunning");
            }
        }
    }



    void GetInput()
    {
        hAxis = Input.GetAxis("Horizontal");
        vAxis = Input.GetAxis("Vertical");

        anim.SetFloat("x", hAxis);
        anim.SetFloat("y", vAxis);

        wDown = Input.GetButton("Walk");//left shift
        jDown = Input.GetButtonDown("Jump"); //space
        rDown = Input.GetButtonDown("roll"); //f
        sDown = Input.GetButtonDown("Interation"); //q
        aDown = Input.GetButtonDown("Attack"); //mouse 0
        eDown = Input.GetButtonDown("Interaction"); //e


    }


    void EndClimbingUp()
    {
        IsClimbingUp = false;
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            capsuleCollider.enabled = true;
        }

        //Vector3 forwardDirection = transform.forward;
        //float moveDistance = 0.37f;
        //transform.position += forwardDirection * moveDistance;
        anim.applyRootMotion = false;


    }

    void EndClimbingExit()
    {
        IsClimbingExit = false;

        // Vector3 backwardDirection = -transform.forward;
        //float moveDistance = 0.37f;
        //transform.position += backwardDirection * moveDistance;
        //anim.applyRootMotion = false;

    }


    void HandleClimbingInput()
    {

        moveVec = Vector3.zero;
        transform.LookAt(transform.position - HitNormalVector);

        if (Input.GetKey(KeyCode.W))
        {
            moveVec += transform.up;

            Ray ray = new Ray(transform.position + new Vector3(0, 1.5f, 0), transform.forward);

            RaycastHit hit;
            // ����ĳ��Ʈ�� �����ϰ� �浹 ���θ� �˻��մϴ�.
            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
            if (Physics.Raycast(ray, out hit, raycastDistance, LayerMask.GetMask("WallClimb")) == false)
            {
                anim.SetTrigger("ClimbingUp");
                anim.applyRootMotion = true;
                IsClimbingUp = true;
                anim.SetBool("IsClimbing", false);
                isClimbing = false;
                rigid.useGravity = true;
                rigid.velocity = Vector3.zero;
                speed = originalSpeed;
                if (capsuleCollider != null)
                {
                    capsuleCollider.enabled = false;
                }
                Invoke("EndClimbingUp", 3.5f);

            }

        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveVec -= transform.up;
        }

        if (Input.GetKey(KeyCode.A))
        {
            moveVec -= transform.right;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveVec += transform.right;
        }

        moveVec.Normalize();
        transform.position += moveVec * 1f * Time.deltaTime;
        // rigid.isKinematic = false;
        if (Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.Space))
        {
            isClimbing = false;
            anim.SetBool("IsClimbing", false);
            // rigid.isKinematic = false;
            speed = originalSpeed;
            IsFall = true;
            rigid.useGravity = true;

        }
    }

    void Move()
    {
        if (isClimbing)
        {
            return;
        }
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        Vector3 camdir = Camera.main.transform.forward * vAxis;
        camdir += Camera.main.transform.right * hAxis;
        camdir.y = 0;
        camdir.Normalize();
        if (isDodge)
            moveVec = dodgeVec;

        if (moveVec.magnitude > 0)
        {
            if (wDown)
            {
                anim.SetInteger("IsMoveType", 1);
                speed = 5f;
            }
            else
            {
                anim.SetInteger("IsMoveType", 2);
                speed = originalSpeed;
            }



            // ������ ���� ĳ������ ���� �������θ� ȸ���ϵ��� �����մϴ�.
            Quaternion Angle = Quaternion.LookRotation(transform.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, Angle, Time.deltaTime * 10f);

            //if(isJump == false)
            transform.position += camdir * speed * Time.deltaTime;


        }
        else
        {
            anim.SetInteger("IsMoveType", 0);
            // �������� ���� ���� ���� ������ �����մϴ�.

            Quaternion Angle = Quaternion.LookRotation(transform.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, Angle, Time.deltaTime * 10f);
        }
    }
    void Jump()
    {
        if (isClimbing)
        {
            return;
        }
        if (jDown && !isJump && !isDodge)
        {

            rigid.AddForce(Vector3.up * 5, ForceMode.Impulse);
            anim.SetBool("IsJump", true);
            anim.SetTrigger("DoJump");
            isJump = true;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            Debug.LogError(downPower);
            if (downPower < -fallDamageHeightThreshold)
            {
                TakeDamage(-(int)(downPower * fallDamageMultiplier));
                downPower = 0;
            }
            if (isClimbing)
            {

                if (anim.GetBool("IsClimbing") && !anim.GetBool("IsJump"))
                {
                    anim.SetTrigger("ClimbingToGround");
                    //anim.applyRootMotion = true;

                    IsClimbingExit = true;
                    isClimbing = false;
                    anim.SetBool("IsClimbing", false);
                    rigid.useGravity = true;
                    rigid.velocity = Vector3.zero;
                    speed = originalSpeed;
                    rigid.velocity = Vector3.zero;
                    Invoke("EndClimbingExit", 0.1f);
                }

            }
            anim.SetBool("IsJump", false);
            isJump = false;
            IsFall = false;
        }
    }

    void Dodge()
    {
        if (isClimbing)
        {
            return;
        }
        if (rDown && moveVec != Vector3.zero && !isJump && !isDodge && MP >= 20)
        {
            dodgeVec = moveVec;
            speed = 15;
            anim.SetTrigger("DoDodge");
            isDodge = true;
            isInvincible = true;

            UseMana(20);

            Invoke("DodgeOut", 1f);
        }
    }

    void DodgeOut()
    {
        speed *= 3f;
        isDodge = false;
        isInvincible = false;
    }

    void Swap()
    {
        if (isClimbing)
        {
            return;
        }
        if (sDown && !isJump && !isDodge)
        {
            if (swaping == false)
                StartCoroutine(SwapWeapons());

        }
    }
    IEnumerator SwapWeapons()
    {
        swaping = true;
        if (nomallMode)
        {
            anim.SetTrigger("SwapFight");
            fightMode = true;
            nomallMode = false;
        }
        else if (fightMode)
        {
            anim.SetTrigger("SwapNomal");
            fightMode = false;
            nomallMode = true;
        }

        float animationDuration = 0.5f;

        yield return new WaitForSeconds(animationDuration);

        claymore1.gameObject.SetActive(nomallMode);
        claymore2.gameObject.SetActive(fightMode);
        swaping = false;
    }

    Vector3 HitNormalVector;
    void RaycastDetection()
    {
        // ĳ������ ��ġ���� �Ʒ��� ���̸� ���ϴ�.
        //Ray ray = new Ray(transform.position + transform.forward * (0.25f) + new Vector3(0, 1.25f, 0),
        //    Vector3.down * 0.4f);
        Ray ray = new Ray(transform.position + new Vector3(0, 1.25f, 0),
            transform.forward);

        RaycastHit hit;

        raycastLayerMask = LayerMask.GetMask("WallClimb");
        // ����ĳ��Ʈ�� �����ϰ� �浹 ���θ� �˻��մϴ�.
        if (Physics.Raycast(ray, out hit, raycastDistance, raycastLayerMask))
        {
            GameObject hitObject = hit.collider.gameObject;


            if (hitObject.layer == LayerMask.NameToLayer("WallClimb"))
            {
                isClimbing = true;
                anim.SetBool("IsClimbing", true);
                // rigid.isKinematic = true;
                speed = 0;
                HitNormalVector = hit.normal;
                rigid.useGravity = false;
                rigid.velocity = Vector3.zero;
                //  hit.point;
            }
            else
            {

                isClimbing = false;
                anim.SetBool("IsClimbing", false);
                rigid.useGravity = true;
                rigid.velocity = Vector3.zero;
                //  rigid.isKinematic = false;
                speed = originalSpeed;
            }
        }

        else // WallClimb�� �������� ���� ���
        {
            if (isClimbing)
            {
                // ���⼭ ĳ���͸� ����߸��ϴ�.
                isClimbing = false;
                anim.SetBool("IsClimbing", false);
                rigid.useGravity = true;
                rigid.velocity = Vector3.zero;
                speed = originalSpeed;
                IsFall = true;
            }
        }

        Debug.DrawRay(transform.position + new Vector3(0, 1.25f, 0),
           transform.forward * 0.5f, Color.red);

    }

    void OverClimb()
    {
        Ray ray = new Ray(transform.position + new Vector3(0, 1.4f, 0f),
            transform.forward * 0.4f);

        RaycastHit hit;
        raycastLayerMask = LayerMask.GetMask("OverWall");

        if (Physics.Raycast(ray, out hit, raycastDistance, raycastLayerMask))
        {
            CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("OverWall"))
            {

                anim.SetTrigger("OverClimbUp");
                anim.applyRootMotion = true;
                IsOverClimbUp = true;
                if (capsuleCollider != null)
                {
                    capsuleCollider.enabled = false;
                }
                Invoke("EndOverClimbing", 3.1f);
                //hit.collider.enabled = false;

            }

        }
        Debug.DrawRay(transform.position + new Vector3(0, 1.4f, 0f),
              transform.forward * 0.4f, Color.red);

    }

    void EndOverClimbing()
    {
        IsOverClimbUp = false;
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            capsuleCollider.enabled = true;
        }
        anim.applyRootMotion = false;
        // Vector3 forwardDirection = transform.forward;
        // float moveDistance = 0.37f;
        // transform.position += forwardDirection * moveDistance;
        // anim.applyRootMotion = false;
    }

    void Attack()
    {
        if (fightMode && aDown)
        {
            if (canAttack)
            {
                anim.SetTrigger("Hit1");
                canAttack = false;
                Invoke("EnableAttack", attackCooldown);
            }
            else
            {
                Debug.Log("���� ��ٿ� ���Դϴ�.");
            }
        }
    }

    void EnableAttack()
    {
        canAttack = true;
    }

    void UpdateUISliders()
    {
        if (hpSlider != null)
        {
            float hpPercentage = (float)HP / maxHP;
            hpSlider.value = hpPercentage;
        }

        //if (mpSlider != null)
        //{
        //    float mpPercentage = (float)MP / maxMP;
        //    mpSlider.value = mpPercentage;
        //}
    }

    void TakeDamage(int damageAmount)
    {
        if (!isInvincible) // ���� ���°� �ƴ� ���� �������� ����
        {
            HP -= damageAmount;
            if (HP < 0)
            {
                HP = 0; // HP�� ������ ���� �ʵ��� ����
                Die();
            }

            // �������� �Ծ��� �� ȸ�� ���¸� ����
            isRegeneratingHP = false;
            timeSinceLastDamage = 0;
            UpdateUISliders();
        }
    }

    void UseMana(int manaCost)
    {
        if (MP >= manaCost)
        {
            MP -= manaCost;
            UpdateUISliders();
        }
        else
        {
            Debug.Log("MP�� �����Ͽ� ��ų�� ����� �� �����ϴ�.");
        }
    }

    void RegenHP()
    {
        HP += hpRegenPerSecond * Time.deltaTime;
        HP = Mathf.Clamp(HP, 0, maxHP);
        UpdateUISliders();
    }

    void RegenMP(int amount)
    {
        PreMp = MP / maxMP;
        Mptimer = 0;
        MP += amount;
        MP = Mathf.Clamp(MP, 0, maxMP);
        UpdateUISliders();
    }

    void Die()
    {
        anim.SetTrigger("Die");

        deathUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartCoroutine(DisableGameObjectAfterDeath());
    }

    IEnumerator DisableGameObjectAfterDeath()
    {
        yield return new WaitForSeconds(2.8f);

    }

    void SetBloodImageAlpha(float alpha)
    {
        Color newColor = originalBloodImageColor; // ���� ���� ����
        newColor.a = alpha; // ���� ����

        // BloodImage�� ������ ���� ������ �������� ������Ʈ
        BloodImage.color = newColor;
    }

    void InteractWithMapNPC()
    {
        // �÷��̾�� MapNPC �ݶ��̴� ���� ��ȣ �ۿ��� �˻�
        if (isInteractingWithMapNPC)
        {
            if (eDown)
            {
                speechBubble.SetActive(true);
            }
        }
        else
        {
            speechBubble.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FireDT"))
        {
            // �÷��̾ 'FireDT' �±׿� ����� ��
            TakeDamage(20); // 20�� ������
        }

        if (other.CompareTag("MapNPC"))
        {
            isInteractingWithMapNPC = true;
        }

        if (other.CompareTag("attack"))
        {
            TakeDamage(tutorialMonster.Gdamage);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MapNPC"))
        {
            isInteractingWithMapNPC = false;
            speechBubble.SetActive(false);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("DutorialOB") && Input.GetButton("Interaction"))
        {
            if (wall1 != null)
            {
                CutSc1.SetActive(true);
                Option.SetActive(false);
                StartCoroutine(DeactivateAfterTime(4.0f));
                //wall1.transform.Translate(new Vector3(0, -8, 0));
            }
        }
    }

    IEnumerator DeactivateAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        if (CutSc1 != null)
        {
            CutSc1.SetActive(false);
            Option.SetActive(true);
        }
    }

    public Collider trigger;

    void DamageStart()
    {
        trigger.enabled = false;
    }

    void Damage()
    {
        trigger.enabled = true;
    }
    void DamageEnd()
    {
        trigger.enabled = false;
    }
}

