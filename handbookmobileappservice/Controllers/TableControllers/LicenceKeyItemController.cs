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

using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using handbookmobileappservice.DataObjects;
using handbookmobileappservice.Models;

namespace handbookmobileappservice.Controllers
{
    [Authorize]
    public class LicenceKeyItemController : TableController<LicenceKeyItem>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<LicenceKeyItem>(context, Request);
        }

        // GET tables/LicenceKeyItem
        public IQueryable<LicenceKeyItem> GetAllLicenceKeyItem()
        {
            return Query(); 
        }

        // GET tables/LicenceKeyItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<LicenceKeyItem> GetLicenceKeyItem(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/LicenceKeyItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<LicenceKeyItem> PatchLicenceKeyItem(string id, Delta<LicenceKeyItem> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/LicenceKeyItem
        public async Task<IHttpActionResult> PostLicenceKeyItem(LicenceKeyItem item)
        {
            LicenceKeyItem current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/LicenceKeyItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteLicenceKeyItem(string id)
        {
             return DeleteAsync(id);
        }
    }
}
