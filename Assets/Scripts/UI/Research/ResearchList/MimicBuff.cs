using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicBuff : MonoBehaviour, Research
{
    [SerializeField]
    private int value = 2;

    public void ActiveResearch()
    {
        PassiveManager.Instance.GolemHoldbackUp(value);
    }
}