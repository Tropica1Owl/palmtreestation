using Content.Shared.Damage;

namespace Content.Server.Palmtree.Surgery
{
    [RegisterComponent]
    public partial class PSurgeryToolComponent : Component
    {
        [DataField("kind")]
        [ViewVariables(VVAccess.ReadWrite)]
        public string kind = "scalpel";

        [DataField("infectionDamage")]
        [ViewVariables(VVAccess.ReadWrite)]
        public float infectionDamage = 1.0f;

        [DataField("damageOnUse", required: true)] // Tools damage the patient on use except in special cases.
        [ViewVariables(VVAccess.ReadWrite)]
        public DamageSpecifier damageOnUse = default!;
    }
}
