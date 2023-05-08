using p3ppc.colourfulPartyPanel.Configuration;
using p3ppc.colourfulPartyPanel.Template;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.Enums;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using System.Runtime.InteropServices;
using static p3ppc.colourfulPartyPanel.Utils;
using static p3ppc.colourfulPartyPanel.Colours;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;
using System.Diagnostics;

namespace p3ppc.colourfulPartyPanel
{
    /// <summary>
    /// Your mod logic goes here.
    /// </summary>
    public unsafe class Mod : ModBase // <= Do not Remove.
    {
        /// <summary>
        /// Provides access to the mod loader API.
        /// </summary>
        private readonly IModLoader _modLoader;

        /// <summary>
        /// Provides access to the Reloaded.Hooks API.
        /// </summary>
        /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
        private readonly IReloadedHooks? _hooks;

        /// <summary>
        /// Provides access to the Reloaded logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Entry point into the mod, instance that created this class.
        /// </summary>
        private readonly IMod _owner;

        /// <summary>
        /// Provides access to this mod's configuration.
        /// </summary>
        private Config _configuration;

        /// <summary>
        /// The configuration of the currently executing mod.
        /// </summary>
        private readonly IModConfig _modConfig;

        private IReverseWrapper<GetMemberColourBattleDelegate> _getMemberColourBattleReverseWrapper;
        private IReverseWrapper<GetMemberColourOverworldDelegate> _getMemberColourOverworldReverseWrapper;
        private IAsmHook _setHpOutlineColourHook;
        private IAsmHook _setIconOutlineColourHook;
        private PartyMember* _inParty;

