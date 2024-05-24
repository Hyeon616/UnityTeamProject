using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GetMonstersData : MonoBehaviour
{
    [System.Serializable]
    public class MonsterData
    {
        public int MonsterID;
        public string MonsterName;
        public int Health;
        public int AttackPower;
        // �ʿ��� ���� �Ӽ� �߰�
    }

    [System.Serializable]
    public class MonstersList
    {
        public List<MonsterData> monsters;
    }

    private string apiUrl = "http://localhost:3000/api/monsters"; // ���� URL �� ��������Ʈ

    void Start()
    {
        StartCoroutine(GetMonstersDataCoroutine());
    }

    IEnumerator GetMonstersDataCoroutine()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error: " + request.error);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                MonstersList monstersList = JsonUtility.FromJson<MonstersList>("{\"monsters\":" + jsonResponse + "}");
                UpdateMonsters(monstersList.monsters);
            }
        }
    }

    void UpdateMonsters(List<MonsterData> monsters)
    {
        foreach (MonsterData monsterData in monsters)
        {
            string monsterObjectName = "mon" + monsterData.MonsterID;
            GameObject monsterObject = GameObject.Find(monsterObjectName);
            if (monsterObject != null)
            {
                MonsterDatabase monsterComponent = monsterObject.GetComponent<MonsterDatabase>();
                if (monsterComponent != null)
                {
                    monsterComponent.MonsterID = monsterData.MonsterID;
                    monsterComponent.MonsterName = monsterData.MonsterName;
                    monsterComponent.Health = monsterData.Health;
                    monsterComponent.AttackPower = monsterData.AttackPower;
                    // �ʿ��� ���� �Ӽ� ����
                }
            }
            else
            {
                Debug.LogError("Monster object not found: " + monsterObjectName);
            }
        }
    }

}
