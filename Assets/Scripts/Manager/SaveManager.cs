using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : Singleton<SaveManager>
{
    // --- ���� ������ �����̸� ���� ("���ϴ� �̸�(����).json") --- //
    string settingDataFileName = "SettingData.json";

    // --- ����� Ŭ���� ���� --- //
    public SettingData settingData = new SettingData();

    public void LoadSettingData()
    {
        settingData = LoadData<SettingData>(settingDataFileName);
        SettingManager.Instance.bgmVolume = settingData.volume_Bgm;
        SettingManager.Instance.fxVolume = settingData.volume_Fxs;
        SettingManager.Instance.screen_FullSize = settingData.fullScreen;
        SettingManager.Instance.ScreenSizeIndex = settingData.screenSizeIndex;
        SettingManager.Instance.mouseSensitivity = settingData.mouseSensitivity;
    }

    public void SaveSettingData()
    {
        settingData.volume_Bgm = SettingManager.Instance.bgmVolume;
        settingData.volume_Fxs = SettingManager.Instance.fxVolume;
        settingData.fullScreen = SettingManager.Instance.screen_FullSize;
        settingData.screenSizeIndex = SettingManager.Instance.ScreenSizeIndex;
        settingData.mouseSensitivity = SettingManager.Instance.mouseSensitivity;

        SaveData(settingData, settingDataFileName);
    }

    private T GenerateNewData<T>() where T : new()
    {
        return new T();
    }

    // �ҷ�����
    public T LoadData<T>(string fileName) where T : new()
    {
        string filePath = Application.persistentDataPath + "/" + fileName;

        // ����� ������ �ִٸ�
        if (File.Exists(filePath))
        {
            // ����� ���� �о���� Json�� Ŭ���� �������� ��ȯ�ؼ� �Ҵ�
            string FromJsonData = File.ReadAllText(filePath);
            return JsonUtility.FromJson<T>(FromJsonData);
        }
        else
            return GenerateNewData<T>();
    }


    // �����ϱ�
    public void SaveData(object data, string fileName)
    {
        // Ŭ������ Json �������� ��ȯ (true : ������ ���� �ۼ�)
        string ToJsonData = JsonUtility.ToJson(data, true);
        string filePath = Application.persistentDataPath + "/" + fileName;
        print(filePath);
        // �̹� ����� ������ �ִٸ� �����, ���ٸ� ���� ���� ����
        File.WriteAllText(filePath, ToJsonData);
    }

    public void Init()
    {
        settingData = LoadData<SettingData>(settingDataFileName);
    }

    public void OnApplicationQuit()
    {
        SaveSettingData();
    }
}
