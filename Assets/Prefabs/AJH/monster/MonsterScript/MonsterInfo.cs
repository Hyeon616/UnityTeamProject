using Unity.VisualScripting;
using UnityEngine;
// using static DataManager;

public class MonsterInfo : MonoBehaviour
{
    public string MyObjectName;
    public string _monsterName; // name ������ _monsterName���� ����
    public int _hp; // hp ������ _hp�� ����
    public int _level; // level ������ _level�� ����
    public int _str; // str ������ _str�� ����

    public Animator animator;
    public Transform wayPoint;

    [SerializeField] private PlayerAttackSound playerSound;
    [SerializeField] private MonsterType monsterType; //���� ���� ���� ������Ʈ ���� ����(���Ϳ� ����)
    

    void Awake()
    {
        //���� ������ ���ӿ�����Ʈ �̸����� �������� ���� ����
        //MyObjectName = (gameObject.name == "LazerMon1(Clone)" || gameObject.name == "LazerMon1" || gameObject.name == "LazerMon(Clone)") ? "mon6" : gameObject.name;
        
        // ������ ����ߵ� ���ӿ�����Ʈ �̸����� ���� ������ �����´�.
        //MonsterData monsterData = DataManager.Instance.GetMonster($"{MyObjectName}");
        animator = GetComponent<Animator>();
        //������ ������ �Լ��� �Ѱܼ� hp,level,str ��� ����
        //SetMonsterData(monsterData);
        //Debug.Log("1...���� ���� ����.." + _monsterName);        
    }
    //���� ���� ����
    //private void SetMonsterData(MonsterData monsterData)
    //{

    //    this._monsterName = monsterData.name;
    //    this._hp = monsterData.hp;
    //    this._level = monsterData.level;
    //    this._str = monsterData.str;
    //    if (gameObject.name == "LazerMon1(Clone)" || gameObject.name == "LazerMon1" )
    //    {
    //        this._hp = 10;
    //        this._str = 10;
    //    } else if (gameObject.name == "LazerMon(Clone)")
    //    {
    //        this._hp = 100;
    //        this._str = 100;
    //    }
    //}

    public void TakeDamage(int damageAmout)
    {
        //Debug.Log($"���� ����!!! Current Hp : {_hp}");
        //Debug.Log(gameObject.name);
        _hp -= damageAmout;
        if (_hp <= 0)
        {
            animator.SetTrigger("die");
            playerSound.MonsterDie();//���� ��� ���� ���
            transform.GetComponent<CapsuleCollider>().enabled = false;

        }
        else
        {
            animator.SetTrigger("damage");
            if (monsterType.monsterType == 1)
            {
                playerSound.BiologyAttack();// ������ ���� Ÿ����
            }
            else if (monsterType.monsterType == 2)
            {
                playerSound.NonBiologyAttack(); // ������� ���� Ÿ����
            }
        }
    }
}