using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileControlUI : MonoBehaviour
{
    [SerializeField]
    private GameObject tileMoveBtn;
    [SerializeField]
    private GameObject tileRemoveBtn;

    public void MoveTile()
    {
        Tile curTile = InputManager.Instance._CurTile;
        if (curTile == null)
            return;
        if (curTile.Movable)
        {
            curTile.ReadyForMove();
        }
        else
            GameManager.Instance.popUpMessage.ToastMsg("Ÿ�� ���� ĳ���Ͱ� �־� ������ �� �����ϴ�!");
    }

    public void RemoveTile()
    {
        Tile curTile = InputManager.Instance._CurTile;
        if (curTile == null)
            return;
        if(curTile.IsRemovable)
        {
            curTile.RemoveTile();
            InputManager.Instance.ResetTileClick();
        }
        else
            GameManager.Instance.popUpMessage.ToastMsg("�ΰ� �̻��� Ÿ�ϰ� ����� Ÿ���� ������ �� �����ϴ�!");
    }

    public void SetButton(bool movable, bool removable)
    {
        tileMoveBtn.SetActive(movable);
        tileRemoveBtn.SetActive(removable);
    }

    public void Update()
    {
        if (!tileMoveBtn.gameObject.activeSelf && !tileRemoveBtn.gameObject.activeSelf)
            return;

    }
}
