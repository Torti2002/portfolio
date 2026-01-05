using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderWidget : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI valueText;

    private SettingSystemV1 system;
    private Setting setting;

    public void Init(Setting setting, SettingSystemV1 system)
    {
        this.setting = setting;
        this.system = system;

        var def  = (SettingDefinition_Slider)setting.Definition;
        var save = (SliderSave)setting.SaveObj;

        label.text = string.IsNullOrEmpty(def.settingKey) ? def.settingDescription : def.settingKey;

        // Slider Range & Value
        slider.minValue = def.min;
        slider.maxValue = def.max;
        slider.wholeNumbers = def.wholeNumbers;
        slider.SetValueWithoutNotify(save?.value ?? def.defaultValue);
        UpdateValueLabel(slider.value);

        slider.onValueChanged.AddListener(OnChanged);
    }

    private void OnChanged(float v)
    {
        // SettingsSystem clamp’t intern
        system.TrySetFloat(setting.Definition.settingKey, v);
        UpdateValueLabel(((SliderSave)setting.SaveObj).value);
    }

    private void UpdateValueLabel(float v)
    {
        valueText.text = v.ToString("0.##"); // Format wie du willst
    }
}
