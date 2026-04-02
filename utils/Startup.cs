using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
    public class Startup
        public Startup(IConfiguration configuration)
            Configuration = configuration;
        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
            services.AddMvc(options =>
                    options.EnableEndpointRouting = false;
                .AddNewtonsoftJson();
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
                app.UseExceptionHandler("/Home/Error");
            app.UseStaticFiles();
            app.UseMvc(routes =>
                routes.MapRoute(
                    name: "resume_bytes",
                    template: "Resume/Bytes/{NumberBytes?}",
                    defaults: new { controller = "Resume", action = "Bytes" });
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
                    name: "redirect",
                    template: "Redirect/{count?}",
                    defaults: new { controller = "Redirect", action = "Index" });
                    name: "delay",
                    template: "Delay/{seconds?}",
                    defaults: new { controller = "Delay", action = "Index" });
                    name: "stall",
                    template: "Stall/{seconds?}/{contentType?}",
                    defaults: new { controller = "Delay", action = "Stall" });
                    name: $"stallbrotli",
                    template: "StallBrotli/{seconds?}/{contentType?}",
                    defaults: new { controller = "Delay", action = $"StallBrotli" });
                    name: $"stalldeflate",
                    template: "StallDeflate/{seconds?}/{contentType?}",
                    defaults: new { controller = "Delay", action = $"StallDeflate" });
                    name: $"stallgzip",
                    template: "StallGZip/{seconds?}/{contentType?}",
                    defaults: new { controller = "Delay", action = $"StallGZip" });
                    name: "post",
                    template: "Post",
                    defaults: new { controller = "Get", action = "Index" },
                    constraints: new RouteValueDictionary(new { httpMethod = new HttpMethodRouteConstraint("POST") }));
                    name: "put",
                    template: "Put",
                    constraints: new RouteValueDictionary(new { httpMethod = new HttpMethodRouteConstraint("PUT") }));
                    name: "patch",
                    template: "Patch",
                    constraints: new RouteValueDictionary(new { httpMethod = new HttpMethodRouteConstraint("PATCH") }));
                    name: "delete",
                    template: "Delete",
                    constraints: new RouteValueDictionary(new { httpMethod = new HttpMethodRouteConstraint("DELETE") }));
                    name: "retry",
                    template: "Retry/{sessionId?}/{failureCode?}/{failureCount?}",
                    defaults: new { controller = "Retry", action = "Retry" });
