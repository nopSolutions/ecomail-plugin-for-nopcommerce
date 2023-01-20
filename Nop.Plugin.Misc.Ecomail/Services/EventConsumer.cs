using System.Threading.Tasks;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Events;

namespace Nop.Plugin.Misc.Ecomail.Services
{
    /// <summary>
    /// Represents plugin event consumer
    /// </summary>
    public class EventConsumer :
        IConsumer<EmailUnsubscribedEvent>,
        IConsumer<EmailSubscribedEvent>,
        IConsumer<EntityInsertedEvent<ShoppingCartItem>>,
        IConsumer<EntityUpdatedEvent<ShoppingCartItem>>,
        IConsumer<EntityDeletedEvent<ShoppingCartItem>>,
        IConsumer<OrderPlacedEvent>
    {
        #region Fields

        private readonly EcomailService _ecomailService;

        #endregion

        #region Ctor

        public EventConsumer(EcomailService ecomailService)
        {
            _ecomailService = ecomailService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handle the email unsubscribed event
        /// </summary>
        /// <param name="eventMessage">The event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EmailUnsubscribedEvent eventMessage)
        {
            await _ecomailService.UnsubscribeContactAsync(eventMessage.Subscription);
        }

        /// <summary>
        /// Handle the email subscribed event
        /// </summary>
        /// <param name="eventMessage">The event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EmailSubscribedEvent eventMessage)
        {
            await _ecomailService.SubscribeContactAsync(eventMessage.Subscription);
        }

        /// <summary>
        /// Handle the add shopping cart item event
        /// </summary>
        /// <param name="eventMessage">The event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityInsertedEvent<ShoppingCartItem> eventMessage)
        {
            if (eventMessage.Entity.ShoppingCartType == ShoppingCartType.ShoppingCart)
                await _ecomailService.HandleShoppingCartChangedEventAsync(eventMessage.Entity);
        }

        /// <summary>
        /// Handle the update shopping cart item event
        /// </summary>
        /// <param name="eventMessage">The event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<ShoppingCartItem> eventMessage)
        {
            if (eventMessage.Entity.ShoppingCartType == ShoppingCartType.ShoppingCart)
                await _ecomailService.HandleShoppingCartChangedEventAsync(eventMessage.Entity);
        }

        /// <summary>
        /// Handle the delete shopping cart item event
        /// </summary>
        /// <param name="eventMessage">The event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<ShoppingCartItem> eventMessage)
        {
            if (eventMessage.Entity.ShoppingCartType == ShoppingCartType.ShoppingCart)
                await _ecomailService.HandleShoppingCartChangedEventAsync(eventMessage.Entity);
        }

        /// <summary>
        /// Handle the order placed event
        /// </summary>
        /// <param name="eventMessage">The event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            await _ecomailService.HandleOrderPlacedEventAsync(eventMessage.Order);
        }

        #endregion
    }
}