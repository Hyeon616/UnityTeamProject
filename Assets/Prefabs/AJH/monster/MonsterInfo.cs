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
    private static Animator animator;


    void Awake()
    {
        //���� ������ ���ӿ�����Ʈ �̸����� �������� ���� ����
        MyObjectName = gameObject.name;
        
        // ������ ����ߵ� ���ӿ�����Ʈ �̸����� ���� ������ �����´�.
        MonsterData monsterData = MonsterDataManager.Instance.GetMonster($"{MyObjectName}");
        animator = GetComponent<Animator>();
        //������ ������ �Լ��� �Ѱܼ� hp,level,str ��� ����
        SetMonsterData(monsterData);
        Debug.Log("1...���� ���� ����.." + _monsterName);        
    }
    //���� ���� ����
    private static void SetMonsterData(MonsterData monsterData)
    {
        _monsterName = monsterData.name;
        _hp = monsterData.hp;
        _level = monsterData.level;
        _str = monsterData.str;
    }
   
    public void TakeDamage(int damageAmout)
    {
        Debug.Log($"���� ����!!! Current Hp : {_hp}");
        _hp -= damageAmout;
        if ( _hp <= 0 )
        {
            animator.SetTrigger("die");
            transform.GetComponent<CapsuleCollider>().enabled = false;
        } else
        {
            animator.SetTrigger("damage");
        }
    }

}
