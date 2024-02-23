using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HerbUpdater1 : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI targetText;

    [SerializeField]
    private Image herbBoxFill;
    [SerializeField]
    private GameObject herbMaxImg;

    private void SetHerbBox(int herbCount, int herbMax)
    {
        if (herbBoxFill == null || herbMaxImg == null)
            return;

        herbMaxImg.SetActive(false);
        if (herbMax == 0 && herbCount > 0)
            herbMaxImg.SetActive(true);
        else if (herbMax > 0 && herbCount > herbMax)
            herbMaxImg.SetActive(true);
        else if (herbMax > 0)
            herbBoxFill.fillAmount = (float)herbCount / (float)herbMax;
        else if (herbMax == 0 && herbCount == 0)
            herbBoxFill.fillAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        int herbCount = GameManager.Instance.herb1;
        targetText.text = herbCount.ToString();
        int herbMax = GameManager.Instance.herb1Max;
        SetHerbBox(herbCount, herbMax);
    }
}
