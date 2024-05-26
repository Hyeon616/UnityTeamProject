using System.Collections;
using UnityEngine;

public class OpenPortal : MonoBehaviour
{
    [SerializeField] private Transform pointA; // ���� ����
    [SerializeField] private Transform pointB; // ���� ����
    [SerializeField] private float duration = 8.0f; // �̵��� �ɸ��� �ð�

    [SerializeField]
    private GameObject boss;

    void Start()
    {
        if (boss.GetComponent<Boss>().currentHealth <= 0)
        {
            StartCoroutine(MoveFromAToB(pointA.position, pointB.position, duration));

        }
        // ������Ʈ�� �̵���Ű�� �ڷ�ƾ�� �����մϴ�.
    }

    private IEnumerator MoveFromAToB(Vector3 start, Vector3 end, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // ��� �ð��� ������ŵ�ϴ�.
            elapsed += Time.deltaTime;

            // ������Ʈ�� A���� B�� �����մϴ�.
            transform.position = Vector3.Lerp(start, end, elapsed / duration);

            // ���� �����ӱ��� ��ٸ��ϴ�.
            yield return null;
        }

        // �̵��� ������ �� ������Ʈ�� ��Ȯ�� ���� ������ ����ϴ�.
        transform.position = end;
    }
}
