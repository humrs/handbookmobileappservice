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
using System.Security.Claims;
using System.IdentityModel.Tokens;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Azure.Mobile.Server.Login;
using handbookmobileappservice.Utilties;
using Microsoft.Azure.Mobile.Server;
using System.Collections.Generic;
using handbookmobileappservice.Models;

namespace handbookmobileappservice.Controllers
{

    [MobileAppController]
    public class TokenController : ApiController
    {

        [Authorize]
        [HttpPost, Route("api/refreshtoken")]
        public HttpResponseMessage Refresh(HttpRequestMessage request)
        {

            string username = FromClaimsPrincipal.GetUsername((ClaimsPrincipal) this.User);
            var claims = FromClaimsPrincipal.GetClaims((ClaimsPrincipal) this.User);

            JwtSecurityToken refreshToken = this.GetAuthenticationTokenFromClaims(claims);
            return request.CreateResponse(
                HttpStatusCode.OK,
                new TokenResponseMessage {
                    Code = TokenResponseMessageCode.Refreshed,
                    Token = refreshToken.RawData,
                    Message = ""
                });
        }


        private JwtSecurityToken GetAuthenticationTokenFromClaims(IEnumerable<Claim> claims)
        {
            MobileAppSettingsDictionary settings = this.Configuration.GetMobileAppSettingsProvider().GetMobileAppSettings();

            var signingKey = SigningKey.GetKey();
            var audience = settings["ValidAudience"];
            var issuer = settings["ValidIssuer"];

            JwtSecurityToken token = AppServiceLoginHandler.CreateToken(
                claims,
                signingKey,
                audience,
                issuer,
                TimeSpan.FromDays(30)
                );

            return token;
        }

    }
}
