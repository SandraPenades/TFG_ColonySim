using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Colony Sim/Character Skin")]
public class CharacterSkinData : ScriptableObject
{
    [Header("Identificación")]
    public string skinId;

    [Header("Sprites de interfaz")]
    public Sprite portrait;
    public Sprite idlePreview;

    [Header("Animaciones")]
    public AnimatorOverrideController animatorOverrideController;
}
