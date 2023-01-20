﻿using System.Threading.Tasks;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.Ecomail.Services
{
    /// <summary>
    /// Represents a schedule task to synchronize contacts
    /// </summary>
    public class SynchronizationTask : IScheduleTask
    {
        #region Fields

        private readonly EcomailService _ecomailSubscriberService;

        #endregion

        #region Ctor

        public SynchronizationTask(EcomailService ecomailSubscriberService)
        {
            _ecomailSubscriberService = ecomailSubscriberService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Execute task
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task ExecuteAsync()
        {
            await _ecomailSubscriberService.SynchronizeAsync();
        }

        #endregion
    }
}