using Content.Server.Palmtree.Surgery;
using Content.Server.Popups;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Damage;

// It's all very crude at the moment, made just for tests n' stuff, it has no real functionality at the moment.
// I know the code is bad but I'm focusing atm to get myself acquainted with the engine, which has been going pretty well.

namespace Content.Server.Palmtree.Surgery.SurgerySystem
{
    public class PSurgerySystem : EntitySystem
    {
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly DamageableSystem _damageableSystem = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<PSurgeryToolComponent, AfterInteractEvent>(OnAfterInteract);
        }
        private void OnAfterInteract(EntityUid uid, PSurgeryToolComponent tool, AfterInteractEvent args)
        {
            if (!args.CanReach || args.Target == null || args.User == args.Target || !TryComp(args.Target, out PPatientComponent? patient))
            {
                _popupSystem.PopupEntity("You cannot perform surgery on this!", args.User, PopupType.Small);
                return;
            }
            switch(tool.kind)
            {
                case "scalpel":
                    if (patient.incised)
                    {
                        _popupSystem.PopupEntity("Patient already has an incision!", args.User, args.User, PopupType.Small);
                    }
                    else
                    {
                        _popupSystem.PopupEntity("You perform an incision!", args.User, args.User, PopupType.Small);
                        patient.incised = true;
                    }
                    break;
                case "retractor":
                    if (patient.retracted)
                    {
                        _popupSystem.PopupEntity("Patient's skin was already retracted!", args.User, args.User, PopupType.Small);
                    }
                    {
                        _popupSystem.PopupEntity("You retract the patient's skin!", args.User, args.User, PopupType.Small);
                        patient.retracted = true;
                    }
                    break;
                case "hemostat":
                    if (patient.clamped)
                    {
                        if (TryComp(uid, out PTendWoundsComponent? tendwounds)) // The moment we add more surgeries it's gonna bug the hell outta this, hopefully I'll have the code changed by then.
                        {
                            _popupSystem.PopupEntity("You tend some of the patient's wounds!", args.User, args.User, PopupType.Small);
                            _damageableSystem.TryChangeDamage(args.Target, tendwounds.healThisMuch, true, origin: uid);
                        }
                        _popupSystem.PopupEntity("Patient's bleeders were already clamped!", args.User, args.User, PopupType.Small);
                    }
                    {
                        _popupSystem.PopupEntity("You clamp the patient's bleeders", args.User, args.User, PopupType.Small);
                        patient.clamped = true;
                    }
                    break;
                case "cautery":
                    _popupSystem.PopupEntity("You cauterize and finish the surgical procedure", args.User, args.User, PopupType.Small);
                    patient.incised = false;
                    patient.retracted = false;
                    patient.clamped = false;
                    break;
            }
        }
    }
}
