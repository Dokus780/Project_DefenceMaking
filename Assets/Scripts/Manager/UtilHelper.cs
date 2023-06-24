using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

public static class UtilHelper
{
    public static List<TKey> GetKeyValues<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    {
        List<TKey> keyValues = new List<TKey>(dictionary.Keys);
        return keyValues;
    }

    public static bool IsTileConnected(TileNode curNode, List<Direction> curTile_Direction, List<Direction> targetTile_Direction)
    {
        foreach (Direction direction in targetTile_Direction)
        {
            if (curNode.neighborNodeDic.ContainsKey(direction))
            {
                Tile targetTile = curNode.neighborNodeDic[direction].curTile;
                if (targetTile == null)
                    continue;

                foreach (Direction targetDirection in curTile_Direction)
                {
                    if (direction == ReverseDirection(targetDirection))
                        return true;
                }
            }
        }

        return false;
    }

    public static List<TileNode> GetConnectedNodes(TileNode curNode)
    {
        List<TileNode> nodes = new List<TileNode>();

        List<Direction> connectedDirection = new List<Direction>();
        foreach(Direction direction in curNode.curTile.PathDirection)
            connectedDirection.Add(direction);
        foreach(Direction direction in curNode.curTile.RoomDirection)
            connectedDirection.Add(direction);

        foreach(Direction direction in connectedDirection)
        {
            TileNode tempNode = curNode.DirectionalNode(direction);
            if (tempNode != null && NodeManager.Instance.activeNodes.Contains(tempNode))
            {
                Direction reverseDirection = ReverseDirection(direction);
                if(tempNode.curTile.PathDirection.Contains(reverseDirection) || tempNode.curTile.RoomDirection.Contains(reverseDirection))
                    nodes.Add(tempNode);
            }
        }

        return nodes;
    }

    public static TileNode RayCastTile()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // �浹�� ������Ʈ ó��
            GameObject hitObject = hit.collider.gameObject;
            //Debug.Log("Hit object: " + hitObject.name);
            TileNode tile = hitObject.GetComponentInParent<TileNode>();
            if (tile != null)
                return tile;
        }

        return null;
    }

    public static void SetAvail(bool value, List<TileNode> targetNode)
    {
        foreach (TileNode node in targetNode)
        {
            node.SetAvail(value);
        }
    }

    public static void SetCollider(bool value, List<TileNode> targetNode)
    {
        foreach (TileNode node in targetNode)
        {
            Collider col = node.GetComponentInChildren<Collider>();
            if(col != null)
                col.enabled = value;
        }
    }

    public static Vector3 GetMouseWorldPosition(float yPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, -yPosition);
        float rayDistance;
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            return point;
        }

        // groundPlane�� �������� ���� ���, playerPosition�� ��ȯ�մϴ�.
        return Vector3.zero;
    }

    public static int Find_Data_Index(object target, List<Dictionary<string, object>> targetDic, string key = "ID")
    {
        for (int i = 0; i < targetDic.Count; i++)
        {
            if (targetDic[i][key].ToString() == target.ToString())
            {
                return i;
            }
        }
        return -1; // ��ġ�ϴ� �����Ͱ� ���� ��� -1 ��ȯ
    }

    public static T Find_Prefab<T>(int id, List<Dictionary<string, object>> dataDic) where T : UnityEngine.Object
    {
        int index = Find_Data_Index(id, dataDic);
        string prefabPath = dataDic[index]["Prefab"].ToString();
        T prefab = Resources.Load<T>(prefabPath);
        return prefab;
    }

    public static Direction ReverseDirection(Direction direction)
    {
        switch(direction)
        {
            case Direction.Left:
                return Direction.Right;
            case Direction.LeftDown:
                return Direction.RightUp;
            case Direction.LeftUp:
                return Direction.RightDown;
            case Direction.Right:
                return Direction.Left;
            case Direction.RightDown:
                return Direction.LeftUp;
            case Direction.RightUp:
                return Direction.LeftDown;
        }

        //�ڵ�Ż�� ����
        return Direction.None;
    }

    public static Vector3 GetGridPosition(Vector3 curPos, Direction direction, float distance)
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
        return curPos + dir;
    }

    public static IEnumerator ScaleLerp(Transform target, float startScale, float endScale, float lerpTime)
    {
        float elapsedTime = 0f;
        while (elapsedTime < lerpTime)
        {
            elapsedTime += Time.deltaTime;
            float nextScale = Mathf.Lerp(startScale, endScale, elapsedTime / lerpTime);
            target.localScale = new Vector3(nextScale, nextScale, nextScale);
            yield return null;
        }
        target.localScale = new Vector3(endScale, endScale, endScale);
    }

    public static void ActiveTrigger(Transform transform, string triggerName)
    {
        Animator animator = GetComponetInChildren<Animator>(transform);
        if (animator != null)
            animator.SetTrigger(triggerName);
    }

    public static string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    public static IEnumerator DelayedFunctionCall(UnityAction func, float delayTime)
    {
        float elapsedTime = 0f;
        while (elapsedTime < delayTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        func();
    }

    public static T Find<T>(Transform transform, string path, bool init = false) where T : Component
    {
        Transform trans = transform.Find(path);
        T findObject = null;
        if (trans != null)
        {
            findObject = trans.GetComponent<T>();
            if (init)
                trans.SendMessage("Init", SendMessageOptions.DontRequireReceiver);
        }

        return findObject;
    }

    public static T GetComponetInChildren<T>(Transform transform, bool init = false) where T : Component
    {
        T t = transform.GetComponentInChildren<T>();
        if (t != null && init)
            t.SendMessage("Init", SendMessageOptions.DontRequireReceiver);

        return t;
    }

    public static T FindobjectOfType<T>(bool init = false) where T : Component
    {
        T t = GameObject.FindObjectOfType<T>();
        if (t != null)
        {
            if (init)
                t.transform.SendMessage("Init", SendMessageOptions.DontRequireReceiver);
        }
        return t;
    }

    // Ÿ���� �Ǵ� transform / Ÿ�����κ����� ��� / ������ �Լ�
    public static Button BindingFunc(Transform transform, string path, UnityAction action)
    {
        Button button = Find<Button>(transform, path);
        if (button != null)
        {
            button.onClick.AddListener(action);
        }
        return button;
    }

    public static Button BindingFunc(Transform transform, UnityAction action)
    {
        Button button = transform.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(action);
        }
        return button;
    }

    public static T Instantiate<T>(string path, Transform parent, bool init = false, bool active = true) where T : UnityEngine.Component
    {
        T objectType = Resources.Load<T>(path);
        if (objectType != null)
        {
            objectType = Object.Instantiate(objectType);
            if (objectType != null)
            {
                if (init)
                    objectType.SendMessage("Init", SendMessageOptions.DontRequireReceiver);

                objectType.gameObject.SetActive(active);
            }
        }
        return objectType;

    }

    public static T CreateObject<T>(Transform parent, bool init = false) where T : Component
    {
        GameObject obj = new GameObject(typeof(T).Name, typeof(T));
        obj.transform.SetParent(parent);
        T t = obj.GetComponent<T>();
        if (init)
            t.SendMessage("Init", SendMessageOptions.DontRequireReceiver);

        return t;
    }
}
