using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class MonsterHit : MonoBehaviour
{
    [Header("Hit Effect Settings")]
    private float flashDuration = 0.2f;
    private Material originalMaterial;
    private Material flashMaterial;

    [Header("Damage Text Settings")]
    private float textDuration = 1.0f;
    private float floatSpeed = 100f;
    private Vector3 textOffset = new Vector3(0, 0.2f, 0);


    private SkinnedMeshRenderer meshRenderer;
    private Canvas damageCanvas;
    private GameObject damageTextPrefab;

    private void Start()
    {
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        originalMaterial = meshRenderer.material;

        flashMaterial = new Material(Shader.Find("Standard"));
        flashMaterial.color = Color.white;
        flashMaterial.EnableKeyword("_EMISSION");
        flashMaterial.SetColor("_EmissionColor", Color.white);

        damageCanvas = GameObject.Find("DamageCanvas").GetComponent<Canvas>();

        damageTextPrefab = Resources.Load<GameObject>("Monsters/DamageTextPrefab");

    }

    public void OnHit()
    {
        StartCoroutine(FlashEffect());
    }

    private IEnumerator FlashEffect()
    {
        meshRenderer.material = flashMaterial;
        yield return new WaitForSeconds(flashDuration);
        meshRenderer.material = originalMaterial;
    }

    public void ShowDamageText(int damage)
    {
        // 몬스터의 위치에서 약간 위로 올린 지점을 기준으로 함
        Vector3 textPosition = transform.position + textOffset;

        // 월드 좌표를 스크린 좌표로 변환
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(textPosition);

        // 데미지 텍스트 생성
        GameObject textObj = Instantiate(damageTextPrefab, damageCanvas.transform);

        // 스크린 좌표를 캔버스 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            damageCanvas.GetComponent<RectTransform>(),
            screenPoint,
            null,
            out Vector2 localPoint
        );

        // 위치 설정
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = localPoint;

        // 텍스트 컴포넌트 설정
        TextMeshProUGUI tmpText = textObj.GetComponent<TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = damage.ToString();
            StartCoroutine(AnimateDamageText(textObj));
        }
    }

    private IEnumerator AnimateDamageText(GameObject textObj)
    {
        TextMeshProUGUI tmpText = textObj.GetComponent<TextMeshProUGUI>();
        RectTransform rectTransform = textObj.GetComponent<RectTransform>();
        float elapsedTime = 0f;
        Vector2 startPos = rectTransform.anchoredPosition;
        Color startColor = tmpText.color;

        while (elapsedTime < textDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / textDuration;

            // 위로 올라가는 애니메이션
            rectTransform.anchoredPosition = startPos + Vector2.up * (floatSpeed * normalizedTime);

            // 페이드 아웃
            Color newColor = startColor;
            newColor.a = 1 - normalizedTime;
            tmpText.color = newColor;

            yield return null;
        }

        Destroy(textObj);
    }

}
