using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingHealthBar : MonoBehaviour
{
    Transform target;
    [SerializeField] private Slider slider;
    public void UpdateHealthBar(float currentValue, float maxValue)
    {
        slider.value = currentValue / maxValue;
    }
    public void SetTarget(Transform transform)
    {
        target = transform;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    /*
    private void LateUpdate()

    {
        if (target != null)
        {
            // ĳ������ ���� ��ǥ�� ������
            Vector3 worldPos = target.position;

            // ī�޶��� ���� ��ǥ�� �������� ĳ������ ��ũ�� ��ǥ�� ���
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            // ��ũ�� ��ǥ�� �������� HP ���� ��ġ�� ����
            transform.position = screenPos + Vector3.up * 100;

            // HP ���� �ʺ� ���� (�ɼ�)
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, rect.sizeDelta.y);
        }
    }*/
    private void LateUpdate()
    {
        if (target != null)
        {
            // ĳ������ ���� ��ǥ�� ������
            Vector3 worldPos = target.position;

            // ĳ������ ���� ��ǥ���� ������� ���̸� �����Ͽ� HP ���� ��ġ�� ���
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos + Vector3.up * 2f);

            // ��ũ�� ��ǥ�� �������� HP ���� ��ġ�� ����
            transform.position = screenPos + Vector3.up * 0;

            // HP ���� �ʺ� ���� (�ɼ�)
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, rect.sizeDelta.y);
        }
    }


}
