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
    public class AppLogItemController : TableController<AppLogItem>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<AppLogItem>(context, Request);
        }

        // GET tables/AppLogItem
        public IQueryable<AppLogItem> GetAllAppLogItem()
        {
            return Query(); 
        }

        // GET tables/AppLogItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<AppLogItem> GetAppLogItem(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/AppLogItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<AppLogItem> PatchAppLogItem(string id, Delta<AppLogItem> patch)
        {
             return UpdateAsync(id, patch);
        }

        // POST tables/AppLogItem
        public async Task<IHttpActionResult> PostAppLogItem(AppLogItem item)
        {
            AppLogItem current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/AppLogItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteAppLogItem(string id)
        {
             return DeleteAsync(id);
        }
    }
}
