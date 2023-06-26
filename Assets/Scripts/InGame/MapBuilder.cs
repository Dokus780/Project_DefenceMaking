using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;




public class MapBuilder : MonoBehaviour
{
    public TileNode curNode;

    public int x_Size = 10;
    public int y_Size = 9;

    public int startPathSize = 4;

    [SerializeField]
    private int emptyNodeSize = 4;

    private void SetBasicTile()
    {
        //��ŸƮ����Ʈ���� ��ŸƮ����Ʈ - ������ - ������  ������ - ������ - ���չ�
        //print(NodeManager.Instance.startPoint.transform.position);
        GameObject startPointPrefab = Resources.Load<GameObject>("Prefab/Tile/StartTile");
        GameObject pathPrefab = Resources.Load<GameObject>("Prefab/Tile/RoadTile0");
        GameObject endPointPrefab = Resources.Load<GameObject>("Prefab/Tile/EndTile");

        Tile startTile = Instantiate(startPointPrefab)?.GetComponent<Tile>();
        startTile.MoveTile(NodeManager.Instance.startPoint);

        NodeManager.Instance.startPoint = startTile.curNode;
        NodeManager.Instance.activeNodes = new List<TileNode>();

        NodeManager.Instance.activeNodes.Add(startTile.curNode);
        NodeManager.Instance.ExpandEmptyNode(startTile.curNode, emptyNodeSize);

        TileNode nextNode = startTile.curNode.neighborNodeDic[Direction.Right];
        NodeManager.Instance.ExpandEmptyNode(nextNode, emptyNodeSize);
        

        for (int i = 0; i < startPathSize; i++)
        {
            Tile pathTile = Instantiate(pathPrefab)?.GetComponent<Tile>();
            pathTile.MoveTile(nextNode);

            NodeManager.Instance.activeNodes.Add(pathTile.curNode);
            nextNode = nextNode.neighborNodeDic[Direction.Right];
            NodeManager.Instance.ExpandEmptyNode(nextNode, emptyNodeSize);
        }

        Tile endTile = Instantiate(endPointPrefab)?.GetComponent<Tile>();
        endTile.MoveTile(nextNode);

        NodeManager.Instance.activeNodes.Add(endTile.curNode);
        NodeManager.Instance.endPoint = endTile.curNode;
        endTile.movable = true;

        PlayerBattleMain king = endTile.GetComponentInChildren<PlayerBattleMain>();
        king.Init();
        GameManager.Instance.king = king;
    }

    private bool IsStartPointValid(TileNode node)
    {
        TileNode traceNode = node;
        for(int i = 0; i < startPathSize + 1; i++)
        {
            if (traceNode.neighborNodeDic.ContainsKey(Direction.Right))
                traceNode = traceNode.neighborNodeDic[Direction.Right];
            else
                return false;
        }

        return true;
    }

    private void SetStartPoint()
    {
        NodeManager.Instance.startPoint = NodeManager.Instance.FindNode(0, 0);
    }

    private void SetNewMap()
    {
        NodeManager.Instance.allNodes.Add(curNode);
        for (int i = 0; i < emptyNodeSize; i++)
        {
            NodeManager.Instance.BuildNewNodes();
        }


        

        //1.���ʳ�� ����
        //2.���ʳ�忡�� ����Ÿ�� + ��ȯȽ���� Ÿ�� �ҷ�����
        //3.�ش��忡�� SetNewNode
        //4.�ش��忡�� ��������Ÿ���� ��ȯȽ����ŭ �̵��ϸ� SetNewNode
        //5.������ ������, �����ʾƷ�, ���ʾƷ�, ���ʼ����� �̵��ϸ� SetNewNode
        //6.

        //1. ���ʳ���� Left���� SetNewNode
        //RightUp, Right, RightDown, LeftDown, Left, LeftUp
        //Direction[] directions = new Direction[]
        //{ Direction.RightUp, Direction.Right, Direction.RightDown, Direction.LeftDown, Direction.Left, Direction.LeftUp};
        
    }

    public void Init()
    {
        SetNewMap();
        NodeManager.Instance.SetEmptyNode();
        NodeManager.Instance.startPoint = NodeManager.Instance.FindNode(0, 0);
        SetBasicTile();

        InputManager.Instance.Call();
        NodeManager.Instance.endPointPosition = NodeManager.Instance.endPoint.transform.position;
    }
}
