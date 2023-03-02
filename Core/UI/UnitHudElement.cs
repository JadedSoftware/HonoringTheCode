using UnityEngine.UIElements;

namespace Core.UI
{
    public class UnitHudElement
    {
        private VisualElement ApHealthContainer;
        private ProgressBar apProgressBar;
        private ProgressBar healthProgressBar;
        private readonly Label nameLabel;
        private readonly VisualElement playerHudRow;
        private PlayerUnit unit;

        public UnitHudElement(PlayerUnit playerUnit, VisualElement UnitHudOverlay)
        {
            playerHudRow = new VisualElement();
            playerHudRow.ClearClassList();
            GameOverlayController.instance.ClearPaddingMargin(playerHudRow);
            playerHudRow.AddToClassList(UiOverlayHelper.PlayerHudRow);
            UnitHudOverlay.Add(playerHudRow);

            nameLabel = new Label(playerUnit.unitName);
            nameLabel.ClearClassList();
            nameLabel.AddToClassList(UiOverlayHelper.PlayerHudName);
            playerHudRow.Add(nameLabel);

            VisualElement ApHealthContainer = new();
            ApHealthContainer.ClearClassList();
            ApHealthContainer.AddToClassList(UiOverlayHelper.HudProgressContainer);
            playerHudRow.Add(ApHealthContainer);

            ProgressBar healthProgressBar = new();
            healthProgressBar.ClearClassList();
            healthProgressBar.AddToClassList(UiOverlayHelper.HudProgressStyle);
            ApHealthContainer.Add(healthProgressBar);
            healthProgressBar.title = "Health";

            ProgressBar apProgressBar = new();
            apProgressBar.ClearClassList();
            apProgressBar.AddToClassList(UiOverlayHelper.HudProgressStyle);
            ApHealthContainer.Add(apProgressBar);
            apProgressBar.title = "Action Points";
        }
    }
}