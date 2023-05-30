namespace Nop.Plugin.Misc.Ecomail.Domain
{
    /// <summary>
    /// Represents order event type enumeration
    /// </summary>
    public enum OrderEventType
    {
        /// <summary>
        /// Transaction request should be sent when order is placed
        /// </summary>
        Placed = 0,

        /// <summary>
        /// Transaction request should be sent when order is paid
        /// </summary>
        Paid = 10
    }
}