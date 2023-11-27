using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinQuest : MonoBehaviour
{
    public GameObject image2;
    public GameObject image3;


    public void CheckBookCountOnClick()
    {
        CheckBookCount();
    }

    private void CheckBookCount()
    {
        int bookCount = DataManager.instance.GetSaveData().BookCount;

        if (bookCount >= 5)
        {
            DataManager.instance.GetSaveData().Skin = true;
            DataManager.instance.SaveData();
            image2.SetActive(false);
            image3.SetActive(true);
        }
        else
        {
            image2.SetActive(true);
            image3.SetActive(false);
        }
    }

    public void IncreaseBookCount()
    {
        DataManager.instance.GetSaveData().BookCount++;
        DataManager.instance.SaveData();
        CheckBookCount();
    }
}