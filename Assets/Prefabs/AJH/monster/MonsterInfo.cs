using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static DataManager;

public class MonsterInfo : MonoBehaviour
{
    public string MyObjectName;
    public string _monsterName; // name ������ _monsterName���� ����
    public int _hp; // hp ������ _hp�� ����
    public int _level; // level ������ _level�� ����
    public int _str; // str ������ _str�� ����
    public Animator animator;


    void Awake()
    {
        //���� ������ ���ӿ�����Ʈ �̸����� �������� ���� ����
        MyObjectName = gameObject.name;
        
        // ������ ����ߵ� ���ӿ�����Ʈ �̸����� ���� ������ �����´�.
        MonsterData monsterData = DataManager.Instance.GetMonster($"{MyObjectName}");
        animator = GetComponent<Animator>();
        //������ ������ �Լ��� �Ѱܼ� hp,level,str ��� ����
        SetMonsterData(monsterData);
        Debug.Log("1...���� ���� ����.." + _monsterName);        
    }
    //���� ���� ����
    private void SetMonsterData(MonsterData monsterData)
    {
        this._monsterName = monsterData.name;
        this._hp = monsterData.hp;
        this._level = monsterData.level;
        this._str = monsterData.str;
    }
   
    public void TakeDamage(int damageAmout)
    {
        //Debug.Log($"���� ����!!! Current Hp : {_hp}");
        Debug.Log(gameObject.name);
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
