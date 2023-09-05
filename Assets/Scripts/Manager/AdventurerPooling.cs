using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdventurerPooling : IngameSingleton<AdventurerPooling>
{
    private List<Adventurer> adventurerPool = new List<Adventurer>();
    
    private Adventurer GetAdventurerInPool(string adventurerId)
    {
        foreach(Adventurer adventurer in adventurerPool)
        {
            if (!adventurer.gameObject.activeSelf && adventurer.BattlerID == adventurerId)
                return adventurer;
        }

        return null;
    }

    public void SpawnAdventurer(string adventurerName)
    {
        //1. ���� adventurerPool�� �ش� adventurerId�� activeSelf == false�� Ÿ�� Ž��
        //2. 1���� Ÿ���� �����Ѵٸ� Init�����ְ�, SetActive(true)
        //3. �������� �ʴ´ٸ� ���� Instantiate

        int adventurerIndex = UtilHelper.Find_Data_Index(adventurerName, DataManager.Instance.Battler_Table, "name");
        string prefab = DataManager.Instance.Battler_Table[adventurerIndex]["prefab"].ToString();
        string adventurerId = DataManager.Instance.Battler_Table[adventurerIndex]["id"].ToString();

        Adventurer adventurer = GetAdventurerInPool(adventurerId);

        if(adventurer == null)
        {
            Adventurer targetPrefab = Resources.Load<Adventurer>("Prefab/Adventurer/" + prefab);
            adventurer = Instantiate(targetPrefab, transform);
            adventurerPool.Add(adventurer);
        }

        adventurer.ResetNode();
        adventurer.Init();
        adventurer.gameObject.SetActive(true);
        GameManager.Instance.adventurersList.Add(adventurer);
    }
}
