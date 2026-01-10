using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Manages the save/load with improvements:
/// - Event system for value changes
/// - Dirty flag system for efficient saving
/// - Better error handling
/// - Async loading support
/// - Settings presets
/// - Validation system
/// </summary>
public class SettingSystemV1 : MonoBehaviour
{
    public static SettingSystemV1 Instance { get; private set; }

    [Header("SettingsUI")]
    [SerializeField] private GameObject settingsUIPrefab;

    // Events
    public event Action<string, object> OnSettingChanged;
    public event Action<string, SettingError, string> OnSettingError;
    public event Action OnSettingsLoaded;

    // Runtime: key -> Setting
    public readonly Dictionary<string, Setting> settings = new Dictionary<string, Setting>();
    public IReadOnlyDictionary<string, Setting> All => settings;

    private Dictionary<string, string> _snapshotByPath;
    private bool _snapshotActive;

    private readonly Dictionary<string, Setting> _byPath = new();
    private readonly HashSet<string> _dirtyKeys = new HashSet<string>();

    public void Add(string key, Setting s) { settings[key] = s; _byPath[s.RelativePath] = s; }
    public void Clear() { settings.Clear(); _byPath.Clear(); _dirtyKeys.Clear(); }

    private string settingsDefRoot;
    private string saveRoot;
    private string presetsRoot;

    private SettingsUI settingsUI;

    // Registry: typeKey -> Handler
    private readonly Dictionary<string, ISettingHandler> handlers = new Dictionary<string, ISettingHandler>()
    {
        { "InputField", new InputFieldHandler() },
        { "Dropdown",   new DropdownHandler()   },
        { "Slider",     new SliderHandler()     },
        { "Toggle",     new ToggleHandler()     },
    };

    // Validators registry
    private readonly Dictionary<string, ISettingValidator> validators = new Dictionary<string, ISettingValidator>();

