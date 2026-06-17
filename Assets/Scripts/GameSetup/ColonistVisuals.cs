using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColonistVisuals : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private CharacterSkinData currentSkin;

    public void ApplySkin(CharacterSkinData skin)
    {
        if (skin == null) return;

        currentSkin = skin;

        if (animator != null && skin.animatorOverrideController != null)
        {
            animator.runtimeAnimatorController = skin.animatorOverrideController;
        }

        if (spriteRenderer != null && skin.idlePreview != null)
        {
            spriteRenderer.sprite = skin.idlePreview;
        }
    }

    public CharacterSkinData GetCurrentSkin()
    {
        return currentSkin;
    }
}
