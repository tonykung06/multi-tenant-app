using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using WebApp.Models;

[assembly: OwinStartup(typeof(WebApp.Startup))]
namespace WebApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Use(async (ctx, next) =>
            {
                Tenant tenant = GetTenantBasedOnUrl(ctx.Request.Uri.Host);
                if (tenant == null)
                {
                    throw new ApplicationException($"Tenant for {ctx.Request.Uri.Host} is not found");
                }
                ctx.Environment.Add("MultiTenant", tenant);
                await next();
            });
        }

        private Tenant GetTenantBasedOnUrl(string urlHost)
        {
            if (String.IsNullOrEmpty(urlHost))
            {
                throw new ApplicationException("urlHost must be specified");
            }

            Tenant tenant;

            string cacheName = "all-tenants-cache-name";
            int cacheTimeOutSeconds = 30;

            List<Tenant> tenants =
                new TCache<List<Tenant>>().Get(
                    cacheName, cacheTimeOutSeconds,
                    () =>
                    {
                        using (var context = new MultiTenantContext())
                        {
                            return context.Tenants.ToList();
                        }
                    }
                );
            tenant = tenants
                            .FirstOrDefault(a => a.DomainName.Equals(urlHost, StringComparison.InvariantCultureIgnoreCase)) ??
                                tenants.FirstOrDefault(a => a.Default);
            if (tenant == null)
            {
                throw new ApplicationException("tenant not found based on URL, no default found");
            }
            return tenant;
        }
    }
}
