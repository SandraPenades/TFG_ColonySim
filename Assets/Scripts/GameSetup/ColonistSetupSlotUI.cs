using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ColonistSetupSlotUI : MonoBehaviour
{
    [Header("Datos del colono")]
    [SerializeField] private TMP_InputField nameInput;

    [Header("Vista previa de apariencia")]
    [SerializeField] private Image skinPreviewImage;

    private CharacterSkinData[] availableSkins;
    private int currentSkinIndex;

    public void Initialize(CharacterSkinData[] skins)
    {
        availableSkins = skins;
        currentSkinIndex = 0;

        RefreshVisuals();
    }

    public void NextSkin()
    {
        currentSkinIndex++;

        if (currentSkinIndex >= availableSkins.Length) currentSkinIndex = 0;

        RefreshVisuals();
    }

    public void PreviousSkin()
    {
        currentSkinIndex--;

        if (currentSkinIndex < 0) currentSkinIndex = availableSkins.Length - 1;

        RefreshVisuals();
    }

    public string GetColonistName()
    {
        string value = nameInput.text.Trim();

        if (string.IsNullOrEmpty(value)) return "Colono";

        return value;
    }

    public CharacterSkinData GetSelectedSkin()
    {
        return availableSkins[currentSkinIndex];
    }

    private void RefreshVisuals()
    {
        if (availableSkins == null || availableSkins.Length == 0) return;

        CharacterSkinData selectedSkin = availableSkins[currentSkinIndex];

        if (skinPreviewImage != null)
        {
            skinPreviewImage.sprite = selectedSkin.portrait != null ? selectedSkin.portrait : selectedSkin.idlePreview;
        }
    }
}
