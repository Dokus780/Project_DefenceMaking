using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Test : MonoBehaviour
{
    public QuestInfo infomer;
    public Quest targetQuest;

    private void SetQuest()
    {


        if (targetQuest == null || infomer == null)
            return;

        infomer.SetQuest(targetQuest);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.T)) { SetQuest(); }
        if(Input.GetKeyDown(KeyCode.A))
        {
            GameManager.Instance.king.GetStatusEffect<Poison>(new Poison(GameManager.Instance.king, 100000));

        }
    }
}
