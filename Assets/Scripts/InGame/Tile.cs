using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Start,
    Path,
    Room,
    End,
}

public class Tile : MonoBehaviour
{
    [SerializeField]
    private List<Direction> pathDirection = new List<Direction>();
    [SerializeField]
    private List<Direction> roomDirection = new List<Direction>();
    [SerializeField]
    private TileType roomType;

    public List<Direction> PathDirection { get => pathDirection; }
    public List<Direction> RoomDirection { get => roomDirection; }

    public TileNode curNode;

    public bool isActive = false;

    public bool movable = false;

    public bool waitToMove = false;

    public Tile twin = null;

    public void MoveTile(TileNode nextNode)
    {
        if(curNode != null)
            curNode.curTile = null;

        curNode = nextNode;
        nextNode.curTile = this;
        transform.position = nextNode.transform.position;
    }

    private void UpdateMoveTilePos(TileNode curNode)
    {
        if (curNode != null && NodeManager.Instance.emptyNodes.Contains(curNode))
            twin.transform.position = curNode.transform.position;
        else
            twin.transform.position = new Vector3(0, 10000, 0);
    }

    private void EndMoveing()
    {
        twin.gameObject.SetActive(false);
        waitToMove = false;
        InputManager.Instance.settingCard = false;
        NodeManager.Instance.ResetAvail();
        //Destroy(twin.gameObject);
        twin = null;
    }

    private void InstanceTwin()
    {
        twin = Instantiate(this);
        twin.waitToMove = false;
    }

    private void CheckEndInput(TileNode curNode)
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (curNode != null && curNode.setAvail)
            {
                NodeManager.Instance.SetActiveNode(this.curNode, false);
                NodeManager.Instance.SetActiveNode(curNode, true);
                MoveTile(curNode);
            }

            EndMoveing();
        }
        else if (Input.GetKeyDown(KeyCode.Mouse1))
            EndMoveing();
    }

    private void RotateDirection()
    {
        transform.rotation *= Quaternion.Euler(0f, 60f, 0f);
    }

    private void RotateCheck()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            twin.RotateTile();
        }
    }

    public List<Direction> RotateDirection(List<Direction> pathDirection)
    {
        //현재 노드의 pathDirection과 roomDirection을 60도 회전하는 함수
        List<Direction> newDirection = new List<Direction>();
        foreach (Direction dir in pathDirection)
        {
            switch (dir)
            {
                case Direction.Left:
                    newDirection.Add(Direction.LeftUp);
                    break;
                case Direction.LeftUp:
                    newDirection.Add(Direction.RightUp);
                    break;
                case Direction.LeftDown:
                    newDirection.Add(Direction.Left);
                    break;
                case Direction.Right:
                    newDirection.Add(Direction.RightDown);
                    break;
                case Direction.RightUp:
                    newDirection.Add(Direction.Right);
                    break;
                case Direction.RightDown:
                    newDirection.Add(Direction.LeftDown);
                    break;
            }
        }

        return newDirection;
    }

    public void RotateTile()
    {
        pathDirection = RotateDirection(pathDirection);
        roomDirection = RotateDirection(roomDirection);
        RotateDirection();
    }

    public void ResetTwin()
    {
        twin.pathDirection = pathDirection;
        twin.roomDirection = roomDirection;
    }

    private void Update()
    {
        if (!waitToMove)
            return;
        if (twin == null)
        {
            InstanceTwin();
            return;
        }
        else if (!twin.gameObject.activeSelf)
        {
            twin.gameObject.SetActive(true);
            return;
        }

        TileNode curNode = UtilHelper.RayCastTile();
        UpdateMoveTilePos(curNode);
        RotateCheck();
        CheckEndInput(curNode);
    }
}
