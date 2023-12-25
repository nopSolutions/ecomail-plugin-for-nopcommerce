using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Misc.Ecomail.Domain;
using Nop.Plugin.Misc.Ecomail.Domain.Api;
using Nop.Plugin.Misc.Ecomail.Domain.Api.Tracking;
using Nop.Plugin.Misc.Ecomail.Domain.Api.Webhook;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Tax;

namespace Nop.Plugin.Misc.Ecomail.Services
{
    /// <summary>
    /// Represents plugin service
    /// </summary>
    public class EcomailService
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly EcomailHttpClient _ecomailHttpClient;
        private readonly EcomailOrderService _ecomailOrderService;
        private readonly EcomailSettings _ecomailSettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IAddressService _addressService;
        private readonly ICategoryService _categoryService;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IGdprService _gdprService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger _logger;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly INopFileProvider _nopFileProvider;
        private readonly IOrderService _orderService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<GenericAttribute> _genericAttributeRepository;
        private readonly IRepository<NewsLetterSubscription> _newsLetterSubscriptionRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly ITaxService _taxService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public EcomailService(CurrencySettings currencySettings,
            EcomailHttpClient ecomailHttpClient,
            EcomailOrderService ecomailOrderService,
            EcomailSettings ecomailSettings,
            IActionContextAccessor actionContextAccessor,
            IAddressService addressService,
            ICategoryService categoryService,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IGdprService gdprService,
            IGenericAttributeService genericAttributeService,
            ILogger logger,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            INopFileProvider nopFileProvider,
            IOrderService orderService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPictureService pictureService,
            IProductService productService,
            IRepository<Address> addressRepository,
            IRepository<Country> countryRepository,
            IRepository<Customer> customerRepository,
            IRepository<GenericAttribute> genericAttributeRepository,
            IRepository<NewsLetterSubscription> newsLetterSubscriptionRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IStoreService storeService,
            ITaxService taxService,
            IUrlHelperFactory urlHelperFactory,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            IWorkContext workContext)
        {
            _currencySettings = currencySettings;
            _ecomailHttpClient = ecomailHttpClient;
            _ecomailOrderService = ecomailOrderService;
            _ecomailSettings = ecomailSettings;
            _actionContextAccessor = actionContextAccessor;
            _addressService = addressService;
            _categoryService = categoryService;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerService = customerService;
            _gdprService = gdprService;
            _genericAttributeService = genericAttributeService;
            _logger = logger;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _nopFileProvider = nopFileProvider;
            _orderService = orderService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _pictureService = pictureService;
            _productService = productService;
            _addressRepository = addressRepository;
            _countryRepository = countryRepository;
            _customerRepository = customerRepository;
            _genericAttributeRepository = genericAttributeRepository;
            _newsLetterSubscriptionRepository = newsLetterSubscriptionRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _storeService = storeService;
            _taxService = taxService;
            _urlHelperFactory = urlHelperFactory;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _workContext = workContext;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Handle function and get result
        /// </summary>
        /// <typeparam name="TResult">Result type</typeparam>
        /// <param name="function">Function</param>
        /// <param name="logErrors">Whether to log errors</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result; error if exists
        /// </returns>
        private async Task<(TResult Result, string Error)> HandleFunctionAsync<TResult>(Func<Task<TResult>> function, bool logErrors = true)
        {
            try
            {
                return (await function(), default);
            }
            catch (Exception exception)
            {
                var errorMessage = exception.Message;
                if (logErrors)
                {
                    var logMessage = $"{EcomailDefaults.SystemName} error: {Environment.NewLine}{errorMessage}";
                    await _logger.ErrorAsync(logMessage, exception, await _workContext.GetCurrentCustomerAsync());
                }

                return (default, errorMessage);
            }
        }

        /// <summary>
        /// Get specific contact list 
        /// </summary>
        /// <param name="listId">Contact list id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains a contact list
        /// </returns>
        private async Task<EcomailContactListDetailsResponse> GetListByIdAsync(int listId)
        {
            if (listId < 1)
                return null;

            try
            {
                var response = await _ecomailHttpClient
                    .RequestAsync(string.Format(EcomailDefaults.GetListApiUrl, listId), null, HttpMethod.Get);
                var contactListDetails = JsonConvert.DeserializeObject<EcomailContactListDetailsResponse>(response);
                return contactListDetails;
            }
            catch (Exception ex)
            {
                throw new NopException($"Failed to get contact list #{listId}. {ex.Message}");
            }
        }

        /// <summary>
        /// Import all contacts from the store to Ecomail account
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of sync messages
        /// </returns>
        private async Task<List<(NotifyType Type, string Message)>> ImportContactsAsync()
        {
            var messages = new List<(NotifyType, string)>();
            var importedContacts = 0;
            try
            {
                if (string.IsNullOrEmpty(_ecomailSettings.ApiKey))
                    throw new NopException("Plugin not configured");

                //ensure list exists
                var list = await GetListByIdAsync(_ecomailSettings.ListId)
                    ?? throw new NopException("Contact list to synchronize not set");

                var store = await _storeContext.GetCurrentStoreAsync();

                //first sync a single contact to create custom fields
                var subscription = (await _newsLetterSubscriptionService
                    .GetAllNewsLetterSubscriptionsAsync(storeId: store.Id, isActive: true, pageSize: 1))
                    .FirstOrDefault();
                await SubscribeContactAsync(subscription);

                var pageIndex = 0;
                var pageSize = _ecomailSettings.SyncPageSize;
                while (true)
                {
                    var subscribers = await PrepareSubscribersAsync(store.Id, pageIndex, pageSize);
                    if (!_ecomailSettings.SyncSubscribersOnly)
                    {
                        var customers = PrepareCustomers(store.Id, pageIndex, pageSize)
                            .Where(customer => !subscribers.Any(subscriber => subscriber.Email == customer.Email))
                            .ToList();
                        subscribers.AddRange(customers);
                    }
                    if (!subscribers.Any())
                        break;

                    var subscribersAddOnBulkRequest = new SubscribersAddOnBulkRequest
                    {
                        SubscriberDataList = subscribers,
                        UpdateExisting = true
                    };

                    var payload = JsonConvert.SerializeObject(subscribersAddOnBulkRequest);
                    try
                    {
                        var response = await _ecomailHttpClient
                            .RequestAsync(string.Format(EcomailDefaults.SubscribeInBulkApiUrl, _ecomailSettings.ListId), payload, HttpMethod.Post);
                    }
                    catch (Exception ex)
                    {
                        throw new NopException($"Failed to import contacts to list #{_ecomailSettings.ListId}. {ex.Message}");
                    }

                    if (_ecomailSettings.ImportOrdersOnSync)
                    {
                        var importedOrders = 0;
                        try
                        {
                            var emails = subscribers.Select(subscriber => subscriber.Email).Distinct().ToList();
                            var orders = await PrepareOrdersAsync(emails, store.Id);
                            var ordersPageIndex = 0;
                            var ordersPageSize = 1000;
                            while (true)
                            {
                                //we can import 1000 transactions in one request
                                var transactions = orders.Skip(ordersPageIndex * ordersPageSize).Take(ordersPageSize).ToList();
                                if (!transactions.Any())
                                    break;

                                //set them as synced
                                await _ecomailOrderService
                                    .InsertOrdersAsync(transactions.Select(item => item.TransactionData.OrderId).ToList(), _ecomailSettings.ApiKey);

                                //import
                                var ordersRequest = JsonConvert.SerializeObject(new TransactionsRequest { Transactions = transactions });
                                var response = await _ecomailHttpClient
                                    .RequestAsync(EcomailDefaults.AddTransactionsApiUrl, ordersRequest, HttpMethod.Post);
                                ordersPageIndex++;
                                importedOrders += transactions.Count;
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new NopException($"Failed to import some orders of contacts to list #{_ecomailSettings.ListId}. {ex.Message}");
                        }
                        messages.Add((NotifyType.Success, $"Synchronization info: {importedOrders} orders of contacts have been imported"));
                    }

                    importedContacts += subscribers.Count;
                    pageIndex++;
                }

                messages.Add((NotifyType.Success, $"Synchronization info: {importedContacts} contacts have been imported to list '{list.ContactListInfo?.Name}'"));
            }
            catch (Exception exception)
            {
                messages.Add((NotifyType.Success, $"Synchronization info: {importedContacts} contacts have been imported"));
                messages.Add((NotifyType.Error, $"Synchronization error: {exception.Message}"));
                var logMessage = $"{EcomailDefaults.SystemName} error: {Environment.NewLine}{exception.Message}";
                await _logger.ErrorAsync(logMessage, exception, await _workContext.GetCurrentCustomerAsync());
            }

            return messages;
        }

        /// <summary>
        /// Export all contacts from Ecomail account to the store
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of sync messages
        /// </returns>
        private async Task<List<(NotifyType Type, string Message)>> ExportContactsAsync()
        {
            var messages = new List<(NotifyType, string)>();
            var contacts = 0;
            try
            {
                if (!_ecomailSettings.ExportContactsOnSync)
                    return new();

                if (string.IsNullOrEmpty(_ecomailSettings.ApiKey))
                    throw new NopException("Plugin not configured");

                //ensure list exists
                var list = await GetListByIdAsync(_ecomailSettings.ListId)
                    ?? throw new NopException("Contact list to synchronize not set");

                var store = await _storeContext.GetCurrentStoreAsync();

                var pageIndex = 1;
                var pageSize = _ecomailSettings.SyncPageSize;
                while (true)
                {
                    var url = string.Format(EcomailDefaults.GetSubscribersApiUrl, _ecomailSettings.ListId) + $"?per_page={pageSize}&page={pageIndex}";
                    var response = string.Empty;
                    try
                    {
                        response = await _ecomailHttpClient.RequestAsync(url, null, HttpMethod.Get);
                    }
                    catch (Exception ex)
                    {
                        throw new NopException($"Failed to export contacts from list #{_ecomailSettings.ListId}. {ex.Message}");
                    }
                    var responseValue = JsonConvert.DeserializeObject<SubscribersListResponse>(response);
                    if (responseValue?.SubscribersDataList is null || !responseValue.SubscribersDataList.Any())
                        break;

                    //subscribe
                    var subscribers = responseValue.SubscribersDataList
                        .Where(sd => sd.Status == (int)ContactStatus.Subscribed)
                        .Select(sd => sd.Email)
                        .ToList();
                    foreach (var email in subscribers)
                    {
                        var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(email, store.Id);
                        if (subscription is null)
                        {
                            await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new()
                            {
                                Active = true,
                                Email = email,
                                StoreId = store.Id,
                                NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                CreatedOnUtc = DateTime.UtcNow
                            }, false);
                        }
                        else if (!subscription.Active)
                        {
                            subscription.Active = true;
                            await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(subscription, false);
                        }
                    }
                    contacts += subscribers.Count;

                    //unsubscribe
                    var unsubscribers = responseValue.SubscribersDataList
                        .Where(sd => sd.Status == (int)ContactStatus.Unsubscribed)
                        .Select(sd => sd.Email)
                        .ToList();
                    foreach (var email in unsubscribers)
                    {
                        var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(email, store.Id);
                        if (subscription?.Active == true)
                        {
                            subscription.Active = false;
                            await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(subscription, false);
                        }
                    }
                    contacts += unsubscribers.Count;

                    pageIndex++;

                    if (responseValue.CurrentPage == responseValue.LastPage)
                        break;
                }

                messages.Add((NotifyType.Success, $"Synchronization info: {contacts} contacts have been exported from list '{list.ContactListInfo?.Name}'"));
            }
            catch (Exception exception)
            {
                messages.Add((NotifyType.Success, $"Synchronization info: {contacts} contacts have been exported"));
                messages.Add((NotifyType.Error, $"Synchronization error: {exception.Message}"));
                var logMessage = $"{EcomailDefaults.SystemName} error: {Environment.NewLine}{exception.Message}";
                await _logger.ErrorAsync(logMessage, exception, await _workContext.GetCurrentCustomerAsync());
            }

            return messages;
        }

        /// <summary>
        /// Prepare newsletter subscribers to sync
        /// </summary>
        /// <param name="storeId">Store id</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of subscriber data
        /// </returns>
        private async Task<List<SubscriberDataRequest>> PrepareSubscribersAsync(int storeId, int pageIndex, int pageSize)
        {
            var subscribers = new List<SubscriberDataRequest>();

            var attributeNames = new[]
            {
                NopCustomerDefaults.FirstNameAttribute,
                NopCustomerDefaults.LastNameAttribute,
                NopCustomerDefaults.CountryIdAttribute,
                NopCustomerDefaults.CompanyAttribute,
                NopCustomerDefaults.CityAttribute,
                NopCustomerDefaults.StreetAddressAttribute,
                NopCustomerDefaults.ZipPostalCodeAttribute,
                NopCustomerDefaults.PhoneAttribute,
                NopCustomerDefaults.DateOfBirthAttribute,
                NopCustomerDefaults.GenderAttribute
            };

            //get contacts of newsletter subscribers
            var contactValues = _newsLetterSubscriptionRepository.Table
                .Where(subscription => subscription.Active && subscription.StoreId == storeId)
                .OrderBy(subscription => subscription.Email)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Join(_customerRepository.Table.Where(customer => customer.Active && !customer.Deleted),
                    subscription => subscription.Email,
                    customer => customer.Email,
                    (subscription, customer) => new { Customer = customer, Active = subscription.Active })
                .Join(_genericAttributeRepository.Table.Where(attribute => attribute.KeyGroup == nameof(Customer) && attributeNames.Contains(attribute.Key)),
                    customer => customer.Customer.Id,
                    attribute => attribute.EntityId,
                    (customer, attribute) => new { Customer = customer.Customer, Active = customer.Active, Name = attribute.Key, Value = attribute.Value })
                .SelectMany(customerAttribute => _countryRepository.Table
                    .Where(country => customerAttribute.Name == NopCustomerDefaults.CountryIdAttribute && country.Id.ToString() == customerAttribute.Value)
                    .DefaultIfEmpty(),
                    (customerAttribute, country) => new
                    {
                        Id = customerAttribute.Customer.Id,
                        Active = customerAttribute.Active,
                        Email = customerAttribute.Customer.Email,
                        Name = customerAttribute.Name,
                        Value = customerAttribute.Name == NopCustomerDefaults.CountryIdAttribute ? country.TwoLetterIsoCode : customerAttribute.Value
                    })
                .GroupBy(customerAttribute => customerAttribute.Email)
                .Select(customerAttributes => new
                {
                    Email = customerAttributes.Key,
                    Id = customerAttributes.FirstOrDefault().Id,
                    Active = customerAttributes.FirstOrDefault().Active,
                    FirstName = customerAttributes.FirstOrDefault(item => item.Name == NopCustomerDefaults.FirstNameAttribute).Value,
                    LastName = customerAttributes.FirstOrDefault(item => item.Name == NopCustomerDefaults.LastNameAttribute).Value,
                    Company = customerAttributes.FirstOrDefault(item => item.Name == NopCustomerDefaults.CompanyAttribute).Value,
                    City = customerAttributes.FirstOrDefault(item => item.Name == NopCustomerDefaults.CityAttribute).Value,
                    Street = customerAttributes.FirstOrDefault(item => item.Name == NopCustomerDefaults.StreetAddressAttribute).Value,
                    Zip = customerAttributes.FirstOrDefault(item => item.Name == NopCustomerDefaults.ZipPostalCodeAttribute).Value,
                    Country = customerAttributes.FirstOrDefault(item => item.Name == NopCustomerDefaults.CountryIdAttribute).Value,
                    Phone = customerAttributes.FirstOrDefault(item => item.Name == NopCustomerDefaults.PhoneAttribute).Value,
                    Birthday = customerAttributes.FirstOrDefault(item => item.Name == NopCustomerDefaults.DateOfBirthAttribute).Value,
                    Gender = customerAttributes.FirstOrDefault(item => item.Name == NopCustomerDefaults.GenderAttribute).Value
                })
                .ToList();

            //GDPR
            if (_ecomailSettings.ConsentId > 0)
            {
                var gdprLogs = await _gdprService.GetAllLogAsync(consentId: _ecomailSettings.ConsentId, requestType: GdprRequestType.ConsentAgree);
                contactValues = contactValues.Where(contact => gdprLogs.Any(consent => consent.CustomerId == contact.Id)).ToList();
            }

            foreach (var contact in contactValues)
            {
                var subscriber = new SubscriberDataRequest
                {
                    Email = contact.Email,
                    FirstName = contact.FirstName ?? string.Empty,
                    Surname = contact.LastName ?? string.Empty,
                    Company = contact.Company ?? string.Empty,
                    City = contact.City ?? string.Empty,
                    Street = contact.Street ?? string.Empty,
                    Zip = contact.Zip ?? string.Empty,
                    Country = contact.Country ?? string.Empty,
                    Phone = contact.Phone ?? string.Empty,
                    Birthday = contact.Birthday ?? string.Empty,
                    Gender = string.IsNullOrEmpty(contact.Gender)
                        ? string.Empty
                        : contact.Gender.Equals("M", StringComparison.InvariantCultureIgnoreCase) ? "male" : "female",
                    CustomFields = new() { [EcomailDefaults.SubscriberCustomField.Name] = EcomailDefaults.SubscriberCustomField.Value }
                };

                subscribers.Add(subscriber);
            }

            //add contacts without details
            var notCustomers = _newsLetterSubscriptionRepository.Table
                .Where(subscription => subscription.Active && subscription.StoreId == storeId)
                .OrderBy(subscription => subscription.Email)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .SelectMany(subscription => _customerRepository.Table
                    .Where(customer => subscription.Email == customer.Email && customer.Active && !customer.Deleted)
                    .DefaultIfEmpty(),
                    (subscription, customer) => new { Email = subscription.Email, Customer = customer, Active = subscription.Active })
                .Where(details => details.Customer == null)
                .Select(details => new { Email = details.Email, Active = details.Active })
                .ToList();

            foreach (var contact in notCustomers)
            {
                var subscriber = new SubscriberDataRequest
                {
                    Email = contact.Email,
                    CustomFields = new() { [EcomailDefaults.SubscriberCustomField.Name] = EcomailDefaults.SubscriberCustomField.Value }
                };

                subscribers.Add(subscriber);
            }

            return subscribers;
        }

        /// <summary>
        /// Prepare customers to sync
        /// </summary>
        /// <param name="storeId">Store id</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>The list of subscriber data</returns>
        private List<SubscriberDataRequest> PrepareCustomers(int storeId, int pageIndex, int pageSize)
        {
            var customers = new List<SubscriberDataRequest>();
            var statusIds = ((_ecomailSettings.OrderStatuses?.Any() ?? false) ? _ecomailSettings.OrderStatuses : new() { (int)OrderStatus.Complete })
                .Select(status => status)
                .ToList();

            //get contacts of customers
            var contactValues = _orderRepository.Table
                .Where(order => !order.Deleted && order.StoreId == storeId && statusIds.Contains(order.OrderStatusId))
                .OrderByDescending(order => order.CreatedOnUtc)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Join(_addressRepository.Table,
                    order => order.BillingAddressId,
                    address => address.Id,
                    (order, address) => address)
                .SelectMany(customerAddress => _countryRepository.Table
                    .Where(country => customerAddress.CountryId.HasValue && customerAddress.CountryId.Value == country.Id)
                    .DefaultIfEmpty(),
                    (customerAddress, country) => new
                    {
                        Email = customerAddress.Email,
                        FirstName = customerAddress.FirstName,
                        LastName = customerAddress.LastName,
                        Company = customerAddress.Company,
                        City = customerAddress.City,
                        Street = customerAddress.Address1,
                        Zip = customerAddress.ZipPostalCode,
                        Phone = customerAddress.PhoneNumber,
                        Country = country.TwoLetterIsoCode
                    })
                .GroupBy(customerAddress => customerAddress.Email)
                .Select(customerAddress => new
                {
                    Email = customerAddress.Key,
                    FirstName = customerAddress.FirstOrDefault().FirstName,
                    LastName = customerAddress.FirstOrDefault().LastName,
                    Company = customerAddress.FirstOrDefault().Company,
                    City = customerAddress.FirstOrDefault().City,
                    Street = customerAddress.FirstOrDefault().Street,
                    Zip = customerAddress.FirstOrDefault().Zip,
                    Country = customerAddress.FirstOrDefault().Country,
                    Phone = customerAddress.FirstOrDefault().Phone
                })
                .ToList();

            foreach (var contact in contactValues)
            {
                customers.Add(new SubscriberDataRequest
                {
                    Email = contact.Email,
                    FirstName = contact.FirstName ?? string.Empty,
                    Surname = contact.LastName ?? string.Empty,
                    Company = contact.Company ?? string.Empty,
                    City = contact.City ?? string.Empty,
                    Street = contact.Street ?? string.Empty,
                    Zip = contact.Zip ?? string.Empty,
                    Country = contact.Country ?? string.Empty,
                    Phone = contact.Phone ?? string.Empty
                });
            }

            return customers;
        }

        /// <summary>
        /// Prepare orders to sync
        /// </summary>
        /// <param name="emails">Emails to filter orders</param>
        /// <param name="storeId">Store id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of subscriber data
        /// </returns>
        private async Task<List<TransactionCreateRequest>> PrepareOrdersAsync(List<string> emails, int storeId)
        {
            var transactions = new List<TransactionCreateRequest>();
            var statusIds = ((_ecomailSettings.OrderStatuses?.Any() ?? false) ? _ecomailSettings.OrderStatuses : new() { (int)OrderStatus.Complete })
                .Select(status => status)
                .ToList();

            var orders = _orderRepository.Table
                .Where(order => !order.Deleted && order.StoreId == storeId && statusIds.Contains(order.OrderStatusId))
                .OrderByDescending(order => order.CreatedOnUtc)
                .GroupJoin(_orderItemRepository.Table,
                    order => order.Id,
                    item => item.OrderId,
                    (order, items) => new { Order = order, Items = items })
                .Join(_addressRepository.Table,
                    order => order.Order.BillingAddressId,
                    address => address.Id,
                    (order, address) => new { Order = order.Order, Items = order.Items, Address = address })
                .Where(order => emails.Contains(order.Address.Email))
                .ToList();

            var store = await _storeService.GetStoreByIdAsync(storeId);
            var countries = await _countryService.GetAllCountriesAsync(showHidden: true);
            var productIds = orders.SelectMany(order => order.Items.Select(item => item.ProductId)).Distinct().ToArray();
            var products = await _productService.GetProductsByIdsAsync(productIds);
            var syncedOrders = await _ecomailOrderService.GetOrdersAsync(_ecomailSettings.ApiKey);

            foreach (var order in orders)
            {
                if (syncedOrders.Any(syncedOrder => syncedOrder.OrderId == order.Order.Id))
                    continue;

                if (!order.Items.Any())
                    continue;

                var orderDate = DateTime.SpecifyKind(statusIds.Contains((int)OrderStatus.Complete)
                    ? (order.Order.PaidDateUtc ?? order.Order.CreatedOnUtc)
                    : order.Order.CreatedOnUtc, DateTimeKind.Utc);

                var timestamp = (int)new DateTimeOffset(orderDate).ToUnixTimeSeconds();
                var transaction = new TransactionData
                {
                    Email = order.Address.Email,
                    OrderId = order.Order.Id,
                    OrderNumber = order.Order.CustomOrderNumber ?? order.Order.Id.ToString(),
                    Shop = store?.Url ?? string.Empty,
                    Amount = order.Order.OrderTotal.ToString("0.00", CultureInfo.InvariantCulture),
                    Tax = order.Order.OrderTax.ToString("0.00", CultureInfo.InvariantCulture),
                    Shipping = order.Order.OrderShippingExclTax.ToString("0.00", CultureInfo.InvariantCulture),
                    City = order.Address?.City ?? string.Empty,
                    County = order.Address?.County ?? string.Empty,
                    Country = countries.FirstOrDefault(country => country.Id == order.Address.CountryId)?.TwoLetterIsoCode ?? string.Empty,
                    Timestamp = timestamp
                };

                var items = order.Items.Select(orderItem => new TransactionItem
                {
                    ItemCode = orderItem.ProductId.ToString(),
                    Title = products.FirstOrDefault(product => product.Id == orderItem.ProductId)?.Name,
                    Price = orderItem.PriceInclTax.ToString("0.00", CultureInfo.InvariantCulture),
                    Quantity = orderItem.Quantity,
                    Timestamp = timestamp,
                }).ToList();

                transactions.Add(new TransactionCreateRequest { TransactionData = transaction, TransactionItems = items });
            }

            return transactions;
        }

        /// <summary>
        /// Generate product feed
        /// </summary>
        /// <param name="fullPath">The path and name of the feed file</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task GenerateAsync(string fullPath)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var items = await (await _productService.SearchProductsAsync(storeId: store.Id, visibleIndividuallyOnly: true)).SelectAwait(async product =>
            {
                var picture = await _pictureService.GetProductPictureAsync(product, null);
                var (imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture);
                var mappings = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
                var category = await _categoryService.GetCategoryByIdAsync(mappings.FirstOrDefault()?.CategoryId ?? 0);
                var seName = await _urlRecordService.GetSeNameAsync(product);
                var url = urlHelper.RouteUrl("Product", new { SeName = seName }, _webHelper.GetCurrentRequestProtocol());
                var (price, _) = await _taxService.GetProductPriceAsync(product, product.Price);

                return new ProductItem
                {
                    Id = product.Id,
                    Name = product.Name,
                    Category = category?.Name,
                    ShortDescription = product.ShortDescription,
                    FullDescription = product.FullDescription,
                    Sku = product.Sku,
                    Price = price,
                    Url = url,
                    ImageUrl = imageUrl,
                    UpdatedOn = product.UpdatedOnUtc
                };
            }).ToListAsync();

