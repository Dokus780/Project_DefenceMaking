using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Adventurer : MonoBehaviour
{
    private List<TileNode> myPath = new List<TileNode>();
    private TileNode prevTile;
    private TileNode curTile;

    private TileNode lastCrossRoad;
    private List<TileNode> afterCrossPath;

    private TileNode FindNextNode(TileNode curNode)
    {
        //���� ��� ã�� ����
        TileNode nextNode = null;

        //�������ϰ�� ����

        return nextNode;
    }

    private IEnumerator Move(Vector3 nextPos, System.Action callback = null)
    {

        //���� ���� �� ��� ���

        yield return null;
        callback?.Invoke();
    }

    private IEnumerator MoveLogic()
    {
        curTile = NodeManager.Instance.startPoint;
        bool reverseLogic = false;
        while(true)
        {
            TileNode nextNode = FindNextNode(curTile);

            if(nextNode != null) // ���ٸ����� ���
            {
                //�����濡 �����Ҷ����� �ǵ��ư�
                reverseLogic = true;
                afterCrossPath.Reverse();
                for(int i = 0; i < afterCrossPath.Count; i++)
                {
                    nextNode = afterCrossPath[i];
                    yield return StartCoroutine(Move(nextNode.transform.position, () => { }));
                }
            }
            else
            {

            }

            yield return StartCoroutine(Move(nextNode.transform.position, () => { }));



            yield return null;
        }
    }

    public void Init()
    {
        
    }
}
