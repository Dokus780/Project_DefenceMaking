using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopUIControl : MonoBehaviour
{
    [SerializeField]
    RectTransform rect;
    [SerializeField]
    RectTransform contentRect;
    [SerializeField]
    RectTransform popUpRect;
    [SerializeField]
    ScrollRect scrollRect;
    [SerializeField]
    Transform guideTransform;

    public void Set()
    {
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.sizeDelta.x - 10f);
        contentRect.anchoredPosition = contentRect.anchoredPosition + new Vector2(-10, 0);

        //1. rect�� popUprect�� ���̸�ŭ ����
        //2. contentRect�� guideTransform.Position ���� �ð��� ���� �̵�
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
            Set();
    }
}
