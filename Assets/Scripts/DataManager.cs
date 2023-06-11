using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    private List<Dictionary<string, object>> wave_Table;

    public List<Dictionary<string, object>> Wave_Table { get => wave_Table; }

    // csv���� �ּ�(Resource���� ��)
    private string wave_Table_DataPath = "Data/waveData";


    private void Init()
    {
        // csv���� �ҷ����� �Լ�
        //skill_Active_Dic = CSVLoader.LoadCSV(Resources.Load<TextAsset>(skill_Active_DataPath));
        wave_Table = CSVLoader.LoadCSV(Resources.Load<TextAsset>(wave_Table_DataPath));
    }
}
