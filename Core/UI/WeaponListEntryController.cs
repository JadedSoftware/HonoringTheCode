using UnityEngine.UIElements;

public class WeaponListEntryController
{
    Label m_NameLabel;

    public void SetVisualElement(VisualElement visualElement)
    {
        m_NameLabel = visualElement.Q<Label>("weapon-name");
    }

    public void SetWeaponData(WarriorWeaponSO weaponData)
    {
        m_NameLabel.text = weaponData.weaponName;
    }
}