            await using var stringWriter = new Utf8StringWriter();
            await using var writer = XmlWriter.Create(stringWriter, new XmlWriterSettings
            {
                Async = true,
                Encoding = Encoding.UTF8,
                ConformanceLevel = ConformanceLevel.Auto
            });

            await writer.WriteStartDocumentAsync();
            await writer.WriteStartElementAsync(null, "data", null);
            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item.Name))
                    continue;

                await writer.WriteStartElementAsync(null, "item", null);
                await writer.WriteElementStringAsync(null, "id", null, item.Id.ToString());
                await writer.WriteElementStringAsync(null, "name", null, item.Name);
                await writer.WriteElementStringAsync(null, "category", null, item.Category);
                await writer.WriteElementStringAsync(null, "shortDescription", null, item.ShortDescription);
                await writer.WriteElementStringAsync(null, "fullDescription", null, item.FullDescription);
                await writer.WriteElementStringAsync(null, "sku", null, item.Sku);
                await writer.WriteElementStringAsync(null, "price", null, item.Price.ToString());
                await writer.WriteElementStringAsync(null, "url", null, await XmlHelper.XmlEncodeAsync(item.Url));
                await writer.WriteElementStringAsync(null, "imageUrl", null, await XmlHelper.XmlEncodeAsync(item.ImageUrl));
                await writer.WriteElementStringAsync(null, "updatedOn", null, item.UpdatedOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                await writer.WriteEndElementAsync();
            }
            await writer.WriteEndElementAsync();
            await writer.FlushAsync();

            if (_nopFileProvider.FileExists(fullPath))
                _nopFileProvider.DeleteFile(fullPath);
            _nopFileProvider.CreateFile(fullPath);
            await _nopFileProvider.WriteAllTextAsync(fullPath, stringWriter.ToString(), Encoding.UTF8);
        }

        #endregion

        #region Methods

        #region Contacts

        /// <summary>
        /// Get available lists to synchronize contacts
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of id-name pairs of lists; error if exists
        /// </returns>
        public async Task<(List<(string Id, string Name)> Result, string Error)> GetListsAsync()
        {
            return await HandleFunctionAsync(async () =>
            {
                if (string.IsNullOrEmpty(_ecomailSettings.ApiKey))
                    throw new NopException("Plugin not configured");

                var availableLists = new List<(string Id, string Name)>();
                var response = string.Empty;
                try
                {
                    response = await _ecomailHttpClient.RequestAsync(EcomailDefaults.ListsApiUrl, null, HttpMethod.Get);
                }
                catch (Exception ex)
                {
                    throw new NopException($"Failed to get contact lists. {ex.Message}");
                }
                var contactLists = JsonConvert.DeserializeObject<List<ContactListResponse>>(response) ?? new();
                foreach (var contactList in contactLists)
                {
                    availableLists.Add((contactList.Id.ToString(), contactList.ListName));
                }

                return availableLists;
            });
        }

        /// <summary>
        /// Create a contact list on account 
        /// </summary>
        /// <param name="createListRequest">Create contact list request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains a contact list of Ecomail; error if exists
        /// </returns>
        public async Task<(ContactListResponse Result, string Error)> CreateListAsync(CreateListRequest createListRequest)
        {
            return await HandleFunctionAsync(async () =>
            {
                if (createListRequest is null)
                    throw new ArgumentNullException(nameof(createListRequest));

                if (string.IsNullOrEmpty(_ecomailSettings.ApiKey))
                    throw new NopException("Plugin not configured");

                var payload = JsonConvert.SerializeObject(createListRequest);
                var response = string.Empty;
                try
                {
                    response = await _ecomailHttpClient.RequestAsync(EcomailDefaults.ListsApiUrl, payload, HttpMethod.Post);
                }
                catch (Exception ex)
                {
                    throw new NopException($"Failed to create contact list. {ex.Message}");
                }
                var contactList = JsonConvert.DeserializeObject<ContactListResponse>(response);
                if (!string.IsNullOrEmpty(contactList.Errors))
                    throw new NopException($"Failed to create contact list, parameters are incorrect: {contactList.Errors}");

                if (contactList is null || contactList.Id < 1)
                    throw new NopException("Failed to create contact list, parameters are incorrect");

                return contactList;
            });
        }

        #endregion

        #region Synchronization

        /// <summary>
        /// Synchronize contacts 
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of sync messages
        /// </returns>
        public async Task<List<(NotifyType Type, string Message)>> SynchronizeAsync()
        {
            var (syncMessages, _) = await HandleFunctionAsync(async () =>
            {
                var messages = new List<(NotifyType Type, string Message)>();

                var importMessages = await ImportContactsAsync();
                messages.AddRange(importMessages);

                var exportMessages = await ExportContactsAsync();
                messages.AddRange(exportMessages);

                return messages;
            });

            return syncMessages;
        }

        /// <summary>
        /// Subscribe a single contact to the list
        /// </summary>
        /// <param name="subscription">Newsletter subscription</param>
        /// <param name="address">Address used to sync customer details; pass null to use subscriber details to sync</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task SubscribeContactAsync(NewsLetterSubscription subscription, Address address = null)
        {
            await HandleFunctionAsync(async () =>
            {
                if (string.IsNullOrEmpty(subscription?.Email))
                    throw new NopException("Email not set");

                if (string.IsNullOrEmpty(_ecomailSettings.ApiKey))
                    throw new NopException("Plugin not configured");

                //ensure list exists
                _ = await GetListByIdAsync(_ecomailSettings.ListId)
                    ?? throw new NopException("Contact list to synchronize not set");

                var customer = await _customerService.GetCustomerByEmailAsync(subscription.Email);
                if (_ecomailSettings.ConsentId > 0 &&
                    customer is not null &&
                    !(await _gdprService.IsConsentAcceptedAsync(_ecomailSettings.ConsentId, customer.Id) ?? true))
                {
                    throw new NopException("Newsletter consent is not accepted");
                }

                var subscriberData = new SubscriberDataRequest { Email = subscription.Email };

                if (address is not null)
                {
                    subscriberData.FirstName = address.FirstName ?? string.Empty;
                    subscriberData.Surname = address.LastName ?? string.Empty;
                    subscriberData.Company = address.Company ?? string.Empty;
                    subscriberData.City = address.City ?? string.Empty;
                    subscriberData.Street = address.Address1 ?? string.Empty;
                    subscriberData.Zip = address.ZipPostalCode ?? string.Empty;
                    subscriberData.Country = (await _countryService.GetCountryByIdAsync(address.CountryId ?? 0))?.TwoLetterIsoCode ?? string.Empty;
                    subscriberData.Phone = address.PhoneNumber ?? string.Empty;
                }
                else if (customer is not null)
                {
                    var countryId = await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.CountryIdAttribute);
                    var country = await _countryService.GetCountryByIdAsync(countryId);
                    var firstName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.FirstNameAttribute);
                    var surname = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.LastNameAttribute);
                    var company = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.CompanyAttribute);
                    var city = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.CityAttribute);
                    var street = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.StreetAddressAttribute);
                    var zip = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.ZipPostalCodeAttribute);
                    var phone = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PhoneAttribute);
                    var birthday = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.DateOfBirthAttribute);
                    var gender = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.GenderAttribute);
                    gender = string.IsNullOrEmpty(gender) ? "" : gender.Equals("M", StringComparison.InvariantCultureIgnoreCase) ? "male" : "female";

                    subscriberData.FirstName = firstName ?? string.Empty;
                    subscriberData.Surname = surname ?? string.Empty;
                    subscriberData.Company = company ?? string.Empty;
                    subscriberData.City = city ?? string.Empty;
                    subscriberData.Street = street ?? string.Empty;
                    subscriberData.Zip = zip ?? string.Empty;
                    subscriberData.Country = country?.TwoLetterIsoCode ?? string.Empty;
                    subscriberData.Phone = phone ?? string.Empty;
                    subscriberData.Birthday = birthday ?? string.Empty;
                    subscriberData.Gender = gender ?? string.Empty;
                }

                if (subscription.Active)
                    subscriberData.CustomFields = new() { [EcomailDefaults.SubscriberCustomField.Name] = EcomailDefaults.SubscriberCustomField.Value };

                var emailExists = false;
                try
                {
                    var response = await _ecomailHttpClient
                        .RequestAsync(string.Format(EcomailDefaults.GetSubscriberApiUrl, subscription.Email), null, HttpMethod.Get);
                    var subscriber = new ContactResponse();
                    subscriber = JsonConvert.DeserializeAnonymousType(response, new { Subscriber = subscriber }).Subscriber;
                    emailExists = subscriber?.Lists?.Any(list => list.Value?.ListId == _ecomailSettings.ListId && list.Value?.Status == 1) ?? false;
                }
                catch { }

                var subscriberAddRequest = new SubscriberAddRequest
                {
                    SubscriberData = subscriberData,
                    TriggerAutoresponders = !emailExists,
                    UpdateExisting = true,
                    Resubscribe = true
                };

                var payload = JsonConvert.SerializeObject(subscriberAddRequest);
                try
                {
                    var response = await _ecomailHttpClient
                        .RequestAsync(string.Format(EcomailDefaults.SubscribeApiUrl, _ecomailSettings.ListId), payload, HttpMethod.Post);
                }
                catch (Exception ex)
                {
                    throw new NopException($"Failed to subscribe contact '{subscription.Email}'. {ex.Message}");
                }

                return true;
            });
        }

        /// <summary>
        /// Unsubscribe a contact from the list
        /// </summary>
        /// <param name="subscription">Newsletter subscription</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task UnsubscribeContactAsync(NewsLetterSubscription subscription)
        {
            await HandleFunctionAsync(async () =>
            {
                if (string.IsNullOrEmpty(subscription?.Email))
                    throw new NopException("Email not set");

                if (string.IsNullOrEmpty(_ecomailSettings.ApiKey))
                    throw new NopException("Plugin not configured");

                //ensure list exists
                _ = await GetListByIdAsync(_ecomailSettings.ListId)
                    ?? throw new NopException("Contact list to synchronize not set");

                var payload = JsonConvert.SerializeObject(new { email = subscription.Email });
                try
                {
                    var response = await _ecomailHttpClient
                        .RequestAsync(string.Format(EcomailDefaults.UnubscribeApiUrl, _ecomailSettings.ListId), payload, HttpMethod.Delete);
                }
                catch (Exception ex)
                {
                    throw new NopException($"Failed to unsubscribe contact '{subscription.Email}'. {ex.Message}");
                }

                return true;
            });
        }

        #endregion

        #region Tracking

        /// <summary>
        /// Set customer mail identifier
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task SetEcomailIdAsync()
        {
            await HandleFunctionAsync(async () =>
            {
                //check id in request query
                var ecomailId = _webHelper.QueryString<string>("ecmid");
                if (string.IsNullOrEmpty(ecomailId))
                    return false;

                var customer = await _workContext.GetCurrentCustomerAsync();
                if (customer.IsBackgroundTaskAccount() || customer.IsSearchEngineAccount())
                    return false;

                await _genericAttributeService.SaveAttributeAsync(customer, EcomailDefaults.CustomerEcomailIdAttribute, ecomailId);

                return true;
            });
        }

        /// <summary>
        /// Handle shopping cart event
        /// </summary>
        /// <param name="cartItem">Shopping cart item</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleShoppingCartEventAsync(ShoppingCartItem cartItem)
        {
            await HandleFunctionAsync(async () =>
            {
                if (cartItem is null)
                    throw new ArgumentNullException(nameof(cartItem));

                if (string.IsNullOrEmpty(_ecomailSettings.ApiKey))
                    throw new NopException("Plugin not configured");

                if (!_ecomailSettings.UseTracking)
                    throw new NopException("Tracking not enabled");

                var customer = await _customerService.GetCustomerByIdAsync(cartItem.CustomerId)
                    ?? throw new NopException("Customer not found");

                var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)
                    ?? throw new NopException("Primary store currency not found");

                var email = customer?.Email;
                if (string.IsNullOrEmpty(email))
                    email = await _genericAttributeService.GetAttributeAsync<string>(customer, EcomailDefaults.CustomerEcomailIdAttribute);
                if (string.IsNullOrEmpty(email))
                    return false;

                //prepare cart data
                var trackEvent = new EcomailEvent
                {
                    Email = email,
                    Category = "ue",
                    Action = EcomailDefaults.BasketActionAttribute,
                    Label = EcomailDefaults.BasketActionAttribute
                };

                //get current customer's shopping cart
                var store = await _storeService.GetStoreByIdAsync(cartItem.StoreId);
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, cartItem.StoreId);
                var shoppingCartGuid = await _genericAttributeService.GetAttributeAsync<Guid?>(customer, EcomailDefaults.ShoppingCartGuidAttribute);
                if (!shoppingCartGuid.HasValue || shoppingCartGuid.Value == Guid.Empty)
                    shoppingCartGuid = Guid.NewGuid();

                if (cart.Any())
                {
                    var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

                    //get shopping cart amounts
                    var (cartTax, _) = await _orderTotalCalculationService.GetTaxTotalAsync(cart, false);
                    var (cartDiscount, _, cartSubtotal, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, false);
                    var (cartShipping, _, _) = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(cart, false);
                    var (cartTotal, _, _, _, _, _) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart, false, false);

                    //get products data by shopping cart items
                    var itemsData = await cart.SelectAwait(async item =>
                    {
                        var product = await _productService.GetProductByIdAsync(item.ProductId);
                        if (product is null)
                            return null;

                        var sku = await _productService.FormatSkuAsync(product, item.AttributesXml);
                        var seName = await _urlRecordService.GetSeNameAsync(product);
                        var picture = await _pictureService.GetProductPictureAsync(product, item.AttributesXml);
                        var (url, _) = await _pictureService.GetPictureUrlAsync(picture);
                        var mappings = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
                        var category = await _categoryService.GetCategoryByIdAsync(mappings.FirstOrDefault()?.CategoryId ?? 0);
                        var (unitPrice, _, _) = await _shoppingCartService.GetUnitPriceAsync(item, true);
                        var (itemPrice, _) = await _taxService.GetProductPriceAsync(product, unitPrice, true, customer);

                        return new TrackCartProduct
                        {
                            ProductId = product.Id.ToString(),
                            Name = product.Name ?? string.Empty,
                            Description = product.ShortDescription ?? string.Empty,
                            FullDescription = product.FullDescription ?? string.Empty,
                            Sku = sku ?? string.Empty,
                            Category = category?.Name ?? string.Empty,
                            Url = urlHelper.RouteUrl("Product", new { SeName = seName }, _webHelper.GetCurrentRequestProtocol()),
                            ImgUrl = url ?? string.Empty,
                            Quantity = item.Quantity,
                            Price = itemPrice.ToString("0.00", CultureInfo.InvariantCulture)
                        };
                    }).Where(product => product is not null).ToArrayAsync();

                    var data = new
                    {
                        data = new EcomailEventData
                        {
                            Data = new
                            {
                                action = EcomailDefaults.BasketActionAttribute,
                                products = itemsData,
                                cartId = shoppingCartGuid.ToString(),
                                url = urlHelper.RouteUrl("ShoppingCart", null, _webHelper.GetCurrentRequestProtocol()),
                                currency = currency.CurrencyCode,
                                affiliation = store.Name,
                                subtotal = cartSubtotal.ToString("0.00", CultureInfo.InvariantCulture),
                                shipping = (cartShipping ?? decimal.Zero).ToString("0.00", CultureInfo.InvariantCulture),
                                tax = cartTax.ToString("0.00", CultureInfo.InvariantCulture),
                                discount = cartDiscount.ToString("0.00", CultureInfo.InvariantCulture)
                            }
                        }
                    };

                    trackEvent.Value = JsonConvert.SerializeObject(data).ToString();
                }
                else
                {
                    //there are no items in the cart, so the cart is deleted
                    trackEvent.Value = string.Empty;
                }

                var payload = JsonConvert.SerializeObject(new EcomailTrackEvent { Event = trackEvent });
                try
                {
                    var response = await _ecomailHttpClient.RequestAsync(EcomailDefaults.TrackEventApiUrl, payload, HttpMethod.Post);
                }
                catch (Exception ex)
                {
                    throw new NopException($"Failed to create cart tracking event. {ex.Message}");
                }

                //update GUID for the current customer's shopping cart
                await _genericAttributeService.SaveAttributeAsync(customer, EcomailDefaults.ShoppingCartGuidAttribute, shoppingCartGuid);

                return true;
            }, _ecomailSettings.LogTrackingErrors);
        }

        /// <summary>
        /// Handle order event
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="orderEventType">Order event type</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleOrderEventAsync(Order order, OrderEventType orderEventType)
        {
            await HandleFunctionAsync(async () =>
            {
                if (order is null)
                    throw new ArgumentNullException(nameof(order));

                if (_ecomailSettings.OrderEventType != orderEventType)
                    return false;

                if (string.IsNullOrEmpty(_ecomailSettings.ApiKey))
                    throw new NopException("Plugin not configured");

                var billingAddress = await _addressService.GetAddressByIdAsync(order?.BillingAddressId ?? 0);
                var email = billingAddress?.Email;
                if (string.IsNullOrEmpty(email))
                    throw new NopException("Email not set");

                var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(email, order.StoreId);
                if (subscription is null && _ecomailSettings.SyncSubscribersOnly)
                    throw new NopException("Subscription not found");

                //first sync customer contact
                subscription ??= new() { Email = email, StoreId = order.StoreId };
                await SubscribeContactAsync(subscription, billingAddress);

                var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId)
                    ?? throw new NopException("Customer not found");

                if (!_ecomailSettings.ImportOrdersOnSync)
                    throw new NopException("Order sync not enabled");

                var store = await _storeService.GetStoreByIdAsync(order.StoreId);
                var country = await _countryService.GetCountryByIdAsync(billingAddress?.CountryId ?? 0);
                var orderDate = DateTime.SpecifyKind(orderEventType == OrderEventType.Paid
                    ? (order.PaidDateUtc ?? order.CreatedOnUtc)
                    : order.CreatedOnUtc, DateTimeKind.Utc);
                var timestamp = (int)new DateTimeOffset(orderDate).ToUnixTimeSeconds();
                var transactionData = new TransactionData
                {
                    OrderId = order.Id,
                    OrderNumber = order.CustomOrderNumber ?? order.Id.ToString(),
                    Email = billingAddress.Email ?? string.Empty,
                    Shop = store?.Url ?? string.Empty,
                    Amount = order.OrderTotal.ToString("0.00", CultureInfo.InvariantCulture),
                    Tax = order.OrderTax.ToString("0.00", CultureInfo.InvariantCulture),
                    Shipping = order.OrderShippingExclTax.ToString("0.00", CultureInfo.InvariantCulture),
                    City = billingAddress?.City ?? string.Empty,
                    County = billingAddress?.County ?? string.Empty,
                    Country = country?.TwoLetterIsoCode ?? string.Empty,
                    Timestamp = timestamp
                };

                var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
                var transactionItems = await orderItems.SelectAwait(async orderItem =>
                {
                    var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                    if (product is null)
                        return null;

                    var mappings = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
                    var category = await _categoryService.GetCategoryByIdAsync(mappings.FirstOrDefault()?.CategoryId ?? 0);

                    return new TransactionItem
                    {
                        ItemCode = product.Id.ToString(),
                        Title = product.Name ?? string.Empty,
                        Category = category?.Name ?? "No category",
                        Price = orderItem.PriceInclTax.ToString("0.00", CultureInfo.InvariantCulture),
                        Quantity = orderItem.Quantity,
                        Timestamp = timestamp,
                    };
                }).Where(item => item is not null).ToListAsync();

                var payload = JsonConvert.SerializeObject(new TransactionCreateRequest
                {
                    TransactionData = transactionData,
                    TransactionItems = transactionItems
                });
                try
                {
                    var response = await _ecomailHttpClient.RequestAsync(EcomailDefaults.AddTransactionDataApiUrl, payload, HttpMethod.Post);
                    var transaction = JsonConvert.DeserializeAnonymousType(response, new { Transaction = new { Id = string.Empty } }).Transaction;
                    await _ecomailOrderService.InsertOrderAsync(new()
                    {
                        OrderId = order.Id,
                        TransactionId = transaction?.Id,
                        ApiKey = _ecomailSettings.ApiKey,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    throw new NopException($"Failed to create order transaction. {ex.Message}");
                }

                return true;
            }, _ecomailSettings.LogTrackingErrors);
        }

        #endregion

        #region Webhooks

        /// <summary>
        /// Handle webhook request
        /// </summary>
        /// <param name="request">HTTP request</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleWebhookAsync(HttpRequest request)
        {
            await HandleFunctionAsync(async () =>
            {
                if (string.IsNullOrEmpty(_ecomailSettings.ApiKey))
                    throw new NopException("Plugin not configured");

                using var streamReader = new StreamReader(request.Body);
                var response = await streamReader.ReadToEndAsync();

                var webhook = JsonConvert.DeserializeObject<WebhookRequest>(response);
                var contact = webhook.WebhookPayload;
                if (string.IsNullOrEmpty(contact?.Email) || string.IsNullOrEmpty(contact.Status))
                    throw new NopException("Webhook error: payload is empty");

                if (_ecomailSettings.ListId != contact.ListId)
                    throw new NopException($"Webhook error: the set sync list (#{_ecomailSettings.ListId}) doesn't match the contact list (#{contact.ListId})");

                var store = await _storeContext.GetCurrentStoreAsync();
                var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(contact.Email, store.Id);

                if (contact.Status.Equals("SUBSCRIBED", StringComparison.InvariantCultureIgnoreCase))
                {
                    var customer = _ecomailSettings.ConsentId > 0 ? await _customerService.GetCustomerByEmailAsync(contact.Email) : null;
                    if (customer is null || (await _gdprService.IsConsentAcceptedAsync(_ecomailSettings.ConsentId, customer.Id) ?? true))
                    {
                        if (subscription is null)
                        {
                            await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new()
                            {
                                Active = true,
                                Email = contact.Email,
                                StoreId = store.Id,
                                NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                CreatedOnUtc = DateTime.UtcNow
                            }, false);
                        }
                        else if (!subscription.Active)
                        {
                            subscription.Active = true;
                            await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(subscription, false);
                        }
                    }
                }

                if (contact.Status.Equals("UNSUBSCRIBED", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (subscription?.Active == true)
                    {
                        subscription.Active = false;
                        await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(subscription, false);
                    }
                }

                return true;
            });
        }

        #endregion

        #region Feed

        /// <summary>
        /// Generate product feed
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<string> GenerateFeedAsync()
        {
            var (path, _) = await HandleFunctionAsync(async () =>
            {
                _nopFileProvider.CreateDirectory(_nopFileProvider.GetAbsolutePath(EcomailDefaults.FeedsDirectory));
                var fullPath = _nopFileProvider.GetAbsolutePath(EcomailDefaults.FeedsDirectory, EcomailDefaults.FeedFileName);
                if (_nopFileProvider.FileExists(fullPath))
                {
                    if (_nopFileProvider.GetLastWriteTimeUtc(fullPath) > DateTime.UtcNow.AddHours(-_ecomailSettings.RebuildFeedXmlAfterHours))
                        return fullPath;
                }

                await GenerateAsync(fullPath);

                return fullPath;
            });

            return path;
        }

        /// <summary>
        /// Text writer that writes to a string buffer with UTF8 encoding
        /// </summary>
        public class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
        }

        #endregion

        #endregion
    }
}