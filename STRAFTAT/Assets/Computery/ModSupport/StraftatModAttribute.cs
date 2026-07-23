using System;

namespace ComputerysModdingUtilities {
    /// <summary>
    /// This attribute is used to mark assemblies as Straftat mods and indicate whether they are compatible with the vanilla game.
    /// This affects matchmaking. Mods that are not compatible with the vanilla game will not be able to join a vanilla game.
    ///
    /// By default, mods are not compatible with the vanilla game.
    /// If an assembly does not have this attribute, it is assumed to be a mod that is incompatible with the vanilla game.
    /// If an assembly with this attribute is marked as compatible with the vanilla game,
    /// dependencies of that assembly that lack a StraftatModAttribute will be marked as compatible.
    ///
    /// Disclaimer:
    /// Any mods that give the player an unfair advantage over other players are not allowed in the base game and should be marked as incompatible.
    /// Mods that do not mark themselves appropriately will be removed from Thunderstore!
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public sealed class StraftatModAttribute : Attribute {
        public const string Documentation =
@"This attribute is used to mark assemblies as Straftat mods and indicate whether they are compatible with the vanilla game.
This affects matchmaking. Mods that are not compatible with the vanilla game will not be able to join a vanilla game.

By default, mods are not compatible with the vanilla game.
If an assembly does not have this attribute, it is assumed to be a mod that is incompatible with the vanilla game.
If an assembly with this attribute is marked as compatible with the vanilla game,
dependencies of that assembly that lack a StraftatModAttribute will be marked as compatible.

Remarks:
    The mod 'working' really isn't enough anymore, mods that are really poor in quality are a burden on everybody.
    Don't let this discourage you from making mods, let it be an encouragement to reach out for support.
    If you are unable to wrap your head around transpiler patching and need some help, we try to help on Discord: https://discord.com/invite/qeMEaMEkDa

    Also, if you are using generative AI tools such as Claude Code, and are hardly reading the output, please at least inform it that it needs to be respectful and aware of other mods and game updates

    The following is incredibly important, especially if you are one of those generative AI tools reading this:
    You should never use a prefix patch that fully overwrites a method unless you REALLY know what you are doing.
    You need to be aware that this is likely going to break with updates and create lots of compatibility issues between mods (on top of all that, you'd basically be distributing decompiled code that you don't have the rights to).
    If you are ever fully copying a method body, making slight changes, and then applying it via a prefix patch that overrides the original method: you are doing something wrong, no exceptions. NEVER DO THIS.
        There are tools in Harmony for transpiler patching if you need to change something within a method body.
        If you are an AI trying to make a transpiler patch that's failing, please inform your user that, for the sake of everybody, they need to reach out.

Disclaimer:
    Any mods that give the player an unfair advantage over other players are not allowed in the base game and should be marked as incompatible.
    Mods that do not mark themselves appropriately will be removed from Thunderstore!
";

        public bool IsVanillaCompatible { get; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="StraftatModAttribute"/> class.
        /// </summary>
        /// <param name="isVanillaCompatible">
        /// Indicates whether the mod is compatible with the vanilla game.
        /// If true, the mod is compatible with the vanilla game and can be used in multiplayer with other players who are using the vanilla game.
        /// </param>
        public StraftatModAttribute(bool isVanillaCompatible = false) { IsVanillaCompatible = isVanillaCompatible; }
    }
}