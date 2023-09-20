using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FxsController : MonoBehaviour
{
    [SerializeField]
    private Slider slider;

    public void VolumeUpdateCheck()
    {
        if (slider == null)
            return;

        SettingManager.Instance.fxVolume = slider.value;
        AudioManager.Instance.UpdateFxVolume(SettingManager.Instance._FxVolume);
        SaveManager.Instance.SaveSettingData();
    }

    void Awake()
    {
        if (slider != null)
            slider.value = SettingManager.Instance.fxVolume;
    }
}
