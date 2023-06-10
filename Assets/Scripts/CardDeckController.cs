using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDeckController : MonoBehaviour
{
    [SerializeField]
    private Button deckDrawBtn;

    [SerializeField]
    private List<GameObject> deckPrefab;

    [SerializeField]
    private Transform cardZone;


    //�ʱ� ī�� ����
    public int hand_CardNumber = 0;
    //�ִ� ī�� ����
    public int maxCardNumber = 10;

    private int ReturnDeck()
    {
        // ���� ����Ȳ�� ���� ������ index�� ��ȯ�ϴ� �Լ� �߰�����
        int random = Random.Range(0, deckPrefab.Count);

        return random;
    }

    public void DrawDeck()
    {
        if (GameManager.Instance.gold < 200 || hand_CardNumber >= maxCardNumber)
            return;

        int targetPrefabNumer = ReturnDeck();
        hand_CardNumber++;
        GameManager.Instance.gold -= 200;

        GameObject temp = Instantiate(deckPrefab[targetPrefabNumer], cardZone);
        temp.transform.position = cardZone.transform.position;
    }

    public void FreeDrawCard()
    {
        int targetPrefabNumer = ReturnDeck();
        hand_CardNumber++;

        GameObject temp = Instantiate(deckPrefab[targetPrefabNumer], cardZone);
        temp.transform.position = cardZone.transform.position;
    }

    // Start is called before the first frame update
    void Awake()
    {
        deckDrawBtn.onClick.AddListener(DrawDeck);
    }
}
