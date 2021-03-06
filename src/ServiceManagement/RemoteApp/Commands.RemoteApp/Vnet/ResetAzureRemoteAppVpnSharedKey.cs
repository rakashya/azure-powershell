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

using Microsoft.Azure.Commands.RemoteApp;
using Microsoft.Azure.Management.RemoteApp;
using Microsoft.Azure.Management.RemoteApp.Models;
using System.Management.Automation;

namespace Microsoft.Azure.Management.RemoteApp.Cmdlets
{

    [Cmdlet(VerbsCommon.Reset, "AzureRemoteAppVpnSharedKey", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High), OutputType(typeof(VNet))]
    public class ResetAzureRemoteAppVpnSharedKey : RdsCmdlet
    {
        [Parameter(Mandatory = false,
           Position = 0,
           ValueFromPipeline = true,
           HelpMessage = "RemoteApp virtual network name.")]
        [ValidatePattern(VNetNameValidatorStringWithWildCards)]
        public string VNetName { get; set; }

        public override void ExecuteCmdlet()
        {
            OperationResultWithTrackingId response = null;
            string description = Commands_RemoteApp.VnetSharedKeyResetConfirmationDescription;
            string warning = Commands_RemoteApp.GenericAreYouSureQuestion;
            string caption = Commands_RemoteApp.VnetSharedKeyResetCaptionMessage;

            if (ShouldProcess(description, warning, caption))
            {
                response = CallClient(() => Client.VNet.ResetVpnSharedKey(VNetName), Client.VNet);
            }

            if (response != null)
            {
                VNetOperationStatusResult operationStatus = null;
                int maxRetries = 600; // 5 minutes?
                // wait for the reset key operation to succeed to get the new key
                do
                {
                    System.Threading.Thread.Sleep(5000); //wait a while before the next check
                    operationStatus = CallClient(() => Client.VNet.GetResetVpnSharedKeyOperationStatus(response.TrackingId), Client.VNet);

                }
                while (operationStatus.Status != VNetOperationStatus.Failed &&
                    operationStatus.Status != VNetOperationStatus.Success &&
                    --maxRetries > 0);

                if (operationStatus.Status == VNetOperationStatus.Success)
                {
                    VNetResult vnet = CallClient(() => Client.VNet.Get(VNetName, true), Client.VNet);
                    WriteObject(vnet.VNet);

                    WriteVerboseWithTimestamp("The request completed successfully.");
                }
                else
                {
                    if (maxRetries > 0)
                    {
                        WriteErrorWithTimestamp("The request failed.");
                    }
                    else
                    {
                        WriteErrorWithTimestamp("The request took a long time to complete.");
                    }
                }
            }
        }

    }
}
