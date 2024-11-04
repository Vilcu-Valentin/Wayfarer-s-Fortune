using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SelectedModuleUI : MonoBehaviour
{
    [SerializeField] private CaravanManager caravanManager;

    [SerializeField] private TextMeshProUGUI moduleName;
    [SerializeField] private TextMeshProUGUI moduleSize;
    [SerializeField] private TextMeshProUGUI inventorySize;

    private StorageModule selectedModule;

    public void UpdateSelectedModule(StorageModule selectedModule)
    {
        this.selectedModule = selectedModule;

        moduleName.text = selectedModule.moduleData.name;
        moduleSize.text = "Size: " + selectedModule.moduleData.size.ToString();
        inventorySize.text = "Inventory: " + selectedModule.moduleData.inventorySize.ToString();
    }

    public void DeleteModule()
    {
        gameObject.SetActive(false);
    }
}
