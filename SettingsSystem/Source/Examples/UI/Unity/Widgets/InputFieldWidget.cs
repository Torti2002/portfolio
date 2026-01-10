using UnityEngine;
using TMPro;

public class InputFieldWidget : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private TMP_InputField input;

    private SettingSystemV1 system;
    private Setting setting;

    public void Init(Setting setting, SettingSystemV1 system)
    {
        this.setting = setting;
        this.system = system;

        var def = (SettingDefinition_InputField)setting.Definition;

        // Label
        label.text = string.IsNullOrEmpty(def.settingKey) ? def.settingDescription : def.settingKey;


        // Startwert
        var save = (InputFieldSave)setting.SaveObj;
        input.text = save?.value ?? "";

        // Events
        input.onEndEdit.AddListener(OnEndEdit);
    }

    private void OnEndEdit(string newValue)
    {
        // TrySetString validiert numeric intern (Handler)
        if (!system.TrySetString(setting.Definition.settingKey, newValue))
        {
            // Falls invalid (z.B. numeric erwartet): revert
            var current = (InputFieldSave)setting.SaveObj;
            input.text = current?.value ?? "";
        }
    }
}