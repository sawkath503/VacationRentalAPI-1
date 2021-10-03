using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using VacationRental.Data.Interfaces;
using VacationRental.Data.Models;
using VacationRental.Data.Repos;
using VacationRental.Logic.Interfaces;
using VacationRental.Logic.Services;

namespace VacationRental.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSwaggerGen(opts => opts.SwaggerDoc("v1", new Info { Title = "Vacation rental information", Version = "v1" }));

            services.AddSingleton<IDictionary<int, RentalDataModel>>(new Dictionary<int, RentalDataModel>());
            services.AddSingleton<IDictionary<int, BookingDataModel>>(new Dictionary<int, BookingDataModel>());            

            services.AddScoped<IBookingRepo, BookingRepo>();
            services.AddScoped<IRentalRepo, RentalRepo>();

            services.AddTransient<IBookingService, BookingService>();
            services.AddTransient<ICalendarService, CalendarService>();
            services.AddTransient<IRentalService, RentalService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(opts => opts.SwaggerEndpoint("/swagger/v1/swagger.json", "VacationRental v1"));
        }
    }
}
