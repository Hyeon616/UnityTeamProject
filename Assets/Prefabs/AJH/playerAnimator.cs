using UnityEngine;
using UnityEngine.InputSystem;

public class playerAnimator : MonoBehaviour
{
    private Animator _animator;
    private Vector3 moveDirection;
    private bool isRunning = false; // �ٱ� ���¸� �����ϴ� ����


    float attackTime = 0;

    void Start()
    {
        _animator = this.GetComponent<Animator>();
    }

    void Update()
    {


        bool hasControl = (moveDirection != Vector3.zero);
        if (hasControl)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
            transform.Translate(Vector3.forward * Time.deltaTime * 2f); // �ٱ� ������ ���� �� ������ �̵�
            _animator.SetBool("isRunning", isRunning);
        }
        else
        {
            _animator.SetBool("isRunning", false); // �̵����� ���� ���� �ٱ� ���� ����
        }


    }


    #region SEND_MESSAGE
    void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();                 // �Է� ���� ���� ��������
        if (input != null)
        {
            moveDirection = new Vector3(input.x, 0f, input.y);

            // �̵� �Է��� ���� ���� �ٱ� ���·� ����
            isRunning = input.magnitude > 0;
        }
    }
    public void onWeaponAttack()
    {
        Debug.Log("ddd");
        _animator.SetTrigger("onWeaponAttack");
    }

    void OnClick()
    {

        onWeaponAttack();
    }
    #endregion
}