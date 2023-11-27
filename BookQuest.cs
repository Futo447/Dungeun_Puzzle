using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookQuest : MonoBehaviour
{
    [SerializeField]
    int bookIndex = 0;
    private void Start()
    {
        if(DataManager.instance.GetSaveData().BookCount > bookIndex)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                // ���⼭ BookCount�� ������Ű�� �����մϴ�.
                DataManager.instance.GetSaveData().BookCount++;
                DataManager.instance.SaveData();
                Destroy(gameObject);
            }
        }
    }
}
