// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using EdFi.Admin.DataAccess.Contexts;
using EdFi.Ods.AdminApp.Management;
using EdFi.Ods.AdminApp.Web.Infrastructure.IO;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace EdFi.Ods.AdminApp.Web.Models.ViewModels.BulkUpload
{
    public class BulkFileUploadModel
    {
        public int? BulkFileType { get; set; }
        public IEnumerable<IFormFile> BulkFiles { get; set; }
        public const int MaxFileSize = 20000000;
        public bool CredentialsSaved => !string.IsNullOrEmpty(ApiKey);
        [Display(Name = "Api Key")]
        public string ApiKey { get; set; }
        [Display(Name = "Api Secret")]
        public string ApiSecret { get; set; }
        public bool IsJobRunning { get; set; }
        public bool IsSameOdsInstance { get; set; }
    }

    public class SaveBulkUploadCredentialsModel
    {
        [Display(Name = "Api Key")]
        public string ApiKey { get; set; }
        [Display(Name = "Api Secret")]
        public string ApiSecret { get; set; }
    }

    public class SaveBulkUploadCredentialsModelValidator : AbstractValidator<SaveBulkUploadCredentialsModel>
    {
        private readonly IUsersContext _usersContext;
        private readonly InstanceContext _instanceContext;

        public SaveBulkUploadCredentialsModelValidator(IUsersContext usersContext, InstanceContext instanceContext)
        {
            _usersContext = usersContext;
            _instanceContext = instanceContext;

            RuleFor(m => m.ApiKey).NotEmpty();
            RuleFor(m => m.ApiSecret).NotEmpty();
            RuleFor(m => m.ApiKey)
                .Must(BeAssociatedToTheSelectedInstance)
                .When(m => !string.IsNullOrEmpty(m.ApiKey))
                .WithMessage("The Api Key provided is not associated with the currently selected ODS instance.");
        }

        private bool BeAssociatedToTheSelectedInstance(SaveBulkUploadCredentialsModel model, string apiKey)
        {
            var apiClient = _usersContext.Clients
                .Include(x => x.Application.OdsInstance)
                .SingleOrDefault(x => x.Key == apiKey);

            var odsInstanceNameForGivenKey = apiClient?.Application?.OdsInstance?.Name;

            return odsInstanceNameForGivenKey != null && odsInstanceNameForGivenKey == _instanceContext.Name;
        }
    }

    public class BulkFileUploadModelValidator : AbstractValidator<BulkFileUploadModel>
    {
        private readonly IUsersContext _usersContext;

        public BulkFileUploadModelValidator(IUsersContext usersContext)
        {
            _usersContext = usersContext;            

            RuleFor(m => m.ApiKey)
                .Must(BeAssociatedWithValidApplication)
                .When(m => !string.IsNullOrEmpty(m.ApiKey))
                .WithMessage("Provided Api Key is not associated with any application. Please reset the credentials.");
        }

        private bool BeAssociatedWithValidApplication(BulkFileUploadModel model, string apiKey)
        {
            var apiClient = _usersContext.Clients
                .Include(x => x.Application)
                .SingleOrDefault(x => x.Key == apiKey);

            var associatedApplication = apiClient?.Application;

            return associatedApplication != null;
        }
    }
}
