using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModuleButtonUI : MonoBehaviour
{
    public Button Button;
    public TextMeshProUGUI ModuleName;
    public Image icon;

    private StorageModule module;

    public void Setup(StorageModule moduleData,  bool highlight)
    {
        Debug.Log("This module is highlighted: " + highlight);
        module = moduleData;

        ModuleName.text = moduleData.moduleData.name;
        icon.sprite = moduleData.moduleData.icon;

        if (highlight)
            icon.color = new Color(100, 65, 65, 100);

        Button.onClick.AddListener(OnSelectModule);
    }

    private void OnSelectModule()
    {
        Inventory.Instance.SelectModule(module);
        Setup(module, true);
    }
}
