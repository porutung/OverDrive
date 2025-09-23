using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoView : View_Base<PlayerInfoViewModel>
{
    [SerializeField][Bind("SpeedText")] TextMeshProUGUI _speedText;
    [SerializeField][Bind("ComboText")] TextMeshProUGUI _comboText;
    [SerializeField][Bind("Fuel")] Slider _comboComboText;
    [SerializeField][Bind("NitroButton")] Button _nitroButton;
    private PlayerInfoViewModel model;
    protected override void BindViewModel()
    {
        model = ViewModel as PlayerInfoViewModel;
        
        model.OnChangeSpeedText += UpdateSpeedText;
        model.OnChangeComboText += UpdateComboText;
        model.OnChangeFuelText += UpdateFuelAmount;
        model.OnActiveNitro += UpdateActiveNitroButton;
        
        _nitroButton.onClick.AddListener(model.OnClickNitroBoost);
    }

    protected override void UnbindViewModel()
    {
        model.OnChangeSpeedText -= UpdateSpeedText;
        model.OnChangeComboText -= UpdateComboText;
        model.OnChangeFuelText -= UpdateFuelAmount;
        model.OnActiveNitro -= UpdateActiveNitroButton;
        
        _nitroButton.onClick.RemoveListener(model.OnClickNitroBoost);
    }

    private void UpdateSpeedText(string speed)
    {
        _speedText.text = speed;
    }

    private void UpdateComboText(string combo)
    {
        _comboText.text = combo;
    }

    private void UpdateFuelAmount(float amount)
    {
        _comboComboText.value = amount;
    }

    private void UpdateActiveNitroButton(bool active)
    {
        _nitroButton.interactable = active;
        _nitroButton.gameObject.SetActive(active);
    }
}
