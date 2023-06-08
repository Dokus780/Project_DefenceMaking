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

    public Vector3 GetNewNodePosition(TileNode curNode, Direction direction, float distance)
    {
        float angle = 0f;
        switch (direction)
        {
            case Direction.LeftUp:
                angle = 120f;
                break;
            case Direction.RightUp:
                angle = 60f;
                break;
            case Direction.Left:
                angle = 180f;
                break;
            case Direction.LeftDown:
                angle = -120f;
                break;
            case Direction.RightDown:
                angle = -60f;
                break;
        }

        float angleInRadians = angle * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(angleInRadians), 0, Mathf.Sin(angleInRadians));
        dir *= distance;
        return curNode.transform.position + dir;
    }

    private void SetNewNode(TileNode curNode)
    {

    }

    public bool IsNodeInstance()
    {

        return false;
    }
}
