using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CardType
{
    MapTile,
    Monster,
    Trap,
}

public class CardController : MonoBehaviour
{
    public CardType cardType;
    [SerializeField]
    private GameObject targetPrefab;

    private Button button;

    private GameObject instancedObject;

    private Collider targetCollider;

    private TileNode curNode = null;

    private TileNode availNodes;


    //1. �׷��Ƚ� ����ĳ���� ���
    //2. ���콺���� : ī�� Ȯ��(������ ����)
    //3. �巡�׽��� : ī�� ����
    //4. �巡���� : ���콺 ��ġ���� ������ ǥ��
    //5. �巡������ : Ÿ�ϳ���/���

    private void CardSelectCheck()
    {

    }

    private bool SetInput()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            return true;
        }

        return false;
    }

    private void Discard(bool value)
    {
        if(value)
        {
            //Ÿ�� ����
            instancedObject = null;
            GameManager.Instance.cardDeckController.hand_CardNumber--;

            GameManager.Instance.cardDeckController.cards.Remove(this.transform);
            GameManager.Instance.cardDeckController.SetCardPosition();

            Destroy(this.gameObject);
        }
        else
        {
            instancedObject.SetActive(false);
        }

        if(instancedObject != null)
        {
            Destroy(instancedObject.gameObject);
            instancedObject = null;
        }
        curNode = null;
    }

    private void SetTile()
    {
        Tile tile = instancedObject.GetComponent<Tile>();
        if (tile != null)
        {
            NodeManager.Instance.emptyNodes.Remove(curNode);
            NodeManager.Instance.activeNodes.Add(curNode);
            tile.MoveTile(curNode);
        }
    }

    private void SetMonster()
    {
        Monster monster = instancedObject.GetComponent<Monster>();
        if (monster != null)
        {
            monster.SetStartPoint(curNode);
            monster.transform.position = curNode.transform.position;
            monster.Init();
        }
    }

    private void SetTrap()
    {
        Trap trap = instancedObject.GetComponent<Trap>();
        if (trap != null)
        {
            trap.transform.SetParent(curNode.transform);
            trap.Init();
        }
    }

    private void SetObjectOnMap()
    {
        bool discard = false;
        if(curNode != null && curNode.setAvail)
        {
            switch (cardType)
            {
                case CardType.MapTile:
                    SetTile();
                    break;
                case CardType.Monster:
                    SetMonster();
                    break;
                case CardType.Trap:
                    SetTrap();
                    break;
            }
            discard = true;
        }

        Discard(discard);
        InputManager.Instance.settingCard = false;
        NodeManager.Instance.ResetAvail();
    }

    private void UpdateObjectPosition()
    {
        curNode = UtilHelper.RayCastTile();

        if(curNode != null && curNode.setAvail)
        {
            instancedObject.transform.position = curNode.transform.position;
        }
        else
        {
            instancedObject.transform.position = new Vector3(0, 10000, 0);
        }
    }

    private void UpdateObjectState()
    {
        UpdateObjectPosition();
        if(cardType == CardType.MapTile && Input.GetKeyDown(KeyCode.R))
        {
            Tile tile = instancedObject.GetComponent<Tile>();
            tile.RotateTile();
            ReadyForMapTile();
        }

        if (SetInput())
            SetObjectOnMap();
    }

    private void ReadyForTrap()
    {
        NodeManager.Instance.ResetAvail();
        UtilHelper.SetAvail(true, NodeManager.Instance.activeNodes);
    }

    private void ReadyForMapTile()
    {
        NodeManager.Instance.ResetAvail();
        Tile tile = instancedObject.GetComponent<Tile>();
        NodeManager.Instance.SetTileAvail(tile);
    }

    private void CallCard()
    {
        //targetPrefab����
        if(instancedObject == null)
            instancedObject = Instantiate(targetPrefab);
        else
            instancedObject.SetActive(true);
        targetCollider = instancedObject.GetComponentInChildren<Collider>();
        if(targetCollider != null ) { targetCollider.enabled = false; }
        InputManager.Instance.settingCard = true;
        //ī�� ������ ó��
        switch (cardType)
        {
            case CardType.MapTile:
                ReadyForMapTile();
                break;
            case CardType.Monster:
                ReadyForTrap();
                break;
            case CardType.Trap:
                ReadyForTrap();
                break;
        }
    }

    public void Init(CardType type, GameObject targetPrefab)
    {
        this.cardType = type;
        this.targetPrefab = targetPrefab;
        button = GetComponent<Button>();
        button.onClick.AddListener(CallCard);
    }

    // Update is called once per frame
    void Update()
    {
        if (instancedObject != null && instancedObject.gameObject.activeSelf)
            UpdateObjectState();
    }

    private void Start()
    {
        Init(cardType, targetPrefab);
    }
}
