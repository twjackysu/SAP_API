﻿using DotnetSdkUtilities.Factory.ResponseFactory;
using ManagePurchaseRequestInNS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using API.Common;
using API.Configuration;
using API.DTO.Request;
using API.Utilities;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace API.Controllers.SAPControllers
{
    [Route("api/SAP/[controller]/[action]")]
    [ApiController]
    public class ManagePurchaseRequestInController : ControllerBase
    {
        private readonly ILogger<ManagePurchaseRequestInController> _logger;
        private readonly IMyResponseFactory _myResponseFactory;
        private readonly IOptionsMonitor<Settings> _setting;

        public ManagePurchaseRequestInController(ILogger<ManagePurchaseRequestInController> logger, IMyResponseFactory myResponseFactory, IOptionsMonitor<Settings> setting)
        {
            _logger = logger;
            _myResponseFactory = myResponseFactory;
            _setting = setting;
        }

        /// <summary>
        /// 採購請求-一般庫存請購
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/SAP/ManagePurchaseRequestIn/GeneralInventoryPurchaseRequest
        ///     {
        ///        "Payload": {
        ///           "PurchaseRequestMaintainBundle": [
        ///              {
        ///                 "Item": [
        ///                    {
        ///                       "ItemID": "1",
        ///                       "ProductKeyItem": {
        ///                          "ProductTypeCode": "1",
        ///                          "ProductIdentifierTypeCode": "1",
        ///                          "ProductID": {
        ///                             "Value": "PECNMT0012"
        ///                          }
        ///                       },
        ///                       "TypeCode": "18",
        ///                       "Quantity": {
        ///                          "unitCode": "EA",
        ///                          "Value": 6
        ///                       },
        ///                       "DeliveryPeriod": {
        ///                          "StartDateTime": {
        ///                             "timeZoneCode": "UTC+8",
        ///                             "Value": "2024-04-20T20:00:00.1234567+08:00"
        ///                          }
        ///                       },
        ///                       "ShipToLocationID": {
        ///                          "Value": "AT"
        ///                       },
        ///                       "ListUnitPrice": {
        ///                          "Amount": {
        ///                             "currencyCode": "TWD",
        ///                             "Value": 1500
        ///                          },
        ///                          "BaseQuantity": {
        ///                             "unitCode": "EA",
        ///                             "Value": 1
        ///                          }
        ///                       },
        ///                       "CompanyIDParty": {
        ///                          "PartyID": {
        ///                             "Value": "AT"
        ///                          },
        ///                          "actionCode": 0,
        ///                          "actionCodeSpecified": true
        ///                       },
        ///                       "RequesterIDParty": {
        ///                          "PartyID": {
        ///                             "Value": "E999967"
        ///                          },
        ///                          "actionCode": 0,
        ///                          "actionCodeSpecified": true
        ///                       },
        ///                       "SupplierIDParty": {
        ///                          "PartyID": {
        ///                             "Value": "A01038"
        ///                          },
        ///                          "actionCode": 0,
        ///                          "actionCodeSpecified": true
        ///                       },
        ///                       "directMaterialIndicator": true,
        ///                       "directMaterialIndicatorSpecified": true,
        ///                       "Project_No": "ALICE-TESTAPI",
        ///                       "specnote": "產品規格_ALICE",
        ///                       "actionCode": 0,
        ///                       "actionCodeSpecified": true
        ///                    }
        ///                 ],
        ///                 "PURCREQUNIT": "AT-CS00",
        ///                 "actionCode": 0,
        ///                 "actionCodeSpecified": true
        ///              }
        ///           ]
        ///        },
        ///        "User": "Jacky"
        ///     }
        /// </remarks>
        [ProducesResponseType(typeof(ApiOkResponse<ManuallyPurchaseRequestResponse[]>), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse<ErrorCodes>), 400)]
        [ProducesResponseType(typeof(ApiErrorResponse<ErrorCodes>), 500)]
        [Produces("application/json")]
        [HttpPost]
        public async Task<IActionResult> GeneralInventoryPurchaseRequest([FromBody] GeneralInventoryPurchaseRequestRequest request, [FromHeader(Name = "API-Key")] string _, [FromHeader(Name = "Client-Credential-Option")] string? clientCredentialOption)
        {
            var endpointAddress = new EndpointAddress(_setting.CurrentValue.SAP.EndPoints.ManagePurchaseRequestIn);

            var binding = new CustomBinding(
                new MtomMessageEncodingBindingElement(),
                new HttpsTransportBindingElement
                {
                    AuthenticationScheme = System.Net.AuthenticationSchemes.Basic
                });

            _logger.LogInformation("api: {actionName}, user: {user}, request: {request}", ControllerContext.ActionDescriptor.ActionName, request.User, JsonConvert.SerializeObject(request));
            var client = new ManagePurchaseRequestInClient(binding, endpointAddress);
            var (userName, password) = CredentialHelper.GetCredentials(_setting, clientCredentialOption);

            client.ClientCredentials.UserName.UserName = userName;
            client.ClientCredentials.UserName.Password = password;

            var response = await client.MaintainBundleAsync(request.Payload);

            _logger.LogInformation("api: {actionName}, user: {user}, response: {response}", ControllerContext.ActionDescriptor.ActionName, request.User, JsonConvert.SerializeObject(response));
            if (response.PurchaseRequestMaintainBundleConfirmation?.PurchaseRequestResponse1 == null)
            {
                return _myResponseFactory.CreateErrorResponse(ErrorCodes.BadRequestInvalidData, JsonConvert.SerializeObject(response.PurchaseRequestMaintainBundleConfirmation?.Log.Item.Select(x => x.Note)));
            }
            else
            {
                return _myResponseFactory.CreateOKResponse(response.PurchaseRequestMaintainBundleConfirmation.PurchaseRequestResponse1);
            }
        }
    }
}
