using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;

public class CardDeckController : MonoBehaviour
{
    [SerializeField]
    private Button deckDrawBtn;

    [SerializeField]
    private List<GameObject> deckPrefab;

    [SerializeField]
    private Transform cardZone;
    [SerializeField]
    private Transform cardZoom;

    private List<int> cardDeck;

    private bool initState = false;

    [SerializeField]
    private float minPos_X = 550;
    [SerializeField]
    private float maxPos_X = 100;
    [SerializeField]
    private float minPos_Y = 1370;
    [SerializeField]
    private float maxPos_Y = 1820;
    [SerializeField]
    private int pointPadding = 2;
    [SerializeField]
    private float handHeight = 1;

    public List<Transform> cards = new List<Transform>();

    [SerializeField]
    private UILineRenderer lineRenderer;
    [SerializeField]
    private int lineRendererPoint = 10;
    [SerializeField]
    private float lineAmplitude = 1f;

    private Coroutine set_CardPos_Coroutine = null;

    [SerializeField]
    private TextMeshProUGUI deckCountText;

    [SerializeField]
    private int cardPrice = 5;

    public void DeckSupply(CardType type)
    {
        List<int> cardPool = new List<int>();
        switch(type)
        {
            case CardType.MapTile:
                cardPool = DataManager.Instance.TileCard_Indexs;
                break;
            case CardType.Trap:
                cardPool = DataManager.Instance.TrapCard_Indexs;
                break;
            case CardType.Monster:
                cardPool = DataManager.Instance.MonsterCard_Indexs;
                break;
        }

        //���� ���Ŀ����� ���濹��
        int randomIndex = cardPool[UnityEngine.Random.Range(0, cardPool.Count)];
        cardDeck.Add(randomIndex);
        UpdateDeckCount();
    }

    private void UpdateDeckCount()
    {
        if (deckCountText == null)
            return;

        deckCountText.text = (cardDeck.Count).ToString();
        if (cardDeck.Count < 10)
            deckCountText.color = Color.red;
        else
            deckCountText.color = Color.black;
    }

    public void DrawGuide(Vector3 cardPos, bool value)
    {
        lineRenderer.gameObject.SetActive(value);
        if(value)
        {
            List<Vector2> lines = GetLine(cardPos, Input.mousePosition);
            lineRenderer.Points = lines.ToArray();
        }
    }

