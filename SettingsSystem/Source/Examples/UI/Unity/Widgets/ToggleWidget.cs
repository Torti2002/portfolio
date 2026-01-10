using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class ToggleWidget : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Toggle toggle;

    private SettingSystemV1 system;
    private Setting setting;

    public void Init(Setting setting, SettingSystemV1 system)
    {
        this.setting = setting;
        this.system = system;

        var def = (SettingDefinition_Toggle)setting.Definition;
        var save = (ToggleSave)setting.SaveObj;

        label.text = string.IsNullOrEmpty(def.settingKey) ? def.settingDescription : def.settingKey;
        toggle.SetIsOnWithoutNotify(save?.value ?? def.defaultValue);

        toggle.onValueChanged.AddListener(OnChanged);
    }

    private void OnChanged(bool v)
    {
        // kleine Brücke: verwende TrySetInt(1/0) oder bau TrySetBool
        system.TrySetInt(setting.Definition.settingKey, v ? 1 : 0);
    }
}
