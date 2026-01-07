using System;
using System.Collections.Generic;
using System.Text;

namespace PaperTanksV2Client.GameEngine
{
    public enum GameAction
    {
        // Movement actions
        Left,
        Right,
        Up,
        Down,

        // Combat actions
        PrimaryAttack,
        SecondaryAttack,
        Reload,

        // Interaction actions
        Interact,
        UseItem,
        SwitchWeapon,

        // Special actions
        Dodge,
        Block,
        SpecialAbility,

        // UI actions
        OpenInventory,
        OpenMap,

        // Analog actions (for AnalogActions dictionary)
        WeaponCharge,    // How long attack button is held
        MovementSpeed,   // Walking vs running
        AimIntensity    // For weapons with variable power
    }
}
