//
//  Copyright 2016  R. Stanley Hum <r.stanley.hum@gmail.com>
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using handbookmobileappservice.DataObjects;
using handbookmobileappservice.Models;
using handbookmobileappservice.Utilties;
using Microsoft.Azure.Mobile.Server.Config;


namespace handbookmobileappservice.Controllers
{
    [Authorize]
    [MobileAppController]
    public class AppLogBulkController : BaseDbApiController
    {


        [HttpPost, Route("api/loadapplog")]
        public HttpResponseMessage LoadAppLog(HttpRequestMessage request, List<AppLogItemMessage> items)
        {
            string username = FromClaimsPrincipal.GetUsername((ClaimsPrincipal)this.User);

            List<AppLogItem> logitems = new List<AppLogItem>();
            foreach (var item in items)
            {
                AppLogItem logitem = new AppLogItem {
                    Id = Guid.NewGuid().ToString(),
                    UserId = username,
                    LogDateTime = DateTimeOffset.Parse(item.LogDateTime),
                    LogName = item.LogName,
                    LogDataJson = item.LogDataJson
                };
                logitems.Add(logitem);
            }

            try
            {
                this.context.AppLogItems.AddRange(logitems);
                this.context.SaveChanges();
            }
            catch (Exception ex)
            {
                return request.CreateResponse(
                    HttpStatusCode.BadRequest,
                    new AppLogBulkResponseMessage {
                        Code = AppLogBulkResponseCode.Exception,
                        Message = string.Format("LoadAppLog: Exception: {0}", ex.Message.ToString())
                    });
            }

            return request.CreateResponse(
                HttpStatusCode.OK,
                new AppLogBulkResponseMessage {
                    Code = AppLogBulkResponseCode.Success,
                    Message = "AppLogBulkResponseMessage Success"
                });
        }
    }
}
