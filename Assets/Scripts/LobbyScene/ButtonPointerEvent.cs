//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using TMPro;
//using UnityEngine.UI;

//public class ButtonPointerEvent : MonoBehaviour
//{
//    public TextMeshProUGUI text;
//    public Transform transform;
//    public Image image;
//    public GameObject select;
//    Vector3 defaulttransform;
//    public void Start()
//    {
//        defaulttransform = transform.localScale;
//    }


//    public void OnPointerEnter()
//    {
//        transform.localScale = defaulttransform*1.2f;
//        text.color = Color.black;
//    }
//    public void OnPointerExit()
//    {

//        transform.localScale = defaulttransform;
//        text.color = Color.white;
//    }
//    public void OnPointerDown()
//    {
//        Debug.Log("����");
//        transform.localScale = defaulttransform;
//        text.color = Color.white;
//    }
//    public void OnPointerUp()
//    {
//        Debug.Log("��");
//    }
//    public void OnPointerDownUpgrade()
//    {
//        image.color = Color.red;
//    }
//    public void OnPointerUpUpgrade()
//    {
//        image.color = Color.white;
//    }
//    public void OnpointerEnterStage()
//    {
//        transform.localScale = defaulttransform * 1.2f;
//        select.SetActive(true);

//    }
//    public void OnpointerExitStage()
//    {
//        transform.localScale = defaulttransform;
//        select.SetActive(false);

//    }
//}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ButtonPointerEvent : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Image image;
    public GameObject select;
    private Vector3 defaultScale; // �ʱ� localScale ���� �����ϴ� private ����

    void Start()
    {
        defaultScale = transform.localScale; // �ʱ� localScale ���� ����
    }

    public void OnPointerEnter()
    {
        ChangeScaleAndColor(1.2f, Color.black); // ũ��� ������ �����ϴ� �޼��� ȣ��
    }

    public void OnPointerExit()
    {
        ChangeScaleAndColor(1.0f, Color.white); // ���� ũ��� �������� �ǵ����� �޼��� ȣ��
    }

    public void OnPointerDown()
    {
        Debug.Log("����"); // Ŭ�� �� �α� ���
        ResetScaleAndColor(); // ũ��� ������ �ʱ� ���·� �ǵ����� �޼��� ȣ��
    }

    public void OnPointerUp()
    {
        Debug.Log("��"); // Ŭ�� ���� �� �α� ���
    }

    public void OnPointerDownUpgrade()
    {
        ChangeImageColor(Color.red); // �̹��� ������ ���������� �����ϴ� �޼��� ȣ��
    }

    public void OnPointerUpUpgrade()
    {
        ChangeImageColor(Color.white); // �̹��� ������ ������� �����ϴ� �޼��� ȣ��
    }

    public void OnPointerEnterStage()
    {
        ChangeScale(1.2f); // ũ�⸦ �����ϴ� �޼��� ȣ��
        select.SetActive(true); // ���� ������Ʈ Ȱ��ȭ
    }

    public void OnPointerExitStage()
    {
        ChangeScale(1.0f); // ���� ũ��� �ǵ����� �޼��� ȣ��
        select.SetActive(false); // ���� ������Ʈ ��Ȱ��ȭ
    }

    // ũ��� �ؽ�Ʈ ������ �����ϴ� �޼��� �߰�
    private void ChangeScaleAndColor(float scaleMultiplier, Color textColor)
    {
        transform.localScale = defaultScale * scaleMultiplier; // ũ�� ����
        text.color = textColor; // �ؽ�Ʈ ���� ����
    }

    // ũ��� �ؽ�Ʈ ������ �ʱ� ���·� �ǵ����� �޼��� �߰�
    private void ResetScaleAndColor()
    {
        transform.localScale = defaultScale; // ũ�� �ʱ�ȭ
        text.color = Color.white; // �ؽ�Ʈ ���� �ʱ�ȭ
    }

    // ũ�⸸ �����ϴ� �޼��� �߰�
    private void ChangeScale(float scaleMultiplier)
    {
        transform.localScale = defaultScale * scaleMultiplier; // ũ�� ����
    }

    // �̹��� ���� �����ϴ� �޼��� �߰�
    private void ChangeImageColor(Color color)
    {
        image.color = color; // �̹��� ���� ����
    }
}
