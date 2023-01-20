namespace Nop.Plugin.Misc.Ecomail.Domains
{
    /// <summary>
    /// Represents contact status enumeration
    /// </summary>
    public enum ContactStatus
    {
        /// <summary>
        /// Subscribed to a contact list
        /// </summary>
        Subscribed = 1,

        /// <summary>
        /// Unsubscribed from contact list
        /// </summary>
        Unsubscribed = 2,

        /// <summary>
        /// Soft bounced
        /// </summary>
        SoftBounce = 3,

        /// <summary>
        /// Hard bounced
        /// </summary>
        HardBounce = 4,

        /// <summary>
        /// SPAM complaint
        /// </summary>
        SpamComplaint = 5,

        /// <summary>
        /// Unconfirmed
        /// </summary>
        Unconfirmed = 6
    }
}