    private void Awake()
    {
        settingsDefRoot = Path.Combine(Application.streamingAssetsPath, "Settings");
        saveRoot = Path.Combine(Application.persistentDataPath, "Saves", "Settings");
        presetsRoot = Path.Combine(settingsDefRoot, "Presets");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAll();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadAll()
    {
        Clear();

        var indexPath = Path.Combine(settingsDefRoot, "SettingsReferenceCollection.json");
        if (!File.Exists(indexPath)) 
        { 
            Debug.LogError($"Index not found: {indexPath}"); 
            OnSettingError?.Invoke("LoadAll", SettingError.IOError, "Index file not found");
            return; 
        }

        var index = JsonUtility.FromJson<SettingReferenceCollection>(File.ReadAllText(indexPath));
        if (index?.settingRefs == null) 
        { 
            Debug.LogError("Invalid SettingsReferenceCollection.json"); 
            OnSettingError?.Invoke("LoadAll", SettingError.InvalidValue, "Invalid index structure");
            return; 
        }

        foreach (var sr in index.settingRefs)
        {
            if (!handlers.TryGetValue(sr.settingTypeKey, out var handler))
            {
                Debug.LogError($"No handler for typeKey '{sr.settingTypeKey}'");
                continue;
            }

            var defPath = Path.Combine(settingsDefRoot, sr.path);
            if (!File.Exists(defPath)) 
            { 
                Debug.LogError($"Definition file missing: {defPath}"); 
                continue; 
            }

            var def = (SettingDefinition)JsonUtility.FromJson(File.ReadAllText(defPath), handler.DefType);
            if (def == null || string.IsNullOrEmpty(def.settingKey))
            {
                Debug.LogError($"Invalid definition at {defPath}");
                continue;
            }

            // Default-Save anlegen
            object saveObj = handler.CreateDefaultSave(def);

            var savePath = Path.Combine(saveRoot, sr.path);
            if (File.Exists(savePath))
            {
                try
                {
                    var json = File.ReadAllText(savePath);
                    var loaded = handler.LoadSave(json);
                    if (loaded != null)
                        saveObj = handler.Merge(def, loaded);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to load save for {def.settingKey}: {ex.Message}");
                    OnSettingError?.Invoke(def.settingKey, SettingError.IOError, ex.Message);
                }
            }

            var setting = new Setting(def, handler, saveObj, sr.path);
            Add(def.settingKey, setting);
        }

        OnSettingsLoaded?.Invoke();
    }

    public async Task LoadAllAsync()
    {
        Clear();

        var indexPath = Path.Combine(settingsDefRoot, "SettingsReferenceCollection.json");
        if (!File.Exists(indexPath)) 
        { 
            Debug.LogError($"Index not found: {indexPath}"); 
            OnSettingError?.Invoke("LoadAll", SettingError.IOError, "Index file not found");
            return; 
        }

        var indexJson = await File.ReadAllTextAsync(indexPath);
        var index = JsonUtility.FromJson<SettingReferenceCollection>(indexJson);
        if (index?.settingRefs == null) 
        { 
            Debug.LogError("Invalid SettingsReferenceCollection.json"); 
            return; 
        }

        foreach (var sr in index.settingRefs)
        {
            if (!handlers.TryGetValue(sr.settingTypeKey, out var handler))
            {
                Debug.LogError($"No handler for typeKey '{sr.settingTypeKey}'");
                continue;
            }

            var defPath = Path.Combine(settingsDefRoot, sr.path);
            if (!File.Exists(defPath)) continue;

            var defJson = await File.ReadAllTextAsync(defPath);
            var def = (SettingDefinition)JsonUtility.FromJson(defJson, handler.DefType);
            if (def == null || string.IsNullOrEmpty(def.settingKey)) continue;

            object saveObj = handler.CreateDefaultSave(def);

            var savePath = Path.Combine(saveRoot, sr.path);
            if (File.Exists(savePath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(savePath);
                    var loaded = handler.LoadSave(json);
                    if (loaded != null)
                        saveObj = handler.Merge(def, loaded);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to load save for {def.settingKey}: {ex.Message}");
                }
            }

            var setting = new Setting(def, handler, saveObj, sr.path);
            Add(def.settingKey, setting);
        }

        OnSettingsLoaded?.Invoke();
    }

    // --- Write/Save API (with dirty flag) ---
    public SettingError TrySetString(string key, string value, out string errorMessage)
    {
        errorMessage = null;

        if (!settings.TryGetValue(key, out var s))
        {
            errorMessage = $"Setting '{key}' not found";
            OnSettingError?.Invoke(key, SettingError.KeyNotFound, errorMessage);
            return SettingError.KeyNotFound;
        }

        // Validate if validators are defined
        if (s.Definition.validators != null && s.Definition.validators.Length > 0)
        {
            foreach (var validatorKey in s.Definition.validators)
            {
                if (validators.TryGetValue(validatorKey, out var validator))
                {
                    if (!validator.Validate(s.Definition, value, out var validationError))
                    {
                        errorMessage = validationError;
                        OnSettingError?.Invoke(key, SettingError.ValidationFailed, errorMessage);
                        return SettingError.ValidationFailed;
                    }
                }
            }
        }

        if (!s.Handler.TrySetFromString(s.Definition, s.SaveObj, value))
        {
            errorMessage = $"Invalid value '{value}' for setting '{key}'";
            OnSettingError?.Invoke(key, SettingError.InvalidValue, errorMessage);
            return SettingError.InvalidValue;
        }

        _dirtyKeys.Add(key);
        OnSettingChanged?.Invoke(key, s.SaveObj);
        return SettingError.Success;
    }

    public bool TrySetString(string key, string value)
    {
        return TrySetString(key, value, out _) == SettingError.Success;
    }

    public SettingError TrySetFloat(string key, float value, out string errorMessage)
    {
        errorMessage = null;

        if (!settings.TryGetValue(key, out var s))
        {
            errorMessage = $"Setting '{key}' not found";
            OnSettingError?.Invoke(key, SettingError.KeyNotFound, errorMessage);
            return SettingError.KeyNotFound;
        }

        if (!s.Handler.TrySetFromFloat(s.Definition, s.SaveObj, value))
        {
            errorMessage = $"Invalid value '{value}' for setting '{key}'";
            OnSettingError?.Invoke(key, SettingError.InvalidValue, errorMessage);
            return SettingError.InvalidValue;
        }

        _dirtyKeys.Add(key);
        OnSettingChanged?.Invoke(key, s.SaveObj);
        return SettingError.Success;
    }

    public bool TrySetFloat(string key, float value)
    {
        return TrySetFloat(key, value, out _) == SettingError.Success;
    }

    public SettingError TrySetInt(string key, int value, out string errorMessage)
    {
        errorMessage = null;

        if (!settings.TryGetValue(key, out var s))
        {
            errorMessage = $"Setting '{key}' not found";
            OnSettingError?.Invoke(key, SettingError.KeyNotFound, errorMessage);
            return SettingError.KeyNotFound;
        }

        if (!s.Handler.TrySetFromInt(s.Definition, s.SaveObj, value))
        {
            errorMessage = $"Invalid value '{value}' for setting '{key}'";
            OnSettingError?.Invoke(key, SettingError.InvalidValue, errorMessage);
            return SettingError.InvalidValue;
        }

        _dirtyKeys.Add(key);
        OnSettingChanged?.Invoke(key, s.SaveObj);
        return SettingError.Success;
    }

    public bool TrySetInt(string key, int value)
    {
        return TrySetInt(key, value, out _) == SettingError.Success;
    }

    public bool TrySetDropdownIndex(string key, int value)
    {
        return TrySetInt(key, value);
    }

    /// <summary>
    /// Write all dirty settings to disk
    /// </summary>
    public void FlushDirty()
    {
        foreach (var key in _dirtyKeys)
        {
            if (settings.TryGetValue(key, out var s))
            {
                try
                {
                    Persist(key, s);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to persist {key}: {ex.Message}");
                    OnSettingError?.Invoke(key, SettingError.IOError, ex.Message);
                }
            }
        }
        _dirtyKeys.Clear();
    }

    private void Persist(string key, Setting s)
    {
        var file = Path.Combine(saveRoot, s.RelativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        var json = s.Handler.SaveToJson(s.SaveObj);
        File.WriteAllText(file, json);
    }

    // --- Presets ---
    public bool ApplyPreset(string presetName)
    {
        var presetPath = Path.Combine(presetsRoot, $"{presetName}.json");
        if (!File.Exists(presetPath))
        {
            Debug.LogWarning($"Preset not found: {presetName}");
            return false;
        }

        try
        {
            var preset = JsonUtility.FromJson<SettingsPreset>(File.ReadAllText(presetPath));
            if (preset?.values == null) return false;

            foreach (var pv in preset.values)
            {
                TrySetString(pv.settingKey, pv.value);
            }

            FlushDirty();
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to apply preset {presetName}: {ex.Message}");
            return false;
        }
    }

    public string[] GetAvailablePresets()
    {
        if (!Directory.Exists(presetsRoot)) return Array.Empty<string>();

        var files = Directory.GetFiles(presetsRoot, "*.json");
        var presets = new string[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            presets[i] = Path.GetFileNameWithoutExtension(files[i]);
        }
        return presets;
    }

    // --- Reset to Defaults ---
    public void ResetToDefaults()
    {
        foreach (var s in settings.Values)
        {
            s.SaveObj = s.Handler.CreateDefaultSave(s.Definition);
            _dirtyKeys.Add(s.Definition.settingKey);
        }
        FlushDirty();
    }

    public void ResetToDefault(string key)
    {
        if (settings.TryGetValue(key, out var s))
        {
            s.SaveObj = s.Handler.CreateDefaultSave(s.Definition);
            _dirtyKeys.Add(key);
            OnSettingChanged?.Invoke(key, s.SaveObj);
        }
    }

    // --- SettingsUI ---
    public void Open()
    {
        if (UIRoot.Instance == null) { Debug.LogError("UIRoot not available."); return; }

        if (settingsUI == null)
        {
            var go = UIRoot.Instance.ShowGlobalModal(settingsUIPrefab);
            UIRoot.StretchFull((RectTransform)go.transform);
            settingsUI = go.GetComponent<SettingsUI>();
        }

        CreateSnapshot();

        settingsUI.gameObject.SetActive(true);
        settingsUI.transform.SetSiblingIndex(settingsUI.transform.parent.childCount - 1);
        settingsUI.Build();
    }

    public void RevertAndCloseSettings()
    {
        if (_snapshotActive && _snapshotByPath != null)
        {
            foreach (var kv in _snapshotByPath)
            {
                if (_byPath.TryGetValue(kv.Key, out var setting))
                {
                    var restored = setting.Handler.LoadSave(kv.Value);
                    restored = setting.Handler.Merge(setting.Definition, restored);
                    setting.SaveObj = restored;

                    Persist(setting.Definition.settingKey, setting);
                    OnSettingChanged?.Invoke(setting.Definition.settingKey, setting.SaveObj);
                }
            }
        }
        _snapshotActive = false;
        _snapshotByPath = null;
        _dirtyKeys.Clear();

        Close();
    }

    public void CommitAndCloseSettings()
    {
        FlushDirty();
        _snapshotActive = false;
        _snapshotByPath = null;
        Close();
    }

    private void CreateSnapshot()
    {
        _snapshotByPath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var s in settings.Values)
        {
            var json = s.Handler.SaveToJson(s.SaveObj);
            _snapshotByPath[s.RelativePath] = json;
        }
        _snapshotActive = true;
        _dirtyKeys.Clear();
    }

    public void Close()
    {
        if (settingsUI != null)
            settingsUI.gameObject.SetActive(false);
    }

    public void DestroySettings()
    {
        if (settingsUI != null)
        {
            Destroy(settingsUI.gameObject);
            settingsUI = null;
        }
    }

    // --- API to get the settings-values ---
    public float GetFloat(string key, float fallback = 0f)
    {
        if (settings.TryGetValue(key, out var s) && s.SaveObj is SliderSave save)
            return save.value;
        return fallback;
    }

    public int GetInt(string key, int fallback = 0)
    {
        if (settings.TryGetValue(key, out var s))
        {
            if (s.SaveObj is InputFieldSave isf && int.TryParse(isf.value, out var v)) return v;
            if (s.SaveObj is DropdownSave ds) return ds.value;
        }
        return fallback;
    }

    public bool GetBool(string key, bool fallback = false)
    {
        if (settings.TryGetValue(key, out var s) && s.SaveObj is ToggleSave ts)
            return ts.value;
        return fallback;
    }

    public string GetString(string key, string fallback = "")
    {
        if (settings.TryGetValue(key, out var s))
        {
            if (s.SaveObj is InputFieldSave isf) return isf.value;
            if (s.SaveObj is DropdownSave ds)
            {
                var def = (SettingDefinition_Dropdown)s.Definition;
                if (def.options != null && ds.value >= 0 && ds.value < def.options.Length)
                    return def.options[ds.value];
            }
        }
        return fallback;
    }

    // --- Validator Registration ---
    public void RegisterValidator(string key, ISettingValidator validator)
    {
        validators[key] = validator;
    }

    private void OnDestroy()
    {
        FlushDirty(); // Save any pending changes on exit
    }

    private void OnApplicationQuit()
    {
        FlushDirty();
    }
}

// --------- ENUMS ---------
public enum SettingError
{
    Success,
    KeyNotFound,
    InvalidValue,
    ValidationFailed,
    IOError
}

// --------- RUNTIME MODEL ---------
public sealed class Setting
{
    public SettingDefinition Definition;
    public ISettingHandler Handler;
    public object SaveObj;
    public string RelativePath;

