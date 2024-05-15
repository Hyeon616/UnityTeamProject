using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static DataManager;

public class playerAnimator : MonoBehaviour
{
//������ ����
    public Animator _animator; 
    private CharacterController _characterController;    
    private Vector3 _moveDirection;              // �÷��̾��� �̵� ����
    private bool _isRunning = false;             // �÷��̾ �޸��� �ִ��� ���θ� �����ϴ� �÷���
    private int _skillA = -1;                    // ��ų A ����ó�� ���� ���� ��ų
    private int _skillB = -1;                    // ��ų B �پ �ٸ��콺ó�� ��½�ų 
    public bool isAction = false;                // �÷��̾ �׼��� ���� ������ ���θ� �����ϴ� �÷���
    private float _gravity = -9.81f;             // �߷� ���ӵ�
    private float _velocity;                     // �÷��̾��� ���� �ӵ�
    private static string MyObjectName;          // �÷��̾� ������Ʈ �̸�
    private static string _PlayerName;           // �÷��̾� �̸�  
    private static int _hp;                      // �÷��̾� ü��
    private static int _level;                   // �÷��̾� ����
    private static int _str;                     // �÷��̾� ��
    private static bool isSkillACooldown = false;// ��ų A ��ٿ� ���θ� �����ϴ� �÷���
    private static bool isSkillBCooldown = false;// ��ų B ��ٿ� ���θ� �����ϴ� �÷���
    public float dashCooldownDuration = 5f;      // ��� ��ٿ� ���� �ð�(��)
    private bool isDashCooldown = false;         // ��� ��ٿ� ���¸� �����ϴ� �÷�

    [SerializeField]
    private Collider WeaponCollider;             // ���� �ݶ��̴�

    void Start()
    {
        StartCoroutine(SkillCooldown());         // ��ų ��ٿ��� �����ϴ� �ڷ�ƾ ����
        MyObjectName = gameObject.name;          // �÷��̾� ������Ʈ�� �̸� ��������
        PlayerData playerData = DataManager.Instance.GetPlayer($"{MyObjectName}"); // DataManager�� ����Ͽ� �÷��̾� ������ ��������
        _animator = GetComponent<Animator>();
        _characterController = GetComponent<CharacterController>();
        SetPlayerData(playerData);
      /*  Debug.Log(_PlayerName);
        Debug.Log(_hp);
        Debug.Log(_level);
        Debug.Log(_str);*/

    }
// ��ų ��ٿ��� �����ϴ� �ڷ�ƾ
    IEnumerator SkillCooldown()                      
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);     // 1�ʸ��� üũ    

             
            if (isSkillACooldown)                    // ��ų A ��ٿ� ���� Ȯ�� �� ó��
            {
                yield return new WaitForSeconds(5f); //5�ʰ� ���
                isSkillACooldown = false;            // ��ٿ� ����
            }

            
            if (isSkillBCooldown)                    // ��ų B�� ��ٿ��� Ȱ��ȭ�Ǿ� �ִ� ���
            {
                yield return new WaitForSeconds(5f); // 5�ʰ� ���
                isSkillBCooldown = false;            // ��ٿ� ����
            }
        }
    }

// �÷��̾� ������ ���� �Լ�
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
        if (isAction) return; //�������̰ų� 2����ų �ߵ����� �� ĳ���̵� X�ϱ� ���� return

        bool hasControl = (_moveDirection != Vector3.zero);
        if (hasControl)
        {
            if (_characterController.isGrounded)// �̵� �������� ĳ���͸� ȸ����ŵ�ϴ�.
            {
                Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }            
            _characterController.Move(_moveDirection * 6f * Time.deltaTime);// ĳ���͸� �̵���ŵ�ϴ�.
            _animator.SetBool("isRunning", _isRunning);// �ٱ� ���¸� �����մϴ�.
        }
        else
        {
            _animator.SetBool("isRunning", false); // �̵����� ���� ���� �ٱ� ���� ����
        }
    }

    // �÷��̾ ���ظ� ���� �� ȣ��Ǵ� �Լ�
    public void TakeDamage(int damageAmout)
    {
        //Debug.Log($"���� ����!!! Current Hp : {_hp}");
        _hp -= damageAmout;
        if (_hp <= 0)
        {
            _animator.SetTrigger("Die");

        }
        else
        {
            _animator.SetTrigger("hitCharacter");
        }
    }

    // �߷��� �����ϴ� �Լ�
    void ApplyGravity()
    {
       
        if (!_characterController.isGrounded) // �߷��� �����մϴ�.
        {
            _velocity += _gravity * Time.deltaTime;
        }
        else
        {
            _velocity = 0f;
        }
        _moveDirection.y = _velocity; // ���� �̵��� �����մϴ�.
    }

    #region SEND_MESSAGE
    void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>(); // �Է� ���� ���� ��������
        if (input != null)
        {
            _moveDirection = new Vector3(input.x, 0f, input.y);
            _isRunning = _moveDirection.magnitude > 0;// �̵� �Է��� ���� ���� �ٱ� ���·� ����
        }
    }
    void OnDash()
    {
        if (!isDashCooldown) // ��ð� ��ٿ� ���� �ƴ��� Ȯ��
        {
            Dash(); // ��� ����
            StartCoroutine(StartDashCooldown()); // ��� ��ٿ� ����
        }
    }
    // ��� ��ٿ��� �����ϴ� �ڷ�ƾ
    IEnumerator StartDashCooldown()
    {
        isDashCooldown = true; // ��� ��ٿ� ����
        yield return new WaitForSeconds(dashCooldownDuration); // ��ٿ� �ð� ���� ���
        isDashCooldown = false; // ��� ��ٿ� ����
    }
    // �÷��̾ ��� ������ ��ġ�� �ε巴�� �̵���Ű�� �ڷ�ƾ
    IEnumerator MovePlayerToPosition(Vector3 startPosition, Vector3 endPosition, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;// �÷��̾ ��Ȯ�� ��ġ�� �����ϵ��� ����
    }

    //��ý�ų Shift ���� ���濹��
    void Dash()
    {
        Vector3 dashDirection = transform.forward; // �÷��̾ ���� �ִ� �������� ���
        float dashDistance = 5f;  // ��� �Ÿ�
        float dashDuration = 0.2f; // ��� ���� ��

        // ��� ������ ��ġ ���
        Vector3 dashDestination = transform.position + dashDirection * dashDistance;

        // �÷��̾��� ��ġ�� ������ �̵��Ͽ� ��� ����
        StartCoroutine(MovePlayerToPosition(transform.position, dashDestination, dashDuration));

        // ���⿡ ��� �ִϸ��̼� ����� ���� �߰� �۾��� �߰� ����
    }
    void OnSkillA(InputValue value)
    {
        if (!isSkillACooldown)
        {
            _animator.SetInteger("skillA", 0);// ��ų A �ִϸ��̼� ���
            _animator.Play("ChargeSkillA_Skill"); // ��ų A ���� �ִϸ��̼� ���
            isSkillACooldown = true; // ��ų A ��ٿ� ����
        }
    }

    void OnSkillB(InputValue value)
    {
        if (!isSkillBCooldown)
        {
            if (isAction) return;
            StartCoroutine(ActionTimer("SkillA_unlock 1", 2.2f));
            isSkillBCooldown = true;
        }
    }

   // public void onWeaponAttack()
    //{
       
   // }

    public void SkillClick()
    {
        /*Debug.Log("test");*/
        //onWeaponAttack();
        _animator.SetTrigger("onWeaponAttack");
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