using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    private Battler battler;
    [SerializeField]
    private Image hp_Bar;

    public float hp_Rate;

    public float delayTime = 1f;
    public float lerpTime = 0.2f;

    private Coroutine curCoroutine = null;

    private void HPBarEnd()
    {
        gameObject.SetActive(false);
    }

    public void Init(Battler battler)
    {
        this.battler = battler;
    }


    private void UpdatePosition()
    {
        if (battler == null)
            return;

        // controller�� ��ġ�� ���� ��ǥ�迡�� ��ũ�� ��ǥ��� ��ȯ
        Vector3 screenPos = Camera.main.WorldToScreenPoint(battler.transform.position);

        // ��ũ�� ��ǥ�迡�� RectTransform ��ǥ��� ��ȯ�Ͽ� ��ġ �̵�
        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GameManager.Instance.cameraCanvas.transform as RectTransform, screenPos, Camera.main, out anchoredPos);
        anchoredPos.y += 150;
        RectTransform rect = transform.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPos;
    }

    private void UpdateHp()
    {
        // ���� ü�¹ٴ� �ٷ� ������Ʈ
        float nextAmount = battler.curHp / battler.maxHp;
        hp_Bar.fillAmount = nextAmount;
    }

    // Update is called once per frame
    void Update()
    {
        if (battler == null)
            return;

        UpdateHp();
        UpdatePosition();
    }
}
