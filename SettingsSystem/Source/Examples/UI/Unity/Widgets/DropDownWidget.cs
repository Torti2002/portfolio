using UnityEngine;
using TMPro;

public class DropdownWidget : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private TMP_Dropdown dropdown;

    private SettingSystemV1 system;
    private Setting setting;

    public void Init(Setting setting, SettingSystemV1 system)
    {
        this.setting = setting;
        this.system = system;

        var def  = (SettingDefinition_Dropdown)setting.Definition;
        var save = (DropdownSave)setting.SaveObj;

        label.text = string.IsNullOrEmpty(def.settingKey) ? def.settingDescription : def.settingKey;

        dropdown.ClearOptions();
        if (def.options != null && def.options.Length > 0)
            dropdown.AddOptions(new System.Collections.Generic.List<string>(def.options));

        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.SetValueWithoutNotify(Mathf.Clamp(save?.value ?? 0, 0, (def.options?.Length ?? 1) - 1));
        dropdown.onValueChanged.AddListener(OnChanged);
    }

    private void OnChanged(int index)
    {
        // clamp passiert intern
        system.TrySetDropdownIndex(setting.Definition.settingKey, index);
    }
}
