using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct WaveData
{
    public int number;
    public string adventurerName;
    public string prefab;

    public WaveData(string adventurerName, int number, string prefab)
    {
        this.adventurerName = adventurerName;
        this.number = number;
        this.prefab = prefab;
    }
}

public class WaveController : MonoBehaviour
{
    public List<GameObject> adventurerPrefab;

    [SerializeField]
    private TextMeshProUGUI waveText;
    [SerializeField]
    private WaveGauge waveFill;

    public float WaveProgress { get { return waveFill.WaveRate; } }

    private float CalSpawnWaitTime(int allAmount, float restrictTime = 720f)
    {
        float spawnTime = 2f;
        restrictTime = 720 / (GameManager.Instance.DefaultSpeed);
        if (allAmount * spawnTime > restrictTime)
        {
            spawnTime = restrictTime / allAmount;
        }
        return spawnTime;
    }

    public List<WaveData> SetWaveData(int waveIndex)
    {
        //List<Dictionary<string, object>> wave_Table����
        //key���� "level"�� �ε����� ã��
        List<int> indexList = new List<int>();

        for(int i = 0; i < DataManager.Instance.Wave_Table.Count; i++)
        {
            if (Convert.ToInt32(DataManager.Instance.Wave_Table[i]["level"]) == waveIndex)
                indexList.Add(i);
        }

        List<WaveData> curWave = new List<WaveData>();
        foreach(int i in indexList)
        {
            string adventurerName = DataManager.Instance.Wave_Table[i]["adventure"].ToString();
            int number = Convert.ToInt32(DataManager.Instance.Wave_Table[i]["num"]);
            int adventurerIndex = UtilHelper.Find_Data_Index(adventurerName, DataManager.Instance.Battler_Table, "name");
            string prefab = DataManager.Instance.Battler_Table[adventurerIndex]["prefab"].ToString();
            WaveData waveData = new WaveData(adventurerName, number, prefab);
            curWave.Add(waveData);
        }

        return curWave;
    }

    public IEnumerator ISpawnWave(int waveIndex, List<WaveData> curWave)
    {
        waveText.text = (waveIndex + 1).ToString("D2");
        int maxEnemyNumber = 0;
        int curEnemyNumber = 0;
        foreach(WaveData waveData in curWave)
            maxEnemyNumber += waveData.number;
        waveFill.SetWaveGauge(waveIndex, curEnemyNumber, maxEnemyNumber);
        float spawnWaitTime = CalSpawnWaitTime(maxEnemyNumber);
        foreach (WaveData waveData in curWave)
        {
            for(int i = 0; i < waveData.number; i++)
            {
                float elapsedTime = 0f;
                while (elapsedTime < spawnWaitTime)
                {
                    elapsedTime += Time.deltaTime * GameManager.Instance.timeScale;
                    yield return null;
                }

                //���谡 ����
                GameObject prefab = Resources.Load<GameObject>("Prefab/Adventurer/" + waveData.prefab);
                GameObject temp = Instantiate(prefab);
                Adventurer adventurer = temp.GetComponent<Adventurer>();
                adventurer.Init();
                GameManager.Instance.adventurersList.Add(adventurer);

                curEnemyNumber++;

                waveFill.SetWaveGauge(waveIndex, curEnemyNumber, maxEnemyNumber);
            }
        }
        yield return null;
    }

    public bool SpawnWave(int curWave)
    {
        List<WaveData> waveData = SetWaveData(curWave);
        if (waveData != new List<WaveData>() && waveData.Count != 0)
        {
            StartCoroutine(ISpawnWave(curWave, waveData));
            return true;
        }
        else
            return false;
    }

    public void Init()
    {

    }
}
