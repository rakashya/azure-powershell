﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using Microsoft.Azure.Common.Authentication;
using Microsoft.Azure.Common.Authentication.Models;
using Microsoft.Azure.Management.Sql;
using Microsoft.Azure.Management.Sql.Models;
using System;

namespace Microsoft.Azure.Commands.Sql.Security.Services
{
    /// <summary>
    /// This class is responsible for all the REST communication with the secure connection endpoints
    /// </summary>
    public class SecureConnectionEndpointsCommunicator
    {

        private static SqlManagementClient SqlClient { get; set; }             
        private static AzureSubscription Subscription {get ; set; }
        public AzureProfile Profile { get; set; }

        public SecureConnectionEndpointsCommunicator(AzureProfile profile , AzureSubscription subscription)
        {
            Profile = profile;
            if (subscription != Subscription)
            {
                Subscription = subscription;
                SqlClient = null;
            }
        }

        /// <summary>
        /// Get the secure connection policy for a specific database
        /// </summary>
        public SecureConnectionPolicy GetDatabaseSecureConnectionPolicy(string resourceGroupName, string serverName, string databaseName, string clientRequestId)
        {
            ISecureConnectionPolicyOperations operations = GetCurrentSqlClient(clientRequestId).SecureConnection;
            SecureConnectionPolicyGetResponse response = operations.Get(resourceGroupName, serverName, databaseName);
            return response.SecureConnectionPolicy;
        }

        /// <summary>
        /// Set (or create) the secure connection policy for a specific database
        /// </summary>
        public void SetDatabaseSecureConnectionPolicy(string resourceGroupName, string serverName, string databaseName, string clientRequestId, SecureConnectionPolicyCreateOrUpdateParameters parameters)
        {
            ISecureConnectionPolicyOperations operations = GetCurrentSqlClient(clientRequestId).SecureConnection;
            operations.CreateOrUpdate(resourceGroupName, serverName, databaseName, parameters);
        }

        /// <summary>
        /// Retrieve the SQL Management client for the currently selected subscription, adding the session and request
        /// id tracing headers for the current cmdlet invocation.
        /// </summary>
        /// <returns>The SQL Management client for the currently selected subscription.</returns>
        private SqlManagementClient GetCurrentSqlClient(String clientRequestId)
        {
            // Get the SQL management client for the current subscription
            if (SqlClient == null)
            {
                SqlClient = AzureSession.ClientFactory.CreateClient<SqlManagementClient>(Profile, Subscription, AzureEnvironment.Endpoint.ResourceManager);
            }
            SqlClient.HttpClient.DefaultRequestHeaders.Remove(Constants.ClientRequestIdHeaderName);
            SqlClient.HttpClient.DefaultRequestHeaders.Add(Constants.ClientRequestIdHeaderName, clientRequestId);
            return SqlClient;
        }
    }
}