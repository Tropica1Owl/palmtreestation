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
    }
}
