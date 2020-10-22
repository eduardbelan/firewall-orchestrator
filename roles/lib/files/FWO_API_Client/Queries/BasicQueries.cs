﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using System.IO;
using FWO.Logging;

namespace FWO.ApiClient.Queries
{
    public class BasicQueries : Queries
    {
        public static readonly string getTenantId;
        public static readonly string getVisibleDeviceIdsPerTenant;
        public static readonly string getVisibleManagementIdsPerTenant;
        public static readonly string getLdapConnections;
        public static readonly string getManagementsDetails;
        public static readonly string getDeviceTypeDetails;
        public static readonly string newManagement;
        public static readonly string updateManagement;
        public static readonly string deleteManagement;

        static BasicQueries()
        {
            try
            {
                getTenantId = File.ReadAllText(QueryPath + "auth/getTenantId.graphql");

                getVisibleDeviceIdsPerTenant = File.ReadAllText(QueryPath + "auth/getVisibleDeviceIdsPerTenant.graphql");

                getVisibleManagementIdsPerTenant = File.ReadAllText(QueryPath + "auth/getVisibleManagementIdsPerTenant.graphql");

                getLdapConnections = File.ReadAllText(QueryPath + "auth/getLdapConnections.graphql");

                getManagementsDetails = File.ReadAllText(QueryPath + "device/getManagementsDetails.graphql");

                getDeviceTypeDetails = File.ReadAllText(QueryPath + "device/getDeviceTypeDetails.graphql");

                newManagement = File.ReadAllText(QueryPath + "device/newManagement.graphql");

                updateManagement = File.ReadAllText(QueryPath + "device/updateManagement.graphql");

                deleteManagement = File.ReadAllText(QueryPath + "device/deleteManagement.graphql");
            }
            catch (Exception exception)
            {
                Log.WriteError("Initialize Api Queries", "Api Basic Queries could not be loaded.", exception);
                Environment.Exit(-1);
            }
        }
    }
}
