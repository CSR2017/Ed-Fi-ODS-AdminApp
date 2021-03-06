// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EdFi.Ods.AdminApp.Management;
using EdFi.Ods.AdminApp.Management.Api;
using EdFi.Ods.AdminApp.Management.Helpers;
using EdFi.Ods.AdminApp.Management.Settings;
using EdFi.Ods.AdminApp.Web.Display.TabEnumeration;
using EdFi.Ods.AdminApp.Web.Infrastructure;
using EdFi.Ods.AdminApp.Web.Infrastructure.Jobs;
using EdFi.Ods.AdminApp.Web.Models.ViewModels;
using EdFi.Ods.AdminApp.Web.Models.ViewModels.OdsInstanceSettings;
using Microsoft.Extensions.Options;

namespace EdFi.Ods.AdminApp.Web.Controllers
{
    public class OdsInstanceSettingsController : ControllerBase
    {
        private readonly ICachedItems _cachedItems;
        private readonly ICloudOdsSettingsService _cloudOdsSettingsService;
        private readonly IGetProductionApiProvisioningWarningsQuery _getProductionApiProvisioningWarningsQuery;
        private readonly IOdsApiFacadeFactory _odsApiFacadeFactory;
        private readonly IProductionSetupJob _productionSetupJob;
        private readonly ITabDisplayService _tabDisplayService;
        private readonly InstanceContext _instanceContext;
        private readonly AppSettings _appSettings;

        public OdsInstanceSettingsController(
              IOdsApiFacadeFactory odsApiFacadeFactory
            , IGetProductionApiProvisioningWarningsQuery getProductionApiProvisioningWarningsQuery
            , ICachedItems cachedItems
            , IProductionSetupJob productionSetupJob
            , ICloudOdsSettingsService cloudOdsSettingsService
            , ITabDisplayService tabDisplayService 
            , InstanceContext instanceContext
            , IOptions<AppSettings> appSettingsAccessor
            )
        {
            _odsApiFacadeFactory = odsApiFacadeFactory;
            _getProductionApiProvisioningWarningsQuery = getProductionApiProvisioningWarningsQuery;
            _cachedItems = cachedItems;
            _productionSetupJob = productionSetupJob;
            _cloudOdsSettingsService = cloudOdsSettingsService;
            _tabDisplayService = tabDisplayService;
            _instanceContext = instanceContext;
            _appSettings = appSettingsAccessor.Value;
        }

        public async Task<ActionResult> Setup()
        {
            var setupCompleted = (await _odsApiFacadeFactory.Create()).DoesApiDataExist();

            if (setupCompleted) return RedirectToAction("SetupComplete");

            var model = new OdsInstanceSettingsModel
            {
                OdsInstanceSettingsTabEnumerations =
                    _tabDisplayService.GetOdsInstanceSettingsTabDisplay(OdsInstanceSettingsTabEnumeration.Setup)
            };

            return View(model);
        }

        public async Task<ActionResult> SetupComplete()
        {
            var defaultOdsInstance = await _cachedItems.GetDefaultCloudOdsInstance();

            var model = new OdsInstanceSettingsModel
            {
                ProductionSetupCompletedModel = new OdsInstanceSetupCompletedModel
                {
                    ProvisioningWarnings = await _getProductionApiProvisioningWarningsQuery.Execute(defaultOdsInstance)
                },
                OdsInstanceSettingsTabEnumerations =
                    _tabDisplayService.GetOdsInstanceSettingsTabDisplay(OdsInstanceSettingsTabEnumeration.Setup)
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Setup(OdsInstanceSettingsModel model)
        {
            if (CloudOdsAdminAppSettings.Instance.SystemManagedSqlServer)
                _productionSetupJob.EnqueueJob(1);

            return Ok();
        }

        public async Task<ActionResult> Logging()
        {
            var settings = await _cloudOdsSettingsService.GetSettings(_appSettings.DefaultOdsInstance);

            var model = new OdsInstanceSettingsModel
            {
                LogSettingsModel = new LogSettingsModel
                {
                    LogLevel = settings.LogLevel
                },
                OdsInstanceSettingsTabEnumerations =
                    _tabDisplayService.GetOdsInstanceSettingsTabDisplay(OdsInstanceSettingsTabEnumeration.Logging),
                OdsInstance = _instanceContext
            };

            return View(model);
        }
    }
}
