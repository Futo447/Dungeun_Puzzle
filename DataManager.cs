using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PlayerData
{
    public float HP;
    public float MP;
    public string sceneName; // ���� ���� �̸� ����
    public int BookCount;
    public bool Skin;
    public int EquipSkin;
    //public Vector3 position;
}





public class DataManager : MonoBehaviour
{

    //�̱���
    public static DataManager instance;

    PlayerData nowPlayer = new PlayerData(); //�÷��̾� ������ ����
    public void SkillSave(float HP, float MP, string sceneName/*Vector3 position*/)
    {
        nowPlayer.HP = HP;
        nowPlayer.MP = MP;
        nowPlayer.sceneName = sceneName;
        //nowPlayer.position = position;
        SaveData();
    }
    public void SaveDataSetting(float HP, float MP, string sceneName/*,Vector3 position*/)
    {
        nowPlayer.HP = HP;
        nowPlayer.MP = MP;
        nowPlayer.sceneName = sceneName;
        // nowPlayer.position = position;
        SaveData();
    }

    public PlayerData GetSaveData()
    {
        return nowPlayer;
    }
     string path; //���
     int nowSlot; //���� ���Թ�ȣ



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(instance.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);

        path = Application.persistentDataPath + "/save";
        LoadData();
    }

     public void SaveData()
    {
        string data = JsonUtility.ToJson(nowPlayer);
        File.WriteAllText(path + nowSlot.ToString(), data);
        
    }

    public void LoadData()
    {
        FileInfo fi = new FileInfo(path + nowSlot.ToString());
        //FileInfo.Exists�� ���� ���� ���� Ȯ��
        if(fi.Exists)
        {
           string data = File.ReadAllText(path + nowSlot.ToString());
           nowPlayer = JsonUtility.FromJson<PlayerData>(data);
       
            if (nowPlayer.HP == 0 )
            {
            nowPlayer.HP = 100;

            }
           
            if (nowPlayer.MP == 0)
            {
               nowPlayer.MP = 100;

            }

        }
            SaveData();

    }

    //public void DataClear()
    //{
    //    nowSlot = -1;
    //    nowPlayer = new PlayerData();
    //}
}

