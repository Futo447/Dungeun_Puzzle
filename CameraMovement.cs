using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraMovement : MonoBehaviour
{
    public Transform objectTofollow;
    public float followSpeed =10f;
    public float sensitivity = 100f;
    public float clampAngle = 70f;

    private float rotX;
    private float rotY;

    public Transform realCamera;
    public Vector3 dirNormalized;
    public Vector3 finalDir;
    public float minDistance;
    public float maxDistance;
    public float finalDistance;
    public float smoothness = 10f;

    public Scrollbar sensitivityScrollbar;
    public GameObject optionUI; // OptionUI 게임 오브젝트

    private bool optionUIActive = false;
    private bool isSpeechBubble1Active = false;
    private bool isSpeechBubble2Active = false;

    private Player player;
    void Start()
    {
        rotX = transform.localRotation.eulerAngles.x;
        rotY = transform.localRotation.eulerAngles.y;

        dirNormalized = realCamera.localPosition.normalized;
        finalDistance = realCamera.localPosition.magnitude;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        player = FindObjectOfType<Player>();

    }

    // Update is called once per frame
    void Update()
    {
        optionUIActive = optionUI.activeSelf;

        if (player != null)
        {
            if (player.speechBubble1.activeSelf)
            {
                isSpeechBubble1Active = true;
            }
            else
            {
                isSpeechBubble1Active = false;
            }

            if (player.speechBubble2.activeSelf)
            {
                isSpeechBubble2Active = true;
            }
            else
            {
                isSpeechBubble2Active = false;
            }


        }

        // OptionUI가 활성화되어 있지 않고 Speech Bubble이 활성화되어 있지 않을 때만 마우스 입력을 처리합니다.
        if (!optionUIActive && !isSpeechBubble1Active && !isSpeechBubble2Active)
        {
            //sensitivityScrollbar.value
            rotX += -(Input.GetAxis("Mouse Y") * sensitivity * sensitivityScrollbar.value) * Time.deltaTime;
            rotY += Input.GetAxis("Mouse X") * sensitivity * sensitivityScrollbar.value * Time.deltaTime;

            rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);
            Quaternion rot = Quaternion.Euler(rotX, rotY, 0);
            transform.rotation = rot;
        }

        ToggleCursorVisibility();
    }

    void LateUpdate()
    {

        transform.position = Vector3.MoveTowards(transform.position, objectTofollow.position, followSpeed * Time.deltaTime);

        finalDir = transform.TransformPoint(dirNormalized * maxDistance);

        RaycastHit hit;

        if(Physics.Linecast(transform.position, finalDir, out hit))
        {
            finalDistance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
        }
        else
        {
            finalDistance = maxDistance;
        }
        realCamera.localPosition = Vector3.Lerp(realCamera.localPosition, dirNormalized * finalDistance, Time.deltaTime * smoothness);
    }

    void ToggleCursorVisibility()
    {
        if (optionUIActive || (GameObject.Find("DeathUI") != null && GameObject.Find("DeathUI").activeSelf) || (GameObject.Find("MapUI") != null && GameObject.Find("MapUI").activeSelf) || isSpeechBubble1Active || isSpeechBubble2Active)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
