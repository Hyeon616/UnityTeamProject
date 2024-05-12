using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static MonsterDataManager;

public class MonsterInfo : MonoBehaviour
{
    private static string MyObjectName;
    private static string _monsterName; // name ������ _monsterName���� ����
    private static int _hp; // hp ������ _hp�� ����
    private static int _level; // level ������ _level�� ����
    private static int _str; // str ������ _str�� ����



    void Awake()
    {
        MyObjectName = gameObject.name;
        MonsterData monsterData = MonsterDataManager.Instance.GetMonster($"{MyObjectName}");
        SetMonsterData(monsterData);
        Debug.Log("Monster Name: " + _monsterName);
        Debug.Log("Monster HP: " + _hp);
        Debug.Log("Monster Level: " + _level);
        Debug.Log("Monster Strength: " + _str);
    }
    private static void SetMonsterData(MonsterData monsterData)
    {
        _monsterName = monsterData.name;
        _hp = monsterData.hp;
        _level = monsterData.level;
        _str = monsterData.str;
    }
    public void testc()
    {
    
    }

    //GameObject monster = ObjectPool.Instance.GetInactiveObject($"mon{def}");
    //monster.GetComponent<monsterinfo>().Init(monsterData);
}
