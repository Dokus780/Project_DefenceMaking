using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    //��Ȱ��ȭ ������ ���
    public List<TileNode> virtualNodes;
    //Ȱ��ȭ ������ ���
    public List<TileNode> activeNodes;

    //������κ��� 6�� ������(isActive = false) ����
    //���λ����� �������� �̿���� ����
    //��Ÿ���� �����忡 ��ġ
    //��ġ�� ������� Ȱ������ �̵�
    //���� ��ġ�� ���κ��� �������������ʴ� ����� ������ ����

    public TileNode startPoint;

    

    private void SetNewNode(TileNode curNode)
    {
        curNode.AddNode(Direction.Left);
        curNode.AddNode(Direction.LeftUp);
        curNode.AddNode(Direction.LeftDown);
        curNode.AddNode(Direction.Right);
        curNode.AddNode(Direction.RightUp);
        curNode.AddNode(Direction.RightDown);
    }

    public bool IsNodeInstance()
    {

        return false;
    }
}
