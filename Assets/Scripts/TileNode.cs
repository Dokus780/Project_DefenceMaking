using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    LeftUp,
    RightUp,
    Left,
    Right,
    LeftDown,
    RightDown,
}

public class TileNode : MonoBehaviour
{
    public List<Direction> pathDirection = new List<Direction>();
    public List<Direction> roomDirection = new List<Direction>();

    public List<TileNode> neighborNodes = new List<TileNode>();
    public Dictionary<Direction, TileNode> neighborNodeDic = new Dictionary<Direction, TileNode>();

    public List<TileNode> pathableNodes = new List<TileNode>();


    public bool isActive = false;

    public void CalculatePathableNode()
    {
        pathableNodes = new List<TileNode>();
        foreach(Direction direction in pathDirection)
        {
            TileNode node = DirectionalNode(direction);
            if (node != null)
                pathableNodes.Add(node);
        }

        foreach (Direction direction in roomDirection)
        {
            TileNode node = DirectionalNode(direction);
            if (node != null)
                pathableNodes.Add(node);
        }
    }

    private TileNode InstanceNewNode(TileNode node, Direction direction)
    {
        //Instantiate�ϴ� �Լ� �߰�����
        TileNode newNode = null;

        //newNode�� ������(isActive = false)
        //newNode�� direction���⿡ �����带 �̿����� �߰�
        newNode.neighborNodeDic.Add(UtilHelper.ReverseDirection(direction), node); 
        return newNode;
    }

    public void AddNode(Direction direction)
    {
        TileNode node = DirectionalNode(direction);
        if (node == null)
        {
            node = InstanceNewNode(this, direction);
            neighborNodes.Add(node);
            neighborNodeDic.Add(direction, node);
        }
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
