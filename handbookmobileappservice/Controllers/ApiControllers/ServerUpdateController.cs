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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using handbookmobileappservice.DataObjects;
using handbookmobileappservice.Models;
using handbookmobileappservice.Utilties;
using Microsoft.Azure.Mobile.Server.Config;
using Newtonsoft.Json;

namespace handbookmobileappservice.Controllers
{
    [Authorize]
    [MobileAppController]
    public class ServerUpdateController : BaseDbApiController
    {

        [HttpPost, Route("api/serverupdatejson")]
        public HttpResponseMessage ServerUpdate(HttpRequestMessage request, InitialUpdateJsonItem item)
        {
            string typename = item.Id;
            string typeinitialjson = item.UpdateJson;

            var existing = this.context.InitialUpdateJsonItems.Where(x => x.Id == typename).SingleOrDefault();

            if (existing == null)
            {
                return request.CreateBadRequestResponse();
            }

            var oldupdates = JsonConvert.DeserializeObject<UpdateJsonMessage>(existing.UpdateJson);

            existing.UpdateJson = typeinitialjson;

            this.context.SaveChanges();

            var newupdates = JsonConvert.DeserializeObject<UpdateJsonMessage>(item.UpdateJson);

            foreach (var x in oldupdates.AddBookItemIds)
            {
                newupdates.AddBookItemIds.Remove(x);
            }
            
            foreach (var x in oldupdates.DeleteBookItemIds)
            {
                newupdates.DeleteBookItemIds.Remove(x);
            }

            foreach (var x in oldupdates.AddFullpageItemIds)
            {
                newupdates.AddFullpageItemIds.Remove(x);
            }

            foreach (var x in oldupdates.DeleteFullpageItemIds)
            {
                newupdates.DeleteFullpageItemIds.Remove(x);
            }

            foreach (var userjson in this.context.UserUpdateStatusItems.Where(x => x.HandbookType == typename))
            {
                var current = JsonConvert.DeserializeObject<UpdateJsonMessage>(userjson.UpdateJson);

                current.AddBookItemIds.AddRange(newupdates.AddBookItemIds);
                current.AddBookItemIds = current.AddBookItemIds.Distinct().ToList();

                current.DeleteBookItemIds.AddRange(newupdates.DeleteBookItemIds);
                current.DeleteBookItemIds = current.DeleteBookItemIds.Distinct().ToList();

                current.AddFullpageItemIds.AddRange(newupdates.AddFullpageItemIds);
                current.AddFullpageItemIds = current.AddFullpageItemIds.Distinct().ToList();

                current.DeleteFullpageItemIds.AddRange(newupdates.DeleteFullpageItemIds);
                current.DeleteFullpageItemIds = current.DeleteFullpageItemIds.Distinct().ToList();

                userjson.UpdateJson = JsonConvert.SerializeObject(current);
            }

            this.context.SaveChanges();

            return request.CreateResponse(
                HttpStatusCode.OK
                );
        }
    }
}