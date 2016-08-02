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

using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Security.Claims;


namespace handbookmobileappservice.Utilties
{
    public static class FromClaimsPrincipal
    {
        public static string GetUsername(ClaimsPrincipal claimsUser)
        {
            string provider = claimsUser.FindFirst("http://schemas.microsoft.com/identity/claims/identityprovider").Value;
            string sid =  claimsUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            return string.Format("{0}:{1}", provider, sid);
        }

        public static IEnumerable<Claim> GetClaims(ClaimsPrincipal claimsUser)
        {
            string provider = claimsUser.FindFirst("http://schemas.microsoft.com/identity/claims/identityprovider").Value;
            string sid = claimsUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            return new Claim[] {
                new Claim(JwtRegisteredClaimNames.Sub, sid),
                new Claim("http://schemas.microsoft.com/identity/claims/identityprovider", provider)
            };
        }
    }
}