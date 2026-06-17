// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultRegistry.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PartnerCarrier.Web.DependencyResolution
{
    using PartnerCarrier.Infrastructure.Contracts.Admin.AdminManagement;
    using PartnerCarrier.Infrastructure.Contracts.Setup;
    using PartnerCarrier.Infrastructure.Contracts.User;
    using PartnerCarrier.Infrastructure.Database;
    using PartnerCarrier.Infrastructure.Services.Admin.AdminManagement;
    using PartnerCarrier.Infrastructure.Services.Setup;
    using PartnerCarrier.Infrastructure.Services.User;
    using StructureMap.Configuration.DSL;
    using StructureMap.Graph;

    public class DefaultRegistry : Registry
    {
        #region Constructors and Destructors

        public DefaultRegistry()
        {
            Scan(
                scan =>
                {
                    scan.TheCallingAssembly();
                    scan.WithDefaultConventions();
                    scan.With(new ControllerConvention());
                });
            // var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Sample"].ConnectionString;
            //For<IExample>().Use<Example>();
            For<PartnerCarrier_DevEntities>().Transient().Use(() => new PartnerCarrier_DevEntities(false, null, true, null, null));
            For<IAccountService>().Transient().Use<AccountService>();
            For<IAdminManagementService>().Transient().Use<AdminManagementService>();
            For<ISqlManagerService>().Transient().Use<SqlManagerService>();
            For<IHomepageService>().Transient().Use<HomepageService>();
            For<IContactUsService>().Transient().Use<ContactUsService>();
            For<IBusinessMangementService>().Transient().Use<BusinessMangementService>();
        }

        #endregion
    }
}