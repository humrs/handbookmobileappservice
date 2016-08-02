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
    public class UpdateController : BaseDbApiController
    {
        [HttpPost, Route("api/updates")]
        public HttpResponseMessage Updates(HttpRequestMessage request)
        {

            string username = FromClaimsPrincipal.GetUsername((ClaimsPrincipal)this.User);
            if (string.IsNullOrWhiteSpace(username))
            {
                return createUpdateNoUsernameGivenMessage(request);
            }

            try
            {
                var updateCfg = getUserUpdateStatusItemByUser(this.context, username);
                if (updateCfg == null)
                {
                    return createUpdateNoUsernameFoundMessage(request, username);
                }

                UpdateJsonMessage ujm = JsonConvert.DeserializeObject<UpdateJsonMessage>(updateCfg.UpdateJson);
                var addfullpageitems = ujm.AddFullpageItemIds
                    .Select(i => this.context.FullpageItems.Where(p => p.Id == i))
                    .Select(j => CreateAddFullpageItem(j.First()))
                    .ToList();

                var addbookitems = ujm.AddBookItemIds
                    .Select(i => this.context.BookItems.Where(p => p.Id == i))
                    .Select(j => CreateAddBookItem(j.First()))
                    .ToList();

                var deletefullpageitems = ujm.DeleteFullpageItemIds
                    .Select(i => CreateDeleteFullpageItem(i))
                    .ToList();

                var deletebookitems = ujm.DeleteBookItemIds
                    .Select(i => CreateDeleteBookItem(i))
                    .ToList();

                var fulllist = addfullpageitems.Concat(addbookitems).Concat(deletefullpageitems).Concat(deletebookitems);

                return request.CreateResponse(
                    HttpStatusCode.OK,
                    fulllist
                    );
            }
            catch (Exception ex)
            {
                return createUpdateExceptionMessage(request, username, ex);
            }
        }


        [HttpPost, Route("api/postupdates")]
        public HttpResponseMessage PostUpdates(HttpRequestMessage request, UpdateJsonMessage ujm)
        {
            string username = FromClaimsPrincipal.GetUsername((ClaimsPrincipal)this.User);
            if (string.IsNullOrWhiteSpace(username))
            {
                return createUpdateNoUsernameGivenMessage(request);
            }

            try
            {
                var updateCfg = getUserUpdateStatusItemByUser(this.context, username);
                if (updateCfg == null)
                {
                    return createUpdateNoUsernameFoundMessage(request, username);
                }

                var oldUjm = JsonConvert.DeserializeObject<UpdateJsonMessage>(updateCfg.UpdateJson);
                var newUjm = new UpdateJsonMessage {
                    AddBookItemIds = oldUjm.AddBookItemIds.Except(ujm.AddBookItemIds).ToList(),
                    AddFullpageItemIds = oldUjm.AddFullpageItemIds.Except(ujm.AddFullpageItemIds).ToList(),
                    DeleteBookItemIds = oldUjm.DeleteBookItemIds.Except(ujm.DeleteBookItemIds).ToList(),
                    DeleteFullpageItemIds = oldUjm.DeleteFullpageItemIds.Except(ujm.DeleteFullpageItemIds).ToList()
                };
                var js = JsonConvert.SerializeObject(newUjm);
                updateCfg.UpdateJson = js;
                updateCfg.LastDateTimeChecked = DateTimeOffset.Now;
                this.context.SaveChanges();

                return request.CreateResponse(
                    HttpStatusCode.OK,
                    new UpdateResponseMessage {
                        Code = UpdateResponseMessageCode.Updated,
                        Message = string.Format("Updated")
                    });
            }
            catch (Exception ex)
            {
                return createUpdateExceptionMessage(request, username, ex);
            }
        }

        private HttpResponseMessage createUpdateNoUsernameGivenMessage(HttpRequestMessage request)
        {
            return request.CreateResponse(
                HttpStatusCode.BadRequest,
                new UpdateResponseMessage {
                    Code = UpdateResponseMessageCode.NoUserIdGiven,
                    Message = string.Format("No Username Given.")
                });
        }

        private HttpResponseMessage createUpdateNoUsernameFoundMessage(HttpRequestMessage request, string username)
        {
            return request.CreateResponse(
                HttpStatusCode.BadRequest,
                new UpdateResponseMessage {
                    Code = UpdateResponseMessageCode.NoUserIdFound,
                    Message = string.Format("No Username found. UserId = {0}", username)
                });
        }

        private HttpResponseMessage createUpdateExceptionMessage(HttpRequestMessage request, string username, Exception ex)
        {
            return request.CreateResponse(
                HttpStatusCode.BadRequest,
                new UpdateResponseMessage {
                    Code = UpdateResponseMessageCode.UpdatesException,
                    Message = string.Format("Exception. UserId = {0}: {1}", username, ex.Message.ToString())
                });
        }

        private static ServerUpdateMessage CreateAddFullpageItem(FullpageItem fp)
        {
            return new ServerUpdateMessage {
                Id = 1,
                Action = ServerUpdateMessage.AddFullpageActionId,
                FullPageID = fp.Id,
                FullPageTitle = fp.Title,
                FullPageContent = fp.Content
            };
        }

        private static ServerUpdateMessage CreateDeleteFullpageItem(string id)
        {
            return new ServerUpdateMessage {
                Id = 1,
                Action = ServerUpdateMessage.DeleteFullpageActionId,
                FullPageID = id
            };
        }

        private static ServerUpdateMessage CreateAddBookItem(BookItem bk)
        {
            return new ServerUpdateMessage {
                Id = 2,
                Action = ServerUpdateMessage.AddBookActionId,
                BookID = bk.Id,
                BookTitle = bk.Title,
                BookStartingID = bk.StartingId,
                BookOrder = bk.Order
            };
        }

        private static ServerUpdateMessage CreateDeleteBookItem(string id)
        {
            return new ServerUpdateMessage {
                Id = 2,
                Action = ServerUpdateMessage.DeleteBookActionId,
                BookID = id
            };
        }

        private static UserUpdateStatusItem getUserUpdateStatusItemByUser(MobileServiceContext db, string userId)
        {
            return db.UserUpdateStatusItems.Where(p => p.Id == userId).FirstOrDefault();
        }

    }
}
