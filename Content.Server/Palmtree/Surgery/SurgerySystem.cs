using System.Collections.Generic;
using Content.Server.Palmtree.Surgery;
using Content.Server.Popups;
using Content.Shared.Palmtree.Surgery;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;

// It's all very crude at the moment, but it actually works now.

namespace Content.Server.Palmtree.Surgery.SurgerySystem
{
    public class PSurgerySystem : SharedSurgerySystem
    {
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PSurgeryToolComponent, AfterInteractEvent>(OnAfterInteract);
            SubscribeLocalEvent<PSurgeryToolComponent, SurgeryDoAfterEvent>(OnProcedureFinished);
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
                }
                else
                {
                    _popupSystem.PopupEntity("Perform an incision first!", args.User, PopupType.Small);
                }
            }
            else
            {
                if (!patient.procedures.Contains(tool.kind))
                {
                    // Future code for specific procedures here, for now it's just a bogus handler
                    if (tool.kind == "cautery")
                    {
                        _popupSystem.PopupEntity("You tend to the patient's wounds.", args.User, PopupType.Small);
                        patient.procedures.Clear();
                    }
                    else
                    {
                        patient.procedures.Add(tool.kind);
                        _popupSystem.PopupEntity("You use " + tool.kind + " on the patient.", args.User, PopupType.Small);
                    }
                }
                else
                { // Add the code for surgeries that perform repeated steps here
                    if (tool.kind == "hemostat" && TryComp(uid, out PTendWoundsComponent? tendwounds))
                    {
                        _popupSystem.PopupEntity("You tend to the patient's wounds.", args.User, PopupType.Small);
                        _damageableSystem.TryChangeDamage(args.Target, tendwounds.healThisMuch, true, origin: uid);
                    }
                    else
                    {
                        _popupSystem.PopupEntity("You already did this!", args.User, PopupType.Small);
                    }
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
