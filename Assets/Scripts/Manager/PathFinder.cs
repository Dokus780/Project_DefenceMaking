using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Node
{
    public TileNode tileNode;
    public float gCost;
    public float HCost;
    public float FCost;
    public TileNode prevNode;

    public Node(TileNode tileNode, TileNode prevNode, TileNode startTile, TileNode endTile, List<Node> closedNode)
    {
        this.tileNode = tileNode;
        gCost = PathFinder.Instance.GetGCost(prevNode, startTile, closedNode);
        HCost = PathFinder.Instance.GetHCost(tileNode.transform.position, endTile.transform.position);
        FCost = gCost + HCost;
        this.prevNode = prevNode;
    }
}

public class PathFinder : Singleton<PathFinder>
{
    
    public float GetHCost(Vector3 curPos, Vector3 endPos)
    {
        // �����Ÿ� = �����Ÿ�
        return (curPos - endPos).magnitude;
    }

    // ��ǥ�Ÿ� -> ������κ��� �����Ÿ��� ����尡 ���۳���϶����� ����
    public float GetGCost(TileNode prevTile, TileNode startTile, List<Node> closedNode)
    {
        float dist = 1;
        //����� prevTile�� ��� ã��, ����� prevTile�� startTile�̸� ��ȯ
        //dist = curPaths.Count - 1;
        if(prevTile == startTile)
            return dist;

        int errorStack = 0;
        while(prevTile != startTile)
        {
            foreach (Node node in closedNode)
            {
                if (node.tileNode == prevTile)
                {
                    dist++;
                    prevTile = node.prevNode;
                    break;
                }
            }

            errorStack++;
            if (errorStack > 10000)
                break;
        }

        return dist;
    }

    

    private bool IsInNode(TileNode targetTile, List<Node> targetNode)
    {
        foreach (Node node in targetNode)
        {
            if (node.tileNode == targetTile)
                return true;
        }

        return false;
    }

    private Node FindNode(TileNode targetTile, List<Node> targetNode)
    {
        foreach (Node node in targetNode)
        {
            if (node.tileNode == targetTile)
                return node;
        }

        return new Node();
    }

    private void CheckNode(TileNode curTile, TileNode startTile, TileNode endTile, List<Node> openNode, List<Node> closedNode, out List<Node> resultOpenNode)
    {
        List<TileNode> neighborTiles = UtilHelper.GetConnectedNodes(curTile);
        foreach(TileNode tile in neighborTiles)
        {
            if (tile == startTile || IsInNode(tile, closedNode))
                continue;

            //TileNode tempPrevTile = curPaths.;
            Node neighborNode = new Node(tile, curTile, startTile, endTile, closedNode);
            //Ÿ�Ͽ� �ش��ϴ� ��尡 ���³�忡 �̹� �ִ��� Ȯ��
            if (IsInNode(tile, openNode))
            {
                Node node = FindNode(tile, openNode);
                // �̹� ����Ʈ�� �ִٸ�, ���ο����� G�ڽ�Ʈ�� �� �۴ٸ� ��屳ü
                if (FindNode(curTile, openNode).gCost > neighborNode.gCost)
                {
                    openNode.Remove(node);
                    openNode.Add(neighborNode);
                }
            }
            else //���³�忡 ���ٸ� �߰�
                openNode.Add(neighborNode);
        }
        resultOpenNode = openNode;
    }

    public List<TileNode> CalculateFinalPath(TileNode startTile, TileNode endTile, List<Node> closedNode)
    {
        List<TileNode> finalPath = new List<TileNode>();
        finalPath.Add(endTile);

        TileNode prevNode = FindNode(endTile, closedNode).prevNode;
        while(prevNode != startTile)
        {
            finalPath.Add(prevNode);
            prevNode = FindNode(prevNode, closedNode).prevNode;
        }

        return finalPath;
    }

    public List<TileNode> FindPath(TileNode startTile, TileNode endTile = null)
    {
        if(endTile == null)
            endTile = NodeManager.Instance.endPoint;

        List<Node> openNode = new List<Node>();
        List<Node> closedNode = new List<Node>();
        TileNode curTile = startTile;
        while (curTile != endTile)
        {
            CheckNode(curTile, startTile, endTile, openNode, closedNode, out openNode);
            if (openNode.Count == 0)
            {
                return null; // �� ����
            }

            // ���³��� �� ���� F�ڽ�Ʈ�� ���� ��带 close��忡 �߰��ϰ�, open��忡�� ����
            float minFCost = openNode[0].FCost;
            Node minFCostNode = openNode[0];
            foreach (Node node in openNode)
            {
                if (node.FCost < minFCost)
                    minFCostNode = node;
            }
            closedNode.Add(minFCostNode);
            openNode.Remove(minFCostNode);
            curTile = minFCostNode.tileNode;
        }

        List<TileNode> finalNode = CalculateFinalPath(startTile, endTile, closedNode);
        finalNode.Reverse();
        return finalNode;
    }
}
