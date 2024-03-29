﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace dell_switch_exporter
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var supportedCultures = new[]
            {
                new CultureInfo("en-US")
            };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            app.Run(async (context) =>
            {
                HttpRequest req = context.Request;

                string ip = req.Query["target"].ToString();
                string community = req.Query["community"].ToString();
                string result = string.Empty;

                if (ip != string.Empty && community != string.Empty)
                {
                    DellSwitch dellSwitch = new DellSwitch(ip, community);
                    IList<Interface> interfaces = dellSwitch.GetInterfaceInfo();

                    string InfoString = string.Empty;
                    string InOctetsString = string.Empty;
                    string OutOctetsString = string.Empty;
                    foreach (var i in interfaces)
                    {
                        if (InfoString == string.Empty)
                        {
                            InfoString += Prometheus.CreateMetricDescription(
                                "Interface_Information",
                                "gauge",
                                "The information of the interface."
                            );
                        }
                        InfoString += Prometheus.CreateMetric(
                            "Interface_Information",
                            i.OperStatus.ToString(),
                            "{id=\"" + i.Id + "\" interface=\"" + i.Name + "\", description=" + i.Description + ", adminStatus=\"" + i.AdminStatus.ToString() + "\", speed=\"" + i.Speed.ToString() + "\"}"
                        );

                        if (InOctetsString == string.Empty)
                        {
                            InOctetsString += Prometheus.CreateMetricDescription(
                                "Interface_InOctets",
                                "counter",
                                "The total number of octets received on the interface, including framing characters."
                            );
                        }
                        InOctetsString += Prometheus.CreateMetric(
                            "Interface_InOctets",
                            i.InOctets.ToString(),
                            "{interface=\"" + i.Name + "\", description=" + i.Description + "}"
                        );

                        if (OutOctetsString == string.Empty)
                        {
                            OutOctetsString += Prometheus.CreateMetricDescription(
                                "Interface_OutOctets",
                                "counter",
                                "The total number of octets transmited on the interface, including framing characters."
                            );
                        }
                        OutOctetsString += Prometheus.CreateMetric(
                            "Interface_OutOctets",
                            i.OutOctets.ToString(),
                            "{interface=\"" + i.Name + "\", description=" + i.Description + "}"
                        );
                    }
                    result += InfoString;
                    result += InOctetsString;
                    result += OutOctetsString;
                }
                else
                {
                    result = "Invalid request. Parameters target and community has required.";
                }

                await context.Response.WriteAsync(result);
            });
        }
    }
}
