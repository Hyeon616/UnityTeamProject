using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static DataManager;

public class playerAnimator : MonoBehaviour
{
    public Animator _animator;
    private CharacterController _characterController;   
    private Vector3 _moveDirection;
    private bool _isRunning = false; // �ٱ� ���¸� �����ϴ� ����
    private int _skillA = -1;
    private int _skillB = -1;
    public bool isAction = false;
    private float _gravity = -9.81f;
    private float _velocity;
    private static string MyObjectName;
    private static string _PlayerName;
    private static int _hp;
    private static int _level; 
    private static int _str;
    private static bool isSkillACooldown = false;
    private static bool isSkillBCooldown = false;


    [SerializeField]
    private Collider WeaponCollider;

    void Start()
    {
        StartCoroutine(SkillCooldown());
        MyObjectName = gameObject.name;
        PlayerData playerData = DataManager.Instance.GetPlayer($"{MyObjectName}");
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        SetPlayerData(playerData);
        Debug.Log(_PlayerName);
        Debug.Log(_hp);
        Debug.Log(_level);
        Debug.Log(_str);

    }
    IEnumerator SkillCooldown()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // 1�ʸ��� üũ

            // ��ų A�� ��ٿ��� Ȱ��ȭ�Ǿ� �ִ� ���
            if (isSkillACooldown)
            {
                yield return new WaitForSeconds(5f); //5�ʰ� ���
                isSkillACooldown = false; // ��ٿ� ����
            }

            // ��ų B�� ��ٿ��� Ȱ��ȭ�Ǿ� �ִ� ���
            if (isSkillBCooldown)
            {
                yield return new WaitForSeconds(5f); // 5�ʰ� ���
                isSkillBCooldown = false; // ��ٿ� ����
            }
        }
    }
    private static void SetPlayerData(PlayerData playerData)
    {
        _PlayerName = playerData.name;
        _hp = playerData.hp;
        _level = playerData.level;
        _str = playerData.str;
    }

    void Update()
    {
        ApplyGravity();
        if (isAction) return;

        bool hasControl = (_moveDirection != Vector3.zero);
        if (hasControl)
        {
            // �̵� �������� ĳ���͸� ȸ����ŵ�ϴ�.
            if (_characterController.isGrounded)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }

            // ĳ���͸� �̵���ŵ�ϴ�.
            _characterController.Move(_moveDirection * 6f * Time.deltaTime);

            // �ٱ� ���¸� �����մϴ�.
            _animator.SetBool("isRunning", _isRunning);
        }
        else
        {
            _animator.SetBool("isRunning", false); // �̵����� ���� ���� �ٱ� ���� ����
        }
    }
    public void TakeDamage(int damageAmout)
    {
        Debug.Log($"���� ����!!! Current Hp : {_hp}");
        _hp -= damageAmout;
        if (_hp <= 0)
        {
           
        }
        else
        {
            _animator.SetTrigger("hitCharacter");
        }
    }
    void ApplyGravity()
    {
        // �߷��� �����մϴ�.
        if (!_characterController.isGrounded)
        {
            _velocity += _gravity * Time.deltaTime;
        }
        else
        {
            _velocity = 0f;
        }

        // ���� �̵��� �����մϴ�.
        _moveDirection.y = _velocity;
    }

    #region SEND_MESSAGE
    void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>(); // �Է� ���� ���� ��������
        if (input != null)
        {
            _moveDirection = new Vector3(input.x, 0f, input.y);

            // �̵� �Է��� ���� ���� �ٱ� ���·� ����
            _isRunning = _moveDirection.magnitude > 0;
        }
    }

    void OnSkillA(InputValue value)
    {
        if (!isSkillACooldown)
        {
            _animator.SetInteger("skillA", 0);
            _animator.Play("ChargeSkillA_Skill");
            isSkillACooldown = true; // ��ų A ��ٿ� Ȱ��ȭ
        }
    }

    void OnSkillB(InputValue value)
    {
        if (!isSkillBCooldown)
        {
            //        _animator.SetInteger("skillB", 0);
            //      _animator.Play("SkillA_unlock 1");
            if (isAction) return;
            StartCoroutine(ActionTimer("SkillA_unlock 1", 2.2f));
            isSkillBCooldown = true;
        }
    }

    public void onWeaponAttack()
    {
        _animator.SetTrigger("onWeaponAttack");
    }

    void OnClick()
    {
        //if (isAction) return;
        //StartCoroutine(ActionTimer("onWeaponAttack", 2.2f));
        onWeaponAttack();
    }
    IEnumerator ActionTimer(string actionName, float time)
    {
        isAction = true;

        if (actionName != "none") _animator.Play(actionName);

        yield return new WaitForSeconds(time);
        isAction = false;
    }
    public void EnableWeapon() 
    {
        WeaponCollider.enabled = true;
    }
    public void DisableWeapon()
    {
        WeaponCollider.enabled = false;
    }
    #endregion
}