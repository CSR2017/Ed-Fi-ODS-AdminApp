// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

//This file contains temporary type declarations in service of net48/netcoreapp3.1 cross-compilation.
//These types are intended to be removed as they are ported in full to .NET Core.

using System;
using EdFi.Ods.AdminApp.Management.Workflow;
using EdFi.Ods.AdminApp.Web.Infrastructure.Jobs;

namespace EdFi.Ods.AdminApp.Web.Controllers
{
    public class OdsInstanceSettingsController
    {
        public object Logging() => throw new System.NotImplementedException();
        public object Setup() => throw new System.NotImplementedException();
        public object BulkLoad() => throw new System.NotImplementedException();
        public object LearningStandards() => throw new System.NotImplementedException();
    }
}

namespace EdFi.Ods.AdminApp.Web.Infrastructure.IO
{
    public interface IFileUploadHandler { }
    public class LocalFileSystemFileUploadHandler : IFileUploadHandler { }
    public class BulkImportService
    {
        public event WorkflowStatusUpdated StatusUpdated
        {
            add { throw new System.NotImplementedException(); }
            remove { throw new System.NotImplementedException(); }
        }

        public WorkflowResult Execute(BulkUploadJobContext bulkUploadJobContext)
        {
            throw new System.NotImplementedException();
        }
    }
}

public class HandleErrorInfo
{
    public HandleErrorInfo(Exception exception)
    {
        Exception = exception;
    }

    public Exception Exception { get; }
}
