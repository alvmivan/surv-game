using System;
using UnityEngine;

namespace FPSGame.Player
{
    [Flags]
    public enum CharacterAbilities
    {
        None       = 0,
        Movement   = 1 << 0,
        Jump       = 1 << 1,
        Crouch     = 1 << 2,
        Sprint     = 1 << 3,
        Look       = 1 << 4,
    }

    [CreateAssetMenu(menuName = "FPSGame/Character/Character Setup")]
    public class CharacterSetup : ScriptableObject
    {
        [Header("Identity")]
        public string CharacterName = "New Character";

        [Header("Model")]
        public GameObject ModelPrefab;

        [Header("Abilities")]
        public CharacterAbilities Abilities = CharacterAbilities.Movement | CharacterAbilities.Jump | CharacterAbilities.Look;

        [Header("Movement Config")]
        public PlayerConfig MovementConfig;

        [Header("Physics")]
        public float Height = 2f;
        public float Radius = 0.5f;
    }
}
