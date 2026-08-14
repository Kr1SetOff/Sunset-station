namespace Content.Server.Bible.Components
{
    [RegisterComponent]
    public sealed partial class BibleUserComponent : Component
    {
        /// <summary>
        /// The nullrod this bible user has bound to themselves (see NullrodSystem's bind verb),
        /// used by the recall-at-altar prayer to know which entity to summon back to their hand.
        /// </summary>
        [DataField]
        public EntityUid? NullRod;
    }
}
