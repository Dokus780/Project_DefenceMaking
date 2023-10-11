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

    private void SetRamdomTileToRandomNode(TileNode tileNode, Tile targetTilePrefab, int range)
    {
        //타겟 노드로부터 range거리내의 유효한 타일 중 랜덤한 한 타일에 targetTile을 비활성 상태로 설치
        //차후 문제 생길 시 랜덤리스트 작성 후 하나씩 빼도록 변경해야함
        int randomX = Random.Range(-range, range + 1);
        int randomY = Random.Range(-range, range + 1);
        TileNode targetNode = NodeManager.Instance.FindNode(tileNode.row + randomX, tileNode.col + randomY);
        if(targetNode == null || targetNode.curTile != null)
        {
            SetRamdomTileToRandomNode(tileNode, targetTilePrefab, range);
            return;
        }

        bool isTileAlone = true;
        foreach(TileNode checkNode in targetNode.neighborNodeDic.Values)
        {
            if(checkNode != null && checkNode.curTile != null)
            {
                isTileAlone = false;
                break;
            }
        }

        if(!isTileAlone)
        {
            SetRamdomTileToRandomNode(tileNode, targetTilePrefab, range);
            return;
        }

        int dist = PathFinder.Instance.GetNodeDistance(tileNode, targetNode);
        if(dist == -1 || dist > range)
        {
            SetRamdomTileToRandomNode(tileNode, targetTilePrefab, range);
            return;
        }

        Tile newTile = Instantiate(targetTilePrefab);
        newTile.transform.SetParent(tileNode.transform, false);
        newTile.Init(targetNode, true);
    }

    private void SetBasicTile()
    {
        //스타트포인트부터 스타트포인트 - 직선길 - 직선길  직선길 - 직선길 - 마왕방
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
    }

    public void SetRandomTile()
    {
        int index = DataManager.Instance.TileCard_Indexs[Random.Range(0, DataManager.Instance.TileCard_Indexs.Count)];
        string prefabName = DataManager.Instance.Deck_Table[index]["prefab"].ToString();
        Tile targetTilePrefab = Resources.Load<Tile>("Prefab/Tile/" + prefabName);
        SetRamdomTileToRandomNode(NodeManager.Instance.endPoint, targetTilePrefab, 4);
    }

    public void Init()
    {
        SetNewMap();
        NodeManager.Instance.SetEmptyNode();
        NodeManager.Instance.startPoint = NodeManager.Instance.FindNode(0, 0);
        SetBasicTile();

        for (int i = 0; i < 3; i++)
            SetRandomTile();

        InputManager.Instance.Call();
    }
}