    public Setting(SettingDefinition def, ISettingHandler handler, object saveObj, string relativePath)
    {
        Definition = def;
        Handler = handler;
        SaveObj = saveObj;
        RelativePath = relativePath;
    }
}

// --------- HANDLERS ---------
public interface ISettingHandler
{
    Type DefType { get; }
    Type SaveType { get; }

    object CreateDefaultSave(SettingDefinition def);
    object LoadSave(string json);
    object Merge(SettingDefinition def, object loadedSave);

    bool TrySetFromString(SettingDefinition def, object save, string value);
    bool TrySetFromFloat(SettingDefinition def, object save, float value);
    bool TrySetFromInt(SettingDefinition def, object save, int value);

    string SaveToJson(object save);
}

// --------- VALIDATORS ---------
public interface ISettingValidator
{
    bool Validate(SettingDefinition def, object value, out string error);
}

// Example validators
public class MinLengthValidator : ISettingValidator
{
    private int minLength;
    public MinLengthValidator(int min) { minLength = min; }

    public bool Validate(SettingDefinition def, object value, out string error)
    {
        error = null;
        if (value is string str && str.Length < minLength)
        {
            error = $"Value must be at least {minLength} characters long";
            return false;
        }
        return true;
    }
}

public class RangeValidator : ISettingValidator
{
    private float min, max;
    public RangeValidator(float min, float max) { this.min = min; this.max = max; }

