namespace Content.Server.Palmtree.Surgery
{
    [RegisterComponent]
    public partial class PSurgery : Component //"PSurgery" because wizden might add surgery down the line, so I'm doing this to avoid conflicts.
    {// I'll make this better later with a proper list of steps, I just need a first version for now
        [DataField("incised")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool count = false;

        [DataField("retracted")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool count = false;

        [DataField("clamped")]
        [ViewVariables(VVAccess.ReadWrite)]
        public bool count = false;
    }
}
