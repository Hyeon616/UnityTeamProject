using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class TutorialKeyController : MonoBehaviour
{
    public Image AKey;       // A Ű �̹���
    public Image SKey;       // S Ű �̹���
    public Image DKey;       // D Ű �̹���
    public Image WKey;       // W Ű �̹���
    public Image ShiftKey;   // Shift Ű �̹���
    public Image MouseLeft;  // ���콺 ���� ��ư �̹���
    public Image Key1;       // 1�� Ű �̹���
    public Image Key2;       // 2�� Ű �̹���

    private Dictionary<KeyCode, Image> keyImageMapping; // Ű�� �̹��� ����

    void Start()
    {
        // Ű�� �̹��� ���� �ʱ�ȭ
        keyImageMapping = new Dictionary<KeyCode, Image>
        {
            { KeyCode.A, AKey },
            { KeyCode.S, SKey },
            { KeyCode.D, DKey },
            { KeyCode.W, WKey },
            { KeyCode.LeftShift, ShiftKey },
            { KeyCode.Alpha1, Key1 },
            { KeyCode.Alpha2, Key2 }
        };
    }

    void Update()
    {
        // �� Ű�� ���� �Է� ���� Ȯ�� �� ���� ����
        foreach (var keyImagePair in keyImageMapping)
        {
            UpdateKeyColor(keyImagePair.Key, keyImagePair.Value);
        }

        // ���콺 ���� ��ư �Է� ���� Ȯ�� �� ���� ����
        UpdateMouseColor(0, MouseLeft);
    }

    // Ű �Է� ���¿� ���� �̹��� ���� ����
    private void UpdateKeyColor(KeyCode key, Image image)
    {
        if (Input.GetKeyDown(key)) image.color = Color.red;
        if (Input.GetKeyUp(key)) image.color = Color.white;
    }

    // ���콺 �Է� ���¿� ���� �̹��� ���� ����
    private void UpdateMouseColor(int button, Image image)
    {
        if (Input.GetMouseButtonDown(button)) image.color = Color.red;
        if (Input.GetMouseButtonUp(button)) image.color = Color.white;
    }
}