    public bool Validate(SettingDefinition def, object value, out string error)
    {
        error = null;
        if (value is float f && (f < min || f > max))
        {
            error = $"Value must be between {min} and {max}";
            return false;
        }
        return true;
    }
}

// --- InputField Handler ---
#region InputFieldHandler
public class InputFieldHandler : ISettingHandler
{
    public Type DefType => typeof(SettingDefinition_InputField);
    public Type SaveType => typeof(InputFieldSave);

    public object CreateDefaultSave(SettingDefinition def)
    {
        var d = (SettingDefinition_InputField)def;
        return new InputFieldSave { value = d.defaultValue ?? "" };
    }

    public object LoadSave(string json) => JsonUtility.FromJson(json, SaveType);

    public object Merge(SettingDefinition def, object loadedSave)
    {
        var d = (SettingDefinition_InputField)def;
        var s = (InputFieldSave)loadedSave ?? (InputFieldSave)CreateDefaultSave(def);

        if (d.numeric)
        {
            if (!double.TryParse(s.value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out _))
                s.value = d.defaultValue ?? "0";
        }
        return s;
    }

    public bool TrySetFromString(SettingDefinition def, object save, string value)
    {
        var d = (SettingDefinition_InputField)def;
        var s = (InputFieldSave)save;

        if (d.numeric && !double.TryParse(value, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out _))
            return false;

        s.value = value ?? "";
        return true;
    }

