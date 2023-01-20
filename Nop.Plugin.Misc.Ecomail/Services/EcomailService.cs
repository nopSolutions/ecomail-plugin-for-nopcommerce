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
using Nop.Plugin.Misc.Ecomail.Domains;
using Nop.Plugin.Misc.Ecomail.Domains.Api;
using Nop.Plugin.Misc.Ecomail.Domains.Api.Tracking;
using Nop.Plugin.Misc.Ecomail.Domains.Api.Webhook;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
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
        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<GenericAttribute> _genericAttributeRepository;
        private readonly IRepository<NewsLetterSubscription> _newsLetterSubscriptionRepository;
        private readonly ISettingService _settingService;
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
            IRepository<Country> countryRepository,
            IRepository<Customer> customerRepository,
            IRepository<GenericAttribute> genericAttributeRepository,
            IRepository<NewsLetterSubscription> newsLetterSubscriptionRepository,
            ISettingService settingService,
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
            _countryRepository = countryRepository;
            _customerRepository = customerRepository;
            _genericAttributeRepository = genericAttributeRepository;
            _newsLetterSubscriptionRepository = newsLetterSubscriptionRepository;
            _settingService = settingService;
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

        #region Utilites

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
        /// <param name="apiKey">API key</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains a contact list
        /// </returns>
        private async Task<EcomailContactListDetailsResponse> GetListByIdAsync(int listId, string apiKey)
        {
            if (listId < 1)
                return null;

            try
            {
                var response = await _ecomailHttpClient
                    .RequestAsync(string.Format(EcomailDefaults.GetContactListByIdApiUrl, listId), null, HttpMethod.Get, apiKey);
                var contactListDetails = JsonConvert.DeserializeObject<EcomailContactListDetailsResponse>(response);
                return contactListDetails;
            }
            catch (Exception ex)
            {
                throw new NopException($"Failed to get contact list #{listId}. {ex.Message}");
            }
        }

        /// <summary>
        /// Import all contacts from the stores to account
        /// </summary>
        /// <param name="storeIds">List of store ids</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of sync messages
        /// </returns>
        private async Task<List<(NotifyType Type, string Message)>> ImportContactsAsync(List<int> storeIds)
        {
            var messages = new List<(NotifyType, string)>();
            foreach (var storeId in storeIds)
            {
                var contacts = 0;
                try
                {
                    var apiKeyKey = $"{nameof(EcomailSettings)}.{nameof(EcomailSettings.ApiKey)}";
                    var apiKey = await _settingService.GetSettingByKeyAsync<string>(apiKeyKey, storeId: storeId);
                    if (string.IsNullOrEmpty(apiKey) && storeId > 0)
                        apiKey = await _settingService.GetSettingByKeyAsync<string>(apiKeyKey);
                    if (string.IsNullOrEmpty(apiKey))
                        throw new NopException("Plugin not configured");

                    //get list identifier from the settings
                    var listKey = $"{nameof(EcomailSettings)}.{nameof(EcomailSettings.ListId)}";
                    var listId = await _settingService.GetSettingByKeyAsync<int>(listKey, storeId: storeId);
                    if (listId == 0)
                        listId = await _settingService.GetSettingByKeyAsync<int>(listKey);
                    if (listId == 0)
                        throw new NopException("Contact list to synchronize not set");

                    //ensure list exists
                    var list = await GetListByIdAsync(listId, apiKey);

                    var store = storeId > 0
                        ? await _storeService.GetStoreByIdAsync(storeId)
                        : await _storeContext.GetCurrentStoreAsync();

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

                    var pageIndex = 0;
                    var pageSize = _ecomailSettings.SyncPageSize;
                    while (true)
                    {
                        //try to get store subscriptions
                        var subscriptions = await _newsLetterSubscriptionService
                            .GetAllNewsLetterSubscriptionsAsync(storeId: store.Id, isActive: true, pageIndex: pageIndex, pageSize: pageSize);
                        if (!subscriptions.Any())
                            break;

                        var contactValues = _newsLetterSubscriptionRepository.Table
                            .Where(subscription => subscription.Active)
                            .OrderBy(subscription => subscription.Email)
                            .Skip(pageIndex * pageSize)
                            .Take(pageSize)
                            .Join(_customerRepository.Table.Where(customer => customer.Active && !customer.Deleted),
                                subscription => subscription.Email,
                                customer => customer.Email,
                                (subscription, customer) => customer)
                            .Join(_genericAttributeRepository.Table.Where(attribute => attribute.KeyGroup == nameof(Customer) && attributeNames.Contains(attribute.Key)),
                                customer => customer.Id,
                                attribute => attribute.EntityId,
                                (customer, attribute) => new { Customer = customer, Name = attribute.Key, Value = attribute.Value })
                            .SelectMany(customerAttribute => _countryRepository.Table
                                .Where(country => customerAttribute.Name == NopCustomerDefaults.CountryIdAttribute && country.Id.ToString() == customerAttribute.Value)
                                .DefaultIfEmpty(),
                                (customerAttribute, country) => new
                                {
                                    Id = customerAttribute.Customer.Id,
                                    Email = customerAttribute.Customer.Email,
                                    Name = customerAttribute.Name,
                                    Value = customerAttribute.Name == NopCustomerDefaults.CountryIdAttribute ? country.Name : customerAttribute.Value
                                })
                            .GroupBy(customerAttribute => customerAttribute.Email)
                            .Select(customerAttributes => new
                            {
                                Email = customerAttributes.Key,
                                Id = customerAttributes.FirstOrDefault().Id,
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

                        if (_ecomailSettings.ConsentId > 0)
                        {
                            var gdprLogs = await _gdprService.GetAllLogAsync(consentId: _ecomailSettings.ConsentId, requestType: GdprRequestType.ConsentAgree);
                            contactValues = contactValues.Where(contact => gdprLogs.Any(consent => consent.CustomerId == contact.Id)).ToList();
                        }

                        var subscribers = new List<SubscriberDataRequest>();
                        foreach (var contact in contactValues)
                        {
                            subscribers.Add(new SubscriberDataRequest
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
                                CustomFields = new Dictionary<string, CustomFieldsInfo>
                                {
                                    [EcomailDefaults.SubscriberStoreIdAttribute] = new CustomFieldsInfo(store.Id.ToString(), "int"),
                                    [EcomailDefaults.SubscriberStoreNameAttribute] = new CustomFieldsInfo(store.Name, "string"),
                                    [EcomailDefaults.SubscriberStoreUrlAttribute] = new CustomFieldsInfo(store.Url, "string")
                                }
                            });
                        }

                        var subscribersAddOnBulkRequest = new SubscribersAddOnBulkRequest
                        {
                            SubscriberDataList = subscribers,
                            UpdateExisting = true,
                            Resubscribe = false,
                            TriggerAutoresponders = false
                        };

                        var payload = JsonConvert.SerializeObject(subscribersAddOnBulkRequest);
                        var response = string.Empty;
                        try
                        {
                            response = await _ecomailHttpClient
                                .RequestAsync(string.Format(EcomailDefaults.SubscribeInBulkApiUrl, listId), payload, HttpMethod.Post, apiKey);
                        }
                        catch (Exception ex)
                        {
                            throw new NopException($"Failed to import contacts to list #{listId}. {ex.Message}");
                        }

                        contacts += subscribers.Count;
                        pageIndex++;
                    }

                    //success
                    messages.Add((NotifyType.Success, $"Synchronization info: {contacts} contacts have been imported to list '{list.ContactListInfo?.Name}'"));
                }
                catch (Exception exception)
                {
                    messages.Add((NotifyType.Success, $"Synchronization info: {contacts} contacts have been imported"));
                    messages.Add((NotifyType.Error, $"Synchronization error: {exception.Message}"));
                    var logMessage = $"{EcomailDefaults.SystemName} error: {Environment.NewLine}{exception.Message}";
                    await _logger.ErrorAsync(logMessage, exception, await _workContext.GetCurrentCustomerAsync());
                }
            }

            return messages;
        }

        /// <summary>
        /// Export all contacts from account to the sores
        /// </summary>
        /// <param name="storeIds">List of store ids</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of sync messages
        /// </returns>
        private async Task<List<(NotifyType Type, string Message)>> ExportContactsAsync(List<int> storeIds)
        {
            var messages = new List<(NotifyType, string)>();
            foreach (var storeId in storeIds)
            {
                var contacts = 0;
                try
                {
                    var exportKey = $"{nameof(EcomailSettings)}.{nameof(EcomailSettings.ExportContactsOnSync)}";
                    var export = await _settingService.GetSettingByKeyAsync<bool?>(exportKey, storeId: storeId);
                    if (export is null && storeId > 0)
                        export = await _settingService.GetSettingByKeyAsync<bool?>(exportKey);
                    if (export != true)
                        continue;

                    var apiKeyKey = $"{nameof(EcomailSettings)}.{nameof(EcomailSettings.ApiKey)}";
                    var apiKey = await _settingService.GetSettingByKeyAsync<string>(apiKeyKey, storeId: storeId);
                    if (string.IsNullOrEmpty(apiKey) && storeId > 0)
                        apiKey = await _settingService.GetSettingByKeyAsync<string>(apiKeyKey);
                    if (string.IsNullOrEmpty(apiKey))
                        throw new NopException("Plugin not configured");

                    //get list identifier from the settings
                    var listKey = $"{nameof(EcomailSettings)}.{nameof(EcomailSettings.ListId)}";
                    var listId = await _settingService.GetSettingByKeyAsync<int>(listKey, storeId: storeId);
                    if (listId == 0)
                        listId = await _settingService.GetSettingByKeyAsync<int>(listKey);
                    if (listId == 0)
                        throw new NopException("Contact list to synchronize not set");

                    //ensure list exists
                    var list = await GetListByIdAsync(listId, apiKey);

                    var store = storeId > 0
                        ? await _storeService.GetStoreByIdAsync(storeId)
                        : await _storeContext.GetCurrentStoreAsync();

                    var pageIndex = 1;
                    var pageSize = _ecomailSettings.SyncPageSize;
                    while (true)
                    {
                        var url = string.Format(EcomailDefaults.SubscribeListOfEcomailApiUrl, listId) + $"?per_page={pageSize}&page={pageIndex}";
                        var response = string.Empty;
                        try
                        {
                            response = await _ecomailHttpClient.RequestAsync(url, null, HttpMethod.Get, apiKey);
                        }
                        catch (Exception ex)
                        {
                            throw new NopException($"Failed to export contacts from list #{listId}. {ex.Message}");
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
            }

            return messages;
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

            await using var stringWriter = new StringWriter();
            await using var writer = XmlWriter.Create(stringWriter, new XmlWriterSettings
            {
                Async = true,
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
        /// <param name="apiKey">API key</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of id-name pairs of lists; error if exists
        /// </returns>
        public async Task<(List<(string Id, string Name)> Result, string Error)> GetListsAsync(string apiKey)
        {
            return await HandleFunctionAsync(async () =>
            {
                if (string.IsNullOrEmpty(apiKey))
                    throw new NopException("Plugin not configured");

                var availableLists = new List<(string Id, string Name)>();
                var response = string.Empty;
                try
                {
                    response = await _ecomailHttpClient.RequestAsync(EcomailDefaults.EcomailContactListApiUrl, null, HttpMethod.Get, apiKey);
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
                    response = await _ecomailHttpClient.RequestAsync(EcomailDefaults.EcomailContactListApiUrl, payload, HttpMethod.Post);
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
        /// <param name="synchronizationTask">Whether it's a scheduled synchronization</param>
        /// <param name="storeId">Store identifier; pass 0 to synchronize contacts for all stores</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of sync messages
        /// </returns>
        public async Task<List<(NotifyType Type, string Message)>> SynchronizeAsync(bool synchronizationTask = true, int storeId = 0)
        {
            var (syncMessages, _) = await HandleFunctionAsync(async () =>
            {
                var messages = new List<(NotifyType Type, string Message)>();

                //use only passed store identifier for the manual synchronization
                //use all store ids for the synchronization task
                var storeIds = !synchronizationTask
                    ? new List<int> { storeId }
                    : new List<int> { 0 }.Union((await _storeService.GetAllStoresAsync()).Select(store => store.Id)).ToList();

                var importMessages = await ImportContactsAsync(storeIds);
                messages.AddRange(importMessages);

                var exportMessages = await ExportContactsAsync(storeIds);
                messages.AddRange(exportMessages);

                return messages;
            });

            return syncMessages;
        }

        /// <summary>
        /// Subscribe a single contact to the list
        /// </summary>
        /// <param name="subscription">Newsletter subscription</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task SubscribeContactAsync(NewsLetterSubscription subscription)
        {
            await HandleFunctionAsync(async () =>
            {
                if (subscription is null)
                    throw new ArgumentNullException(nameof(subscription));

                var customer = await _customerService.GetCustomerByEmailAsync(subscription.Email)
                    ?? throw new NopException($"Customer not found by email '{subscription.Email}'");

                var apiKeyKey = $"{nameof(EcomailSettings)}.{nameof(EcomailSettings.ApiKey)}";
                var apiKey = await _settingService.GetSettingByKeyAsync<string>(apiKeyKey, storeId: subscription.StoreId);
                if (string.IsNullOrEmpty(apiKey))
                    apiKey = await _settingService.GetSettingByKeyAsync<string>(apiKeyKey);
                if (string.IsNullOrEmpty(apiKey))
                    throw new NopException("Plugin not configured");

                //get list identifier from the settings
                var listKey = $"{nameof(EcomailSettings)}.{nameof(EcomailSettings.ListId)}";
                var listId = await _settingService.GetSettingByKeyAsync<int>(listKey, storeId: subscription.StoreId);
                if (listId == 0)
                    listId = await _settingService.GetSettingByKeyAsync<int>(listKey);
                if (listId == 0)
                    throw new NopException("Contact list to synchronize not set");

                //ensure list exists
                await GetListByIdAsync(listId, apiKey);

                var store = await _storeService.GetStoreByIdAsync(subscription.StoreId);
                var email = subscription.Email;
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

                var subscriberData = new SubscriberDataRequest
                {
                    Email = subscription.Email,
                    FirstName = firstName ?? string.Empty,
                    Surname = surname ?? string.Empty,
                    Company = company ?? string.Empty,
                    City = city ?? string.Empty,
                    Street = street ?? string.Empty,
                    Zip = zip ?? string.Empty,
                    Country = country?.Name ?? string.Empty,
                    Phone = phone ?? string.Empty,
                    Birthday = birthday ?? string.Empty,
                    Gender = gender ?? string.Empty,
                    CustomFields = new Dictionary<string, CustomFieldsInfo>
                    {
                        [EcomailDefaults.SubscriberStoreIdAttribute] = new CustomFieldsInfo(store.Id.ToString(), "int"),
                        [EcomailDefaults.SubscriberStoreNameAttribute] = new CustomFieldsInfo(store.Name, "string"),
                        [EcomailDefaults.SubscriberStoreUrlAttribute] = new CustomFieldsInfo(store.Url, "string")
                    }
                };

                var subscriberAddRequest = new SubscriberAddRequest
                {
                    SubscriberData = subscriberData,
                    UpdateExisting = true,
                    Resubscribe = false,
                    TriggerAutoresponders = false
                };

                var payload = JsonConvert.SerializeObject(subscriberAddRequest);
                try
                {
                    var response = await _ecomailHttpClient
                        .RequestAsync(string.Format(EcomailDefaults.SubscribeToEcomailApiUrl, listId), payload, HttpMethod.Post, apiKey);
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
                if (subscription is null)
                    throw new ArgumentNullException(nameof(subscription));

                var apiKeyKey = $"{nameof(EcomailSettings)}.{nameof(EcomailSettings.ApiKey)}";
                var apiKey = await _settingService.GetSettingByKeyAsync<string>(apiKeyKey, storeId: subscription.StoreId);
                if (string.IsNullOrEmpty(apiKey))
                    apiKey = await _settingService.GetSettingByKeyAsync<string>(apiKeyKey);
                if (string.IsNullOrEmpty(apiKey))
                    throw new NopException("Plugin not configured");

                //get list identifier from the settings
                var listKey = $"{nameof(EcomailSettings)}.{nameof(EcomailSettings.ListId)}";
                var listId = await _settingService.GetSettingByKeyAsync<int>(listKey, storeId: subscription.StoreId);
                if (listId == 0)
                    listId = await _settingService.GetSettingByKeyAsync<int>(listKey);
                if (listId == 0)
                    throw new NopException("Contact list to synchronize not set");

                //ensure list exists
                await GetListByIdAsync(listId, apiKey);

                var payload = JsonConvert.SerializeObject(new
                {
                    email = subscription.Email
                });
                try
                {
                    var response = await _ecomailHttpClient
                        .RequestAsync(string.Format(EcomailDefaults.UnubscribeFromListEcomailApiUrl, listId), payload, HttpMethod.Delete, apiKey);
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
        /// Handle shopping cart changed event
        /// </summary>
        /// <param name="cartItem">Shopping cart item</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleShoppingCartChangedEventAsync(ShoppingCartItem cartItem)
        {
            await HandleFunctionAsync(async () =>
            {
                if (cartItem is null)
                    throw new ArgumentNullException(nameof(cartItem));

                if (string.IsNullOrEmpty(_ecomailSettings.ApiKey))
                    throw new NopException("Plugin not configured");

                if (!_ecomailSettings.UseTracking)
                    throw new NopException("Tracking not enabled");

                var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)
                    ?? throw new NopException("Primary store currency not found");

                var customer = await _customerService.GetCustomerByIdAsync(cartItem.CustomerId)
                    ?? throw new NopException("Customer not found");

                var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, cartItem.StoreId)
                    ?? throw new NopException("Subscription not found");

                //prepare cart data
                var trackEvent = new EcomailEvent
                {
                    Email = customer.Email ?? string.Empty,
                    Category = "ue",
                    Action = EcomailDefaults.BasketActionAttribute
                };

                //get current customer's shopping cart
                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
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
                        var (itemPrice, _) = await _taxService.GetProductPriceAsync(product, unitPrice, false, customer);

                        return new TrackCartProduct
                        {
                            ProductId = product.Id,
                            Name = product.Name ?? string.Empty,
                            Description = product.ShortDescription ?? string.Empty,
                            FullDescription = product.FullDescription ?? string.Empty,
                            Sku = sku ?? string.Empty,
                            Category = category?.Name ?? string.Empty,
                            Url = urlHelper.RouteUrl("Product", new { SeName = seName }, _webHelper.GetCurrentRequestProtocol()),
                            ImgUrl = url ?? string.Empty,
                            Quantity = item.Quantity,
                            Price = itemPrice
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
                                subtotal = cartSubtotal,
                                shipping = cartShipping ?? decimal.Zero,
                                tax = cartTax,
                                discount = cartDiscount
                            }
                        }
                    };

                    trackEvent.Label = EcomailDefaults.CartUpdatedEventName;
                    trackEvent.Value = JsonConvert.SerializeObject(data).ToString();
                }
                else
                {
                    //there are no items in the cart, so the cart is deleted
                    trackEvent.Label = EcomailDefaults.CartDeletedEventName;
                    trackEvent.Value = string.Empty;
                }

                var payload = JsonConvert.SerializeObject(new EcomailTrackEvent
                {
                    Event = trackEvent,
                });
                try
                {
                    var response = await _ecomailHttpClient.RequestAsync(EcomailDefaults.EcomailTrackEventApiUrl, payload, HttpMethod.Post);
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
        /// Handle order placed event
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleOrderPlacedEventAsync(Order order)
        {
            await HandleFunctionAsync(async () =>
            {
                if (order is null)
                    throw new ArgumentNullException(nameof(order));

                if (string.IsNullOrEmpty(_ecomailSettings.ApiKey))
                    throw new NopException("Plugin not configured");

                if (!_ecomailSettings.UseTracking)
                    throw new NopException("Tracking not enabled");

                var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId)
                    ?? throw new NopException("Customer not found");

                var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, order.StoreId)
                    ?? throw new NopException("Subscription not found");

                var store = await _storeService.GetStoreByIdAsync(order.StoreId);
                var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
                var country = await _countryService.GetCountryByIdAsync(billingAddress?.CountryId ?? 0);
                var timestamp = (int)new DateTimeOffset(order.CreatedOnUtc).ToUnixTimeSeconds();

                var transactionData = new TransactionData
                {
                    OrderId = order.Id,
                    Email = customer.Email ?? string.Empty,
                    Shop = store?.Url ?? string.Empty,
                    Amount = order.OrderTotal,
                    Tax = order.OrderTax,
                    Shipping = order.OrderShippingExclTax,
                    City = billingAddress?.City ?? string.Empty,
                    County = billingAddress?.County ?? string.Empty,
                    Country = country?.Name ?? string.Empty,
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
                        Price = orderItem.PriceInclTax,
                        Quantity = orderItem.Quantity,
                        Timestamp = timestamp,
                    };
                }).Where(item => item is not null).ToListAsync();

                var payload = JsonConvert.SerializeObject(new TransectionCreateRequest
                {
                    TransactionData = transactionData,
                    TransactionItems = transactionItems
                });
                try
                {
                    var response = await _ecomailHttpClient.RequestAsync(EcomailDefaults.AddTransactionDataToEcomailApiUrl, payload, HttpMethod.Post);
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
                var store = await _storeContext.GetCurrentStoreAsync();

                _nopFileProvider.CreateDirectory(_nopFileProvider.GetAbsolutePath(EcomailDefaults.FeedsDirectory));
                var fileName = string.Format(EcomailDefaults.FeedFileName, store.Id);
                var fullPath = _nopFileProvider.GetAbsolutePath(EcomailDefaults.FeedsDirectory, fileName);
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

        #endregion

        #endregion
    }
}