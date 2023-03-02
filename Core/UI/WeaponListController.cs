using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponListController
{
    private VisualTreeAsset listEntryTemplate;

    private ListView weaponList;
    private Label weaponNameLabel;
    private VisualElement weaponIcon;
    private List<WarriorWeaponSO> allWeapons;
    public void InitializeWeaponList(VisualElement root, VisualTreeAsset listElementTemplate)
    {
        EnumerateAllWeapons();

        listEntryTemplate = listElementTemplate;
        weaponList = root.Q<ListView>("weapon-list");
        weaponNameLabel = root.Q<Label>("weapon-name");

        FillWeaponList();

        weaponList.onSelectionChange += OnWeaponSelected;
    }

    public void ReEnumerateAllWeapons(List<WarriorWeaponSO> weapons)
    {
        allWeapons.Clear();
        allWeapons.AddRange(weapons);
        weaponList.Clear();
        FillWeaponList();
    }

    public void EnumerateAllWeapons()
    {
        allWeapons = new List<WarriorWeaponSO>();
        allWeapons.AddRange(Resources.LoadAll<WarriorWeaponSO>("Weapons"));
    }

    void FillWeaponList()
    {
        weaponList.makeItem = () =>
        {
            var newListEntry = listEntryTemplate.Instantiate();
            var newListEntryLogic = new WeaponListEntryController();

            newListEntry.userData = newListEntryLogic;
            newListEntryLogic.SetVisualElement(newListEntry);

            return newListEntry;
        };

        weaponList.bindItem = (item, index) =>
        {
            (item.userData as WeaponListEntryController).SetWeaponData(allWeapons[index]);
        };

        weaponList.fixedItemHeight = 45;
        weaponList.itemsSource = allWeapons;
    }

    void OnWeaponSelected(IEnumerable<object> selectedItems)
    {
        var selectedWeapon = weaponList.selectedItem as WarriorWeaponSO;

        if (selectedWeapon == null)
        {
            weaponNameLabel.text = "";
            weaponIcon.style.backgroundImage = null;

            return;
        }
    }
}