    public bool TrySetFromFloat(SettingDefinition def, object save, float value)
    {
        var s = (InputFieldSave)save;
        s.value = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return true;
    }

    public bool TrySetFromInt(SettingDefinition def, object save, int value)
    {
        var s = (InputFieldSave)save;
        s.value = value.ToString();
        return true;
    }

    public string SaveToJson(object save) => JsonUtility.ToJson((InputFieldSave)save, true);
}
#endregion

// --- Dropdown Handler ---
#region DropDownHandler
public class DropdownHandler : ISettingHandler
{
    public Type DefType => typeof(SettingDefinition_Dropdown);
    public Type SaveType => typeof(DropdownSave);

    public object CreateDefaultSave(SettingDefinition def)
    {
        var d = (SettingDefinition_Dropdown)def;
        var idx = Array.IndexOf(d.options ?? Array.Empty<string>(), d.defaultValue);
        if (idx < 0) idx = 0;
        return new DropdownSave { value = idx };
    }

    public object LoadSave(string json) => JsonUtility.FromJson(json, SaveType);

    public object Merge(SettingDefinition def, object loadedSave)
    {
        var d = (SettingDefinition_Dropdown)def;
        var s = (DropdownSave)loadedSave ?? (DropdownSave)CreateDefaultSave(def);
        if (d.options == null || d.options.Length == 0) { s.value = 0; return s; }
        s.value = Mathf.Clamp(s.value, 0, d.options.Length - 1);
        return s;
    }

    public bool TrySetFromString(SettingDefinition def, object save, string value)
    {
        var d = (SettingDefinition_Dropdown)def;
        if (d.options == null || d.options.Length == 0) return false;

        var idx = Array.IndexOf(d.options, value);
        if (idx < 0) return false;

        ((DropdownSave)save).value = idx;
        return true;
    }

    public bool TrySetFromFloat(SettingDefinition def, object save, float value)
    {
        return TrySetFromInt(def, save, Mathf.RoundToInt(value));
    }

    public bool TrySetFromInt(SettingDefinition def, object save, int value)
    {
        var d = (SettingDefinition_Dropdown)def;
        if (d.options == null || d.options.Length == 0) return false;

        ((DropdownSave)save).value = Mathf.Clamp(value, 0, d.options.Length - 1);
        return true;
    }

