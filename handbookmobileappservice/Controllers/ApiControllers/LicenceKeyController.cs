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


namespace handbookmobileappservice.Controllers
{
    [Authorize]
    [MobileAppController]
    public class LicenceKeyController : BaseDbApiController
    {

        [HttpPost, Route("api/verifylicencekey")]
        public HttpResponseMessage Verify(HttpRequestMessage request, VerifyLicenceKeyMessage vlkm)
        {
            string username = FromClaimsPrincipal.GetUsername((ClaimsPrincipal) this.User);
            if (string.IsNullOrWhiteSpace(username))
            {
                return request.CreateResponse(
                    HttpStatusCode.BadRequest,
                    new LicenceKeyResponseMessage {
                        Code = LicenceKeyResponseMessageCode.NoUserIdGiven,
                        Message = string.Format("No Username Given. LicenceKey = {0}", vlkm.LicenceKey)
                    });
            }

            try
            {
                var lkCfg = getLicenceKey(this.context, vlkm.LicenceKey);
                if (lkCfg == null)
                {
                    return request.CreateResponse(
                        HttpStatusCode.BadRequest,
                        new LicenceKeyResponseMessage {
                            Code = LicenceKeyResponseMessageCode.NoLicenceKeyFound,
                            Message = string.Format("No licence key found. LicenceKey = {0}, UserId = {1}", vlkm.LicenceKey, username)
                        });
                }

                if (String.IsNullOrWhiteSpace(lkCfg.UserId))
                {
                    lkCfg.UserId = username;
                    var iuj = this.context.InitialUpdateJsonItems.Where(p => p.Id == lkCfg.HandbookType).FirstOrDefault();
                    if (iuj == null)
                    {
                        return request.CreateResponse(
                            HttpStatusCode.BadRequest,
                            new LicenceKeyResponseMessage {
                                Code = LicenceKeyResponseMessageCode.InitialUpdateJsonUnavailable,
                                Message = string.Format("Licence Key Verified BUT UpdateJson unavailable: LicenceKey = {0}, UserId = {1}, HandbookType = {2}", vlkm.LicenceKey, username, lkCfg.HandbookType)
                            });
                    }

                    if (this.context.UserUpdateStatusItems.Where(p => p.Id == username).FirstOrDefault() != null)
                    {
                        return request.CreateResponse(
                            HttpStatusCode.BadRequest,
                            new LicenceKeyResponseMessage {
                                Code = LicenceKeyResponseMessageCode.UpdateStatusUserIdInUse,
                                Message = string.Format("Licence Key Verified BUT username already in use: LicenceKey = {0}, UserId = {1}", vlkm.LicenceKey, username)
                            });
                    }

                    var uus = new UserUpdateStatusItem {
                        Id = username,
                        UpdateNeeded = true,
                        HandbookType = lkCfg.HandbookType,
                        UpdateJson = iuj.UpdateJson
                    };

                    this.context.UserUpdateStatusItems.Add(uus);

                    this.context.SaveChanges();

                    return request.CreateResponse(
                        HttpStatusCode.OK,
                        new LicenceKeyResponseMessage {
                            Code = LicenceKeyResponseMessageCode.NewUserCreated,
                            Message = string.Format("Licence Key Verified: LicenceKey = {0}, UserId = {1}. New User created.", vlkm.LicenceKey, username)
                        });
                }

                if(lkCfg.UserId == username)
                {
                    return request.CreateResponse(
                        HttpStatusCode.OK,
                        new LicenceKeyResponseMessage {
                            Code = LicenceKeyResponseMessageCode.Licensed,
                            Message = string.Format("Licence Key Verified: LicenceKey = {0}, UserId = {1}. User logged in.", vlkm.LicenceKey, username)
                        });
                }

                return request.CreateResponse(
                    HttpStatusCode.BadRequest,
                    new LicenceKeyResponseMessage {
                        Code = LicenceKeyResponseMessageCode.LicenceKeyInUse,
                        Message = string.Format("Licence key taken. LicenceKey = {0}, UserId = {1}", vlkm.LicenceKey, username)
                    });
            }
            catch (Exception ex)
            {
                return request.CreateResponse(
                    HttpStatusCode.BadRequest,
                    new LicenceKeyResponseMessage {
                        Code = LicenceKeyResponseMessageCode.VerifyLicenceKeyException,
                        Message = string.Format("Exception: LicenceKey = {0}, UserId = {1}: {2}", vlkm.LicenceKey, username, ex.Message.ToString()) }
                    );
            }
            
        }


        private static LicenceKeyItem getLicenceKey(MobileServiceContext db, string v)
        {
            return db.LicenceKeyItems.Where(p => p.Id.ToUpper() == v.ToUpper()).FirstOrDefault();
        }
    }
}