    private List<Vector2> GetLine(Vector3 startPos, Vector3 endPos)
    {
        int points = lineRendererPoint;
        List<Vector2> linePosition = new List<Vector2>();

        Vector2 rotDir = startPos - endPos;
        float amplitude = lineAmplitude;
        // rotDir�� ���̿� ���� amplitude �����ϴ� �ڵ� �ʿ�
        float minAmp = 0;
        float maxAmp = 300;
        float minDist = 0;
        float maxDist = 2000;
        float distance = rotDir.magnitude;
        float normalizedDist = Mathf.InverseLerp(minDist, maxDist, distance);
        // ����ȭ�� �Ÿ� ���� �ּ������� �ִ����� ������ ������ ����
        amplitude = Mathf.Lerp(minAmp, maxAmp, normalizedDist);
        // �ּ����� ~ �ִ�����
        // �ּҰŸ� ~ �ִ�Ÿ�
        float invert = 1f;
        if (endPos.x > startPos.x)
            invert *= -1f;

        for (int i = 0; i < points; ++i)
        {
            // i�� 0.0-1.0 ������ ������ ����ȭ
            float t = (float)i / (points - 1);

            Vector2 point = Vector2.Lerp(startPos, endPos, t);

            //modifyValue��ŭ point�� ��ǥ���� ����
            Vector2 modifyValue = new Vector2(0, amplitude * Mathf.Sin(2 * Mathf.PI * t * 0.5f) * invert);
            //modifyValue�� rotDir��ŭ ȸ��
            float angle = Mathf.Deg2Rad * Vector2.SignedAngle(Vector2.right, rotDir.normalized);

            Vector2 rotatedModifyValue = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * angle) * modifyValue;

            point.x += rotatedModifyValue.x;
            point.y += rotatedModifyValue.y;

            linePosition.Add(point);
        }
        return linePosition;
    }

    public List<Vector3> CalculateRotation()
    {
        //ȸ�������
        //������Ʈ -> ��������Ʈ�� ���⺤�� / 2�� ������Ʈ�� ���� ��ġ
        //�� ��ġ -> ����ġ�� ���� ���⺤�Ϳ� �� transform.up�� �����ϰ� �ϵ��� roation ����
        int points = pointPadding + hand_CardNumber + pointPadding;
        List<Vector3> cardPos = new List<Vector3>();
        List<Vector3> cardDirVecs = new List<Vector3>();
        for (int i = pointPadding - 1; i < points - pointPadding + 1; ++i)
        {
            float t = (float)i / (points - 1);
            float x = Mathf.Lerp(ReturnStartX(), ReturnStartY(), t);
            float y = handHeight * Mathf.Sin(2 * Mathf.PI * t * 0.5f);
            cardPos.Add(new Vector3(x, y));
        }

        for(int i = 1; i < cardPos.Count - 1; i++)
        {
            Vector3 prevPos = cardPos[i - 1] * 1000;
            Vector3 nextPos = cardPos[i + 1] * 1000;
            Vector3 dirVec = (nextPos - prevPos) / 2 * 1000;
            cardDirVecs.Add((cardPos[i] - (prevPos + dirVec)).normalized);
        }

        return cardDirVecs;
    }

    public float ReturnStartY()
    {
        if (hand_CardNumber >= 0 && hand_CardNumber <= 10)
        {
            // ī�� ������ 0���� 10 ������ ��
            float startX = Mathf.Lerp(minPos_Y, maxPos_Y, hand_CardNumber / 10f);
            return startX;
        }
        else
        {
            // ī�� ������ ������ ��� �� �⺻���� 100�� ��ȯ�ϰų� ���� ó���� �����մϴ�.
            return 450f;
        }
    }

    public float ReturnStartX()
    {
        //���� ī�� ������ 0���� 10�ӿ� ���� 100 ~ 450���� ��ȯ
        if (hand_CardNumber >= 0 && hand_CardNumber <= 10)
        {
            // ī�� ������ 0���� 10 ������ ��
            float startX = Mathf.Lerp(minPos_X, maxPos_X, hand_CardNumber / 10f);
            return startX;
        }
        else
        {
            // ī�� ������ ������ ��� �� �⺻���� 100�� ��ȯ�ϰų� ���� ó���� �����մϴ�.
            return minPos_X;
        }
    }

    public void SetCardPosition()
    {
        if (set_CardPos_Coroutine != null)
            StopCoroutine(set_CardPos_Coroutine);
        set_CardPos_Coroutine = StartCoroutine(ISetCardPosition());
    }

    private List<Vector3> GetCardPosition()
    {
        int points = pointPadding + hand_CardNumber + pointPadding;
        List<Vector3> cardPos = new List<Vector3>();
        for (int i = pointPadding; i < points - pointPadding; ++i)
        {
            // i�� 0.0-1.0 ������ ������ ����ȭ
            float t = (float)i / (points - 1);
            // start���� end ��ġ���� points ������ ���� �����ϰ� ��ġ
            float x = Mathf.Lerp(ReturnStartX(), ReturnStartY(), t);
            // 2*Mathf.PI = 360�̰�, t�� 0.0~1.0 ������ ���̱� ������ �� ���� ���ϸ� 1 ������ ���� �׷����� �ϼ��ǰ�,
            // frequency�� ���ϱ� ������ frequency ���� ���� �������� �����ȴ�.
            float y = handHeight * Mathf.Sin(2 * Mathf.PI * t * 0.5f);
            cardPos.Add(new Vector3(x, y));
        }
        return cardPos;
    }

    //�ʱ� ī�� ����
    public int hand_CardNumber = 0;
    //�ִ� ī�� ����
    public int maxCardNumber = 10;

    private string ReturnDeck()
    {
        int random = cardDeck[UnityEngine.Random.Range(0, cardDeck.Count)];
        cardDeck.Remove(random);
        string cardPrefabPath = DataManager.Instance.Deck_Table[random]["prefab"].ToString();
        return "Prefab/Card/" + cardPrefabPath;
    }

    public void DrawDeck()
    {
        if (GameManager.Instance.gold < cardPrice || hand_CardNumber >= maxCardNumber)
            return;

        GameManager.Instance.gold -= cardPrice;
        DrawCard();
        AudioManager.Instance.Play2DSound("Click_card", SettingManager.Instance.fxVolume);
    }

    private IEnumerator ISetCardPosition()
    {
        //��� ī����� ��ġ�� lerpTime�� ���� ����
        //��� ī����� ȸ������ lerpTime�� ���� ����

        float elapsedTime = 0f;
        float lerpTime = 0.5f;
        List<Vector3> cardPos = GetCardPosition();
        List<Vector3> cardRot = CalculateRotation();
        Vector3[] startPositions = new Vector3[cards.Count];
        Quaternion[] startRotations = new Quaternion[cards.Count];
        for (int i = 0; i < cards.Count; i++)
        {
            startPositions[i] = cards[i].position;
            startRotations[i] = cards[i].rotation;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            CardController temp = cards[i].GetComponent<CardController>();
            temp.originPos = cardPos[i];
            temp.originRot = UtilHelper.AlignUpWithVector(cardRot[i]);
        }

        while (elapsedTime < lerpTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / lerpTime);
            t = Mathf.Sin(t * Mathf.PI * 0.5f);
            for (int i = 0; i < hand_CardNumber; i++)
                cards[i].transform.position = Vector3.Lerp(startPositions[i], cardPos[i], t);

            for (int i = 0; i < cards.Count; i++)
            {
                Vector3 targetDirection = cardRot[i];
                Quaternion targetRotation = UtilHelper.AlignUpWithVector(targetDirection);

                Quaternion currentRotation = Quaternion.Lerp(startRotations[i], targetRotation, t);
                cards[i].transform.rotation = currentRotation;
            }

            yield return null;
        }
    }

    public void DrawCard()
    {
        if(cardDeck.Count < 1) return;

        string targtPrerfab = ReturnDeck();
        hand_CardNumber++;
        GameObject targetPrefab = Resources.Load<GameObject>(targtPrerfab);
        GameObject temp = Instantiate(targetPrefab, cardZone);
        CardController card = temp.GetComponent<CardController>();
        card?.DrawEffect();
        temp.transform.position = transform.position;
        cards.Add(temp.transform);
        SetCardPosition();

        UpdateDeckCount();
    }

    private void SetDeck()
    {
        cardDeck = new List<int>();
        for(int i = 0; i < DataManager.Instance.Deck_Table.Count; i++)
        {
            int cardNumber = Convert.ToInt32(DataManager.Instance.Deck_Table[i]["startNumber"]);
            for (int j = 0; j < cardNumber; j++)
                cardDeck.Add(Convert.ToInt32(DataManager.Instance.Deck_Table[i]["index"]));
        }
    }

    private void Init()
    {
        if (initState) return;

        deckDrawBtn.onClick.AddListener(DrawDeck);
        SetDeck();
        initState = true;
    }

    // Start is called before the first frame update
    void Awake()
    {
        Init();
    }
}