    public string SaveToJson(object save) => JsonUtility.ToJson((DropdownSave)save, true);
}
#endregion

// --- Toggle Handler ---
#region ToggleHandler
public class ToggleHandler : ISettingHandler
{
    public Type DefType => typeof(SettingDefinition_Toggle);
    public Type SaveType => typeof(ToggleSave);

    public object CreateDefaultSave(SettingDefinition def)
    {
        var d = (SettingDefinition_Toggle)def;
        return new ToggleSave { value = d.defaultValue };
    }

    public object LoadSave(string json) => JsonUtility.FromJson(json, SaveType);

    public object Merge(SettingDefinition def, object loadedSave)
        => loadedSave ?? CreateDefaultSave(def);

    public bool TrySetFromString(SettingDefinition def, object save, string value)
    {
        if (!bool.TryParse(value, out var b)) return false;
        ((ToggleSave)save).value = b;
        return true;
    }

    public bool TrySetFromFloat(SettingDefinition def, object save, float value)
    {
        ((ToggleSave)save).value = value >= 0.5f;
        return true;
    }

    public bool TrySetFromInt(SettingDefinition def, object save, int value)
    {
        ((ToggleSave)save).value = value != 0;
        return true;
    }

    public string SaveToJson(object save) => JsonUtility.ToJson((ToggleSave)save, true);
}
#endregion

// --- Slider Handler ---
#region SliderHandler
public class SliderHandler : ISettingHandler
{
    public Type DefType => typeof(SettingDefinition_Slider);
    public Type SaveType => typeof(SliderSave);

    public object CreateDefaultSave(SettingDefinition def)
    {
        var d = (SettingDefinition_Slider)def;
        return new SliderSave { value = Mathf.Clamp(d.defaultValue, d.min, d.max) };
    }

    public object LoadSave(string json) => JsonUtility.FromJson(json, SaveType);

    public object Merge(SettingDefinition def, object loadedSave)
    {
        var d = (SettingDefinition_Slider)def;
        var s = (SliderSave)loadedSave ?? (SliderSave)CreateDefaultSave(def);
        s.value = Mathf.Clamp(s.value, d.min, d.max);
        return s;
    }

    public bool TrySetFromString(SettingDefinition def, object save, string value)
    {
        if (!float.TryParse(value, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var f))
            return false;

        return TrySetFromFloat(def, save, f);
    }

    public bool TrySetFromFloat(SettingDefinition def, object save, float value)
    {
        var d = (SettingDefinition_Slider)def;
        ((SliderSave)save).value = Mathf.Clamp(value, d.min, d.max);
        return true;
    }

    public bool TrySetFromInt(SettingDefinition def, object save, int value)
        => TrySetFromFloat(def, save, value);

    public string SaveToJson(object save) => JsonUtility.ToJson((SliderSave)save, true);
}
#endregion

// --------- DATA MODELS ---------
[Serializable]
public abstract class SettingDefinition
{
    public string settingKey;
    public string category;
    public string settingDescription;
    public string[] tags;
    public string[] validators; // e.g., ["MinLength:3", "Range:0-100"]
}

[Serializable] public class SettingDefinition_Toggle : SettingDefinition
{
    public bool defaultValue;
}
[Serializable] public class ToggleSave { public bool value; }

[Serializable]
public class SettingDefinition_InputField : SettingDefinition
{
    public bool numeric;
    public string defaultValue;
}
[Serializable] public class InputFieldSave { public string value; }

[Serializable]
public class SettingDefinition_Dropdown : SettingDefinition
{
    public string[] options;
    public string defaultValue;
}
[Serializable] public class DropdownSave { public int value; }

[Serializable]
public class SettingDefinition_Slider : SettingDefinition
{
    public float min;
    public float max;
    public float defaultValue;
    public bool wholeNumbers;
}
[Serializable] public class SliderSave { public float value; }

[Serializable]
public class SettingReferenceCollection { public SettingRef[] settingRefs; }

[Serializable]
public class SettingRef { public string settingTypeKey; public string path; }

// --------- PRESETS ---------
[Serializable]
public class SettingsPreset
{
    public string presetName;
    public PresetValue[] values;
}

[Serializable]
public class PresetValue
{
    public string settingKey;
    public string value;
}