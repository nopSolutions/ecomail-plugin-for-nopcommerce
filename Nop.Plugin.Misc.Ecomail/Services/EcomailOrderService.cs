using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Misc.Ecomail.Domain;

namespace Nop.Plugin.Misc.Ecomail.Services
{
    /// <summary>
    /// Represents the service to manage orders
    /// </summary>
    public class EcomailOrderService
    {
        #region Fields

        private readonly IRepository<EcomailOrder> _orderRepository;

        #endregion

        #region Ctor

        public EcomailOrderService(IRepository<EcomailOrder> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get orders
        /// </summary>
        /// <param name="apiKey">Search by API key</param>
        /// <param name="transactionId">Search by transaction identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains orders
        /// </returns>
        public async Task<List<EcomailOrder>> GetOrdersAsync(string apiKey = null, string transactionId = null)
        {
            var query = _orderRepository.Table;

            if (!string.IsNullOrEmpty(apiKey))
                query = query.Where(order => order.ApiKey == apiKey);

            if (!string.IsNullOrEmpty(transactionId))
                query = query.Where(order => order.TransactionId == transactionId);

            query = query.OrderByDescending(order => order.CreatedOnUtc).ThenBy(order => order.Id);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Insert orders
        /// </summary>
        /// <param name="orderIds">List of order identifiers</param>
        /// <param name="apiKey">API key</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InsertOrdersAsync(List<int> orderIds, string apiKey)
        {
            var orders = orderIds
                .Select(id => new EcomailOrder { OrderId = id, ApiKey = apiKey, CreatedOnUtc = DateTime.UtcNow })
                .ToList();
            await _orderRepository.InsertAsync(orders, false);
        }

        /// <summary>
        /// Get order by id
        /// </summary>
        /// <param name="orderId">Order identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order
        /// </returns>
        public async Task<EcomailOrder> GetOrderByIdAsync(int orderId)
        {
            return await _orderRepository.GetByIdAsync(orderId, cache => default);
        }

        /// <summary>
        /// Insert order
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InsertOrderAsync(EcomailOrder order)
        {
            await _orderRepository.InsertAsync(order, false);
        }

        /// <summary>
        /// Update order
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task UpdateOrderAsync(EcomailOrder order)
        {
            await _orderRepository.UpdateAsync(order, false);
        }

        /// <summary>
        /// Delete order
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task DeleteOrderAsync(EcomailOrder order)
        {
            await _orderRepository.DeleteAsync(order, false);
        }

        #endregion
    }
}