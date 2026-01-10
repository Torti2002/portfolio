using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;


public class SettingsUI : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject inputFieldWidgetPrefab;
    [SerializeField] private GameObject   dropdownWidgetPrefab;
    [SerializeField] private GameObject sliderWidgetPrefab;
    [SerializeField] private GameObject toggleWidgetPrefab;

    [SerializeField] private CategoryButton   categoryButtonPrefab; // kleines Script, s.u.

    [SerializeField] private Transform widgetsContentParent;
    [SerializeField] private Transform categoriesContentParent;

    [SerializeField] private Button cancelAndCloseSettingsButton; // exit without saving changes
    [SerializeField] private Button commitAndCloseSettingsButton; // saving changes

    [SerializeField] private TMP_InputField searchInput;

    private string activeCategory = "All";                        // default
    private string currentQuery = "";                              // aktueller Suchstring (lowercased)



    public void Build()
    {
        // Clear
        foreach (Transform child in widgetsContentParent) Destroy(child.gameObject);
        foreach (Transform child in categoriesContentParent) Destroy(child.gameObject);

        var all = SettingSystemV1.Instance.All.Values;

        // Kategorien sammeln
        var cats = new HashSet<string>();
        foreach (var s in all) if (!string.IsNullOrEmpty(s.Definition.category)) cats.Add(s.Definition.category);

        // Kategorie-Buttons bauen (+ "All")
        MakeCategoryButton("All", isAll: true);
        foreach (var c in cats) MakeCategoryButton(c, isAll: false);

        // Wenn noch keine aktiv, „All“
        activeCategory ??= "All";

        // Widgets bauen (optional sortiert)
        foreach (var setting in all)
        {
            //if (activeCategory != "Alle" && !string.Equals(setting.Definition.category, activeCategory, StringComparison.OrdinalIgnoreCase))
            //continue;

            if (setting.Handler is InputFieldHandler)
            {
                var ui = Instantiate(inputFieldWidgetPrefab, widgetsContentParent).GetComponent<InputFieldWidget>();
                ui.Init(setting, SettingSystemV1.Instance);
            }
            else if (setting.Handler is SliderHandler)
            {
                var ui = Instantiate(sliderWidgetPrefab, widgetsContentParent).GetComponent<SliderWidget>();
                ui.Init(setting, SettingSystemV1.Instance);
            }
            else if (setting.Handler is DropdownHandler)
            {
                var ui = Instantiate(dropdownWidgetPrefab, widgetsContentParent).GetComponent<DropdownWidget>();
                ui.Init(setting, SettingSystemV1.Instance);
            }
            else if (setting.Handler is ToggleHandler)
            {
                var ui = Instantiate(toggleWidgetPrefab, widgetsContentParent).GetComponent<ToggleWidget>();
                ui.Init(setting, SettingSystemV1.Instance);
            }
        }

        if (cancelAndCloseSettingsButton != null)
        {
            cancelAndCloseSettingsButton.onClick.RemoveAllListeners();
            cancelAndCloseSettingsButton.onClick.AddListener(() =>
            {
                // Abbrechen: Snapshot zurückspielen
                SettingSystemV1.Instance.RevertAndCloseSettings();
            });
        }

        if (commitAndCloseSettingsButton != null)
        {
            commitAndCloseSettingsButton.onClick.RemoveAllListeners();
            commitAndCloseSettingsButton.onClick.AddListener(() =>
            {
                // Speichern: Snapshot verwerfen (on-change ist bereits persistiert)
                SettingSystemV1.Instance.CommitAndCloseSettings();
            });
        }

        if (searchInput != null)
        {
            searchInput.onValueChanged.RemoveAllListeners();
            searchInput.onValueChanged.AddListener(OnSearchChanged);
            // falls du den zuletzt gesetzten Query behalten willst:
            if (!string.Equals(searchInput.text, currentQuery, System.StringComparison.Ordinal))
                searchInput.SetTextWithoutNotify(currentQuery);
        }

        RebuildListAndCategories();
    }

    private void RebuildListAndCategories()
    {
        // Kategorien-UI neu aufbauen
        foreach (Transform child in categoriesContentParent) Destroy(child.gameObject);

        var all = SettingSystemV1.Instance.All.Values;

        // Kategorien sammeln (nur die, die überhaupt existieren)
        var cats = new HashSet<string>();
        foreach (var s in all)
            if (!string.IsNullOrEmpty(s.Definition.category))
                cats.Add(s.Definition.category);

        // Buttons (+ "Alle")
        MakeCategoryButton("All", isAll: true);
        foreach (var c in cats.OrderBy(x => x))
            MakeCategoryButton(c, isAll: false);

        // Widgets neu rendern (mit Filter)
        foreach (Transform child in widgetsContentParent) Destroy(child.gameObject);

        foreach (var setting in all.OrderBy(s => s.Definition.settingKey))
        {
            if (!PassesFilters(setting)) continue;

            if (setting.Handler is InputFieldHandler)
                Instantiate(inputFieldWidgetPrefab, widgetsContentParent)
                    .GetComponent<InputFieldWidget>().Init(setting, SettingSystemV1.Instance);
            else if (setting.Handler is SliderHandler)
                Instantiate(sliderWidgetPrefab, widgetsContentParent)
                    .GetComponent<SliderWidget>().Init(setting, SettingSystemV1.Instance);
            else if (setting.Handler is DropdownHandler)
                Instantiate(dropdownWidgetPrefab, widgetsContentParent)
                    .GetComponent<DropdownWidget>().Init(setting, SettingSystemV1.Instance);
            else if (setting.Handler is ToggleHandler)
                Instantiate(toggleWidgetPrefab, widgetsContentParent)
                    .GetComponent<ToggleWidget>().Init(setting, SettingSystemV1.Instance);
        }
    }

    private void OnSearchChanged(string text)
    {
        currentQuery = (text ?? "").Trim();
        RebuildListAndCategories();
    }

    private bool PassesFilters(Setting s)
    {
        var def = s.Definition;
        // 1) Kategorie
        if (!string.Equals(activeCategory, "All", System.StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(def.category ?? "", activeCategory, System.StringComparison.OrdinalIgnoreCase))
                return false;
        }

        // 2) Suche (case-insensitive, alle Worte müssen matchen)
        if (!string.IsNullOrWhiteSpace(currentQuery))
        {
            var hayKey = def.settingKey?.ToLowerInvariant() ?? "";
            var hayDesc = def.settingDescription?.ToLowerInvariant() ?? "";
            var hayTags = def.tags != null ? string.Join(" ", def.tags).ToLowerInvariant() : "";

            // split by whitespace; einfache UND-Suche
            var terms = currentQuery.ToLowerInvariant()
                                    .Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (var t in terms)
            {
                if (!(hayKey.Contains(t) || hayDesc.Contains(t) || hayTags.Contains(t)))
                    return false; // ein Term nicht gefunden -> raus
            }
        }

        return true;
    }
    
    private void MakeCategoryButton(string label, bool isAll)
    {
        var btn = Instantiate(categoryButtonPrefab, categoriesContentParent);
        btn.Setup(label, () =>
        {
            activeCategory = isAll ? "All" : label;
            RebuildListAndCategories(); // nur Liste & Buttons neu zeichnen
        });

        // Optional: aktiven Button visuell markieren
        btn.SetActiveVisual(string.Equals(activeCategory, isAll ? "All" : label, System.StringComparison.OrdinalIgnoreCase));
    }
}