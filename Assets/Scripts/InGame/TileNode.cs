using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public enum Direction
{
    None,
    LeftUp,
    RightUp,
    Left,
    Right,
    LeftDown,
    RightDown,
}

public class TileNode : MonoBehaviour
{
    public Tile curTile;

    //public List<TileNode> neighborNodes = new List<TileNode>();
    public Dictionary<Direction, TileNode> neighborNodeDic = new Dictionary<Direction, TileNode>();

    [SerializeField]
    private Renderer guideObject;

    public bool setAvail = false;

    //public void SwapTile(TileNode prevNode, bool switchNode = false)
    //{
    //    Dictionary<Direction, TileNode> newDic = new Dictionary<Direction, TileNode>(prevNode.neighborNodeDic);
    //    if (switchNode)
    //        prevNode.SwapTile(this);
    //    neighborNodeDic = newDic;

    //    Direction closeDirection = GetNodeDirection(prevNode);
    //    neighborNodeDic[UtilHelper.ReverseDirection(closeDirection)] = prevNode;

    //    //��庰�� �̿�Ÿ�� ������
    //    DirectionalNode(Direction.Left)?.PushNeighborNode(UtilHelper.ReverseDirection(Direction.Left), this);
    //    DirectionalNode(Direction.LeftDown)?.PushNeighborNode(UtilHelper.ReverseDirection(Direction.LeftDown), this);
    //    DirectionalNode(Direction.LeftUp)?.PushNeighborNode(UtilHelper.ReverseDirection(Direction.LeftUp), this);
    //    DirectionalNode(Direction.Right)?.PushNeighborNode(UtilHelper.ReverseDirection(Direction.Right), this);
    //    DirectionalNode(Direction.RightDown)?.PushNeighborNode(UtilHelper.ReverseDirection(Direction.RightDown), this);
    //    DirectionalNode(Direction.RightUp)?.PushNeighborNode(UtilHelper.ReverseDirection(Direction.RightUp), this);
    //}

    public Direction GetNodeDirection(TileNode value)
    {
        foreach (var pair in neighborNodeDic)
        {
            if (EqualityComparer<TileNode>.Default.Equals(pair.Value, value))
            {
                return pair.Key;
            }
        }

        // ���� ���� ��� �⺻�� ��ȯ �Ǵ� ���� ó��
        return Direction.None;
    }

    public void SetAvail(bool value)
    {
        if (guideObject == null)
            return;

        guideObject.gameObject.SetActive(true);
        setAvail = value;

        if(value)
        {
            guideObject.material.SetColor("_FresnelColor", Color.green);
        }
        else
        {
            guideObject.material.SetColor("_FresnelColor", Color.red);
        }
    }



    public bool IsConnected(List<Direction> targetNode_PathDirection, List<Direction> targetNode_RoomDirection)
    {
        //���� ���� targetNode�� ���Ͽ� ����Ǿ��ִ��� Ȯ���ϴ� �Լ�
        bool isConnected = false;
        foreach (Direction direction in targetNode_PathDirection)
        {
            if (neighborNodeDic.ContainsKey(direction))
            {
                TileNode targetNode = neighborNodeDic[direction];
                if (!NodeManager.Instance.activeNodes.Contains(targetNode))
                    continue;

                foreach (Direction targetDirection in targetNode.curTile.PathDirection)
                {
                    if (direction == UtilHelper.ReverseDirection(targetDirection))
                        return true;
                }
            }
        }

        foreach (Direction direction in targetNode_RoomDirection)
        {
            if (neighborNodeDic.ContainsKey(direction))
            {
                TileNode targetNode = neighborNodeDic[direction];
                if (!NodeManager.Instance.activeNodes.Contains(targetNode))
                    continue;

                foreach (Direction targetDirection in targetNode.curTile.RoomDirection)
                {
                    if (direction == UtilHelper.ReverseDirection(targetDirection))
                        return true;
                }
            }
        }

        return isConnected;
    }

    //public List<TileNode> CalculatePathableNode()
    //{
    //    List<TileNode> pathableNodes = new List<TileNode>();
    //    foreach(Direction direction in pathDirection)
    //    {
    //        TileNode node = DirectionalNode(direction);
    //        if (node != null)
    //            pathableNodes.Add(node);
    //    }

    //    foreach (Direction direction in roomDirection)
    //    {
    //        TileNode node = DirectionalNode(direction);
    //        if (node != null)
    //            pathableNodes.Add(node);
    //    }

    //    return pathableNodes;
    //}
    
    public void PushNeighborNode(Direction direction, TileNode node)
    {
        if(node == null) return;

        if (neighborNodeDic.ContainsKey(direction))
            neighborNodeDic[direction] = node;
        else
            neighborNodeDic.Add(direction, node);
    }

    //originNode�κ��� ������鰣�� �̿������� �����ϴ� �Լ�
    public void SetNeighborNode (TileNode originNode, Direction direction)
    {
        switch (direction)
        {
            case Direction.Left:
                PushNeighborNode(Direction.RightUp, originNode.DirectionalNode(Direction.LeftUp));
                PushNeighborNode(Direction.RightDown, originNode.DirectionalNode(Direction.LeftDown));
                break;
            case Direction.LeftUp:
                PushNeighborNode(Direction.LeftDown, originNode.DirectionalNode(Direction.Left));
                PushNeighborNode(Direction.Right, originNode.DirectionalNode(Direction.RightUp));
                break;
            case Direction.LeftDown:
                PushNeighborNode(Direction.LeftUp, originNode.DirectionalNode(Direction.Left));
                PushNeighborNode(Direction.Right, originNode.DirectionalNode(Direction.RightDown));
                break;
            case Direction.Right:
                PushNeighborNode(Direction.LeftUp, originNode.DirectionalNode(Direction.RightUp));
                PushNeighborNode(Direction.LeftDown, originNode.DirectionalNode(Direction.RightDown));
                break;
            case Direction.RightUp:
                PushNeighborNode(Direction.Left, originNode.DirectionalNode(Direction.LeftUp));
                PushNeighborNode(Direction.RightDown, originNode.DirectionalNode(Direction.Right));
                break;
            case Direction.RightDown:
                PushNeighborNode(Direction.RightUp, originNode.DirectionalNode(Direction.Right));
                PushNeighborNode(Direction.Left, originNode.DirectionalNode(Direction.LeftDown));
                break;
        }

        PushNeighborNode(UtilHelper.ReverseDirection(direction), originNode);
    }


    public TileNode AddNode(Direction direction)
    {
        TileNode node = null;
        if (!neighborNodeDic.TryGetValue(direction, out node))
        {
            node = NodeManager.Instance.InstanceNewNode(this, direction);
            neighborNodeDic.Add(direction, node);
            NodeManager.Instance.allNodes.Add(node);
            
            return node;
        }
        return null;
    }

    public TileNode DirectionalNode(Direction direction)
    {
        if (neighborNodeDic.ContainsKey(direction))
            return neighborNodeDic[direction];

        return null;
    }

    public void Init()
    {
        
    }

}