        public Mod(ModContext context)
        {
            _modLoader = context.ModLoader;
            _hooks = context.Hooks;
            _logger = context.Logger;
            _owner = context.Owner;
            _configuration = context.Configuration;
            _modConfig = context.ModConfig;

            Initialise(_logger, _configuration);

            var startupScannerController = _modLoader.GetController<IStartupScanner>();
            if (startupScannerController == null || !startupScannerController.TryGetTarget(out var startupScanner))
            {
                LogError($"Unable to get controller for Reloaded SigScan Library, stuff won't work :(");
                return;
            }

            string getMemberColour = _hooks.Utilities.GetAbsoluteCallMnemonics(GetMemberColourBattle, out _getMemberColourBattleReverseWrapper);

            startupScanner.AddMainModuleScan("48 89 05 ?? ?? ?? ?? 48 8D 89 ?? ?? ?? ??", result =>
            {
                if (!result.Found)
                {
                    LogError($"Unable to find InParty, stuff won't work :(");
                    return;
                }
                LogDebug($"Found InParty pointer at 0x{result.Offset + BaseAddress:X}");
                _inParty = (PartyMember*)GetGlobalAddress(result.Offset + BaseAddress + 3);
                LogDebug($"Found InParty pointer at 0x{(nuint)_inParty:X}");
            });

            startupScanner.AddMainModuleScan("E8 ?? ?? ?? ?? 33 C9 E8 ?? ?? ?? ?? 45 33 C9 C7 44 24 ?? 00 00 00 00 33 C9 41 8D 51 ?? 45 8D 41 ?? E8 ?? ?? ?? ?? C6 44 24 ?? FF", result =>
            {
                if (!result.Found)
                {
                    LogError($"Unable to find BattlePanelHpOutlineColour, stuff won't work :(");
                    return;
                }
                LogDebug($"Found BattlePanelHpOutlineColour at 0x{result.Offset + BaseAddress:X}");

                string[] function =
                {
                        "use64",
                        "push rax\npush rcx\npush rdx\npush r8\npush r9\npush r11",
                        "mov rcx, rsi",
                        "sub rsp, 32",
                        $"{getMemberColour}",
                        "add rsp, 32",
                        // Set the returned colour into the colour for the render call
                        "mov [rsp+0x58], al",
                        "shr eax, 8",
                        "mov [rsp+0x60], al",
                        "shr eax, 8",
                        "mov [rsp+0x68], al",
                        "shr eax, 8",
                        "mov [rsp+0x70], al",
                        "pop r11\npop r9\npop r8\npop rdx\npop rcx\npop rax"
                    };
                _setHpOutlineColourHook = _hooks.CreateAsmHook(function, result.Offset + BaseAddress, AsmHookBehaviour.ExecuteFirst).Activate();
            });

            startupScanner.AddMainModuleScan("E8 ?? ?? ?? ?? 33 C9 E8 ?? ?? ?? ?? 45 33 C9 89 5C 24 ?? 8D 53 ??", result =>
            {
                if (!result.Found)
                {
                    LogError($"Unable to find BattlePanelIconOutlineColour, stuff won't work :(");
                    return;
                }
                LogDebug($"Found BattlePanelIconOutlineColour at 0x{result.Offset + BaseAddress:X}");

                string[] function =
                {
                        "use64",
                        "push rax\npush rcx\npush rdx\npush r8\npush r9\npush r11",
                        "mov rcx, rsi",
                        "sub rsp, 32",
                        $"{getMemberColour}",
                        "add rsp, 32",
                        // Set the returned colour into the colour for the render call
                        "mov [rsp+0x58], al",
                        "shr eax, 8",
                        "mov [rsp+0x60], al",
                        "shr eax, 8",
                        "mov [rsp+0x68], al",
                        "shr eax, 8",
                        "mov [rsp+0x70], al",
                        "pop r11\npop r9\npop r8\npop rdx\npop rcx\npop rax"
                    };
                _setIconOutlineColourHook = _hooks.CreateAsmHook(function, result.Offset + BaseAddress, AsmHookBehaviour.ExecuteFirst).Activate();
            });

            startupScanner.AddMainModuleScan("E8 ?? ?? ?? ?? 45 33 C9 C7 44 24 ?? 00 00 00 00 33 C9 41 8D 51 ?? 45 8D 41 ?? E8 ?? ?? ?? ?? 48 8D 8E ?? ?? ?? ??", result =>
            {
                if (!result.Found)
                {
                    LogError($"Unable to find PartyPanelOutlineColour, stuff won't work :(");
                    return;
                }
                LogDebug($"Found PartyPanelOutlineColour at 0x{result.Offset + BaseAddress:X}");

                string[] function =
                {
                        "use64",
                        "push rax\npush rcx\npush rdx\npush r8\npush r9\npush r11",
                        "mov rcx, rbp",
                        "sub rsp, 32",
                        $"{_hooks.Utilities.GetAbsoluteCallMnemonics(GetMemberColourOverworld, out _getMemberColourOverworldReverseWrapper)}",
                        "add rsp, 32",
                        "pop r11\npop r9\npop r8\npop rdx\npop rcx",
                        // Set the returned colour into the colour for the render call
                        "mov dword [rcx+44], eax",
                        "pop rax"
                    };
                _setIconOutlineColourHook = _hooks.CreateAsmHook(function, result.Offset + BaseAddress, AsmHookBehaviour.ExecuteFirst).Activate();
            });
        }

        private Colour GetMemberColourBattle(MemberInfoOuter* info)
        {
            PartyMember member = info->MemberInfo->Member;

            if (member == PartyMember.Protag)
                return maleColour;
            return memberColours[(short)member - 2];
        }

        private Colour GetMemberColourOverworld(int memberSlot)
        {
            PartyMember member = PartyMember.Protag;
            if(memberSlot > 0)
                member = _inParty[memberSlot - 1];

            if (member <= PartyMember.Protag)
                return maleColour;
            return memberColours[(short)member - 2];
        }

        private delegate Colour GetMemberColourBattleDelegate(MemberInfoOuter* info);
        private delegate Colour GetMemberColourOverworldDelegate(int memberSlot);

        [StructLayout(LayoutKind.Explicit)]
        private struct MemberInfoOuter
        {
            [FieldOffset(3296)]
            internal BattlePatyMemberInfo* MemberInfo;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct BattlePatyMemberInfo
        {
            [FieldOffset(2)]
            internal PartyMember Member;

            [FieldOffset(8)]
            internal short Hp;

            [FieldOffset(10)]
            internal short Sp;
        }

        #region Standard Overrides
        public override void ConfigurationUpdated(Config configuration)
        {
            // Apply settings from configuration.
            // ... your code here.
            _configuration = configuration;
            _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");
        }
        #endregion

        #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Mod() { }
#pragma warning restore CS8618
        #endregion
    }
}