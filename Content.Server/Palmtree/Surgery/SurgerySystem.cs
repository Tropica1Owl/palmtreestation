using System.Collections.Generic;
using System.Linq;
using Content.Server.Palmtree.Surgery;
using Content.Server.Popups;
using Content.Shared.Palmtree.Surgery;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;

// It's all very crude at the moment, but it actually works now.

namespace Content.Server.Palmtree.Surgery.SurgerySystem
{
    public class PSurgerySystem : SharedSurgerySystem
    {
        // These procedures can be prototypes later, for now I'll hardcode them because I have no idea how to do it otherwise.
        // Remind me to try and make them prototypes later, please!
        Dictionary<string, string[]> procedures = new Dictionary<string, string[]>
        {
            {"TendWounds", new string[]{"scalpel", "hemostat"}},
            {"SawBones", new string[]{"scalpel", "hemostat", "retractor"}}, // This comes before letting people saw bones
            {"BrainTransfer", new string[]{"scalpel", "hemostat", "retractor", "saw"}}
        };

        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly SharedMindSystem _mind = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PSurgeryToolComponent, AfterInteractEvent>(OnAfterInteract);
            SubscribeLocalEvent<MindExchangerComponent, AfterInteractEvent>(OnMindExchange);
            SubscribeLocalEvent<PSurgeryToolComponent, SurgeryDoAfterEvent>(OnProcedureFinished);
        }
        // Behavior for mind exchange chips, we won't touch organs
        private void OnMindExchange(EntityUid uid, MindExchangerComponent tool, AfterInteractEvent args)
        {
            if (args.User == args.Target || args.Target == null || !_mind.TryGetMind(args.User, out var mindId, out var mind) || TryComp(args.Target, out PPatientComponent? patient))
            {
                return;
            }
            _mind.TransferTo(mindId, args.Target, mind: mind);
        }
        private void OnProcedureFinished(EntityUid uid, PSurgeryToolComponent tool, SurgeryDoAfterEvent args)
        {
            if (args.Cancelled || args.Target == null || !TryComp(args.Target, out PPatientComponent? patient)) return;
            if (patient.procedures.Count == 0)
            {
                if (tool.kind == "scalpel")
                {
                    _popupSystem.PopupEntity("You successfully perform an incision!", args.User, PopupType.Small);
                    patient.procedures.Add("scalpel");
                    _damageableSystem.TryChangeDamage(args.Target, tool.damageOnUse, true, origin: uid);
                }
                else
                {
                    _popupSystem.PopupEntity("Perform an incision first!", args.User, PopupType.Small);
                }
            }
            else
            { // Procedure checks go here
                bool repeatableProcedure = false; // If it is repeatable it won't add to the "crafting" of the surgery
                bool damageOnFinish = true; // Check if it will damage the target by the damage specified in the prototypes
                switch(tool.kind)
                {
                    case "scalpel":
                        _popupSystem.PopupEntity("You perform an incision!", args.User, PopupType.Small);
                        break;
                    case "hemostat":
                        if (patient.procedures.SequenceEqual(procedures["TendWounds"]) && TryComp(uid, out PTendWoundsComponent? tendwounds))
                        {
                            repeatableProcedure = true;
                            damageOnFinish = false;
                            _damageableSystem.TryChangeDamage(args.Target, tendwounds.healThisMuch, true, origin: uid);
                            _popupSystem.PopupEntity("You tend to the patient's wounds!", args.User, PopupType.Small);
                        }
                        else
                        {
                            _popupSystem.PopupEntity("You clamp the patient's bleeders!", args.User, PopupType.Small);
                        }
                        break;
                    case "retractor":
                        _popupSystem.PopupEntity("You retract the patient's skin!", args.User, PopupType.Small);
                        break;
                    case "saw":
                        if (patient.procedures.SequenceEqual(procedures["SawBones"]))
                        {
                            _popupSystem.PopupEntity("You saw the patient's bones!", args.User, PopupType.Small);
                        }
                        else
                        {
                            _popupSystem.PopupEntity("You can't saw your patient's bones right now!", args.User, PopupType.Small);
                            damageOnFinish = false;
                            repeatableProcedure = true; // Technically you can repeat fail, yup.
                        }
                        break;
                    case "cautery":
                        patient.procedures.Clear();
                        repeatableProcedure = true; // You can just burn people over and over with a cautery, yeah.
                        break;
                    default:
                        _popupSystem.PopupEntity("Yo stupid ass forgot to give this tool a behavior check", args.User, PopupType.Small);
                        break;
                }
                if (!repeatableProcedure)
                {
                    patient.procedures.Add(tool.kind);
                }
                if (damageOnFinish)
                {
                    _damageableSystem.TryChangeDamage(args.Target, tool.damageOnUse, true, origin: uid);
                }
            }
        }
        private void OnAfterInteract(EntityUid uid, PSurgeryToolComponent tool, AfterInteractEvent args)
        {
            if (!args.CanReach || args.Target == null || args.User == args.Target || !TryComp(args.Target, out PPatientComponent? patient))
            {
                _popupSystem.PopupEntity("You cannot perform surgery on this!", args.User, PopupType.Small);
                return;
            }
            var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, 2.0f, new SurgeryDoAfterEvent(), uid, target: args.Target)
            {
                NeedHand = true,
                BreakOnMove = true,
                BreakOnWeightlessMove = true,
            };
            _doAfter.TryStartDoAfter(doAfterEventArgs);

        }
    }
}
