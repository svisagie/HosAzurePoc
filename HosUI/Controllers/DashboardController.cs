using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Web;
using System.Web.Mvc;
using HosUI.Models;
using Microsoft.ServiceBus;
using Microsoft.WindowsAzure;
using SqlRepository;

namespace HosUI.Controllers
{
    public class DashboardController : Controller
    {
        static ChannelFactory<IDriverChannel> channelFactory;

        static DashboardController()
        {
            ServiceBusEnvironment.SystemConnectivity.Mode = ConnectivityMode.Http;
            TransportClientEndpointBehavior sharedSecretServicebusCredential = new TransportClientEndpointBehavior();

            Uri serviceUri = ServiceBusEnvironment.CreateServiceUri("sb", CloudConfigurationManager.GetSetting("ServiceNamespace"), "DriverService");
            channelFactory = new ChannelFactory<IDriverChannel>("RelayEndpoint", new EndpointAddress(serviceUri));
            sharedSecretServicebusCredential.TokenProvider = TokenProvider.CreateSharedSecretTokenProvider(
                CloudConfigurationManager.GetSetting("IssuerName"),
                CloudConfigurationManager.GetSetting("IssuerSecret"));
            channelFactory.Endpoint.Behaviors.Add(sharedSecretServicebusCredential);
        }


        private string GetDriverName(int DriverId)
        {
            using (IDriverChannel channel = channelFactory.CreateChannel())
            {
                try
                {
                    return channel.GetDriver(DriverId).DriverName;
                }
                catch (Exception e)
                {
                    //TODO: Write Trace error somewhere
                    //Trace. Console.WriteLine("Error: {0}", e.Message);
                }
            }
            return "";

            //TODO: CLOSE FACTORY
            //channelFactory.Close();
        }


        //
        // GET: /Dashboard/
        public ActionResult Index()
        {
            var dashboard = new Dashboard();
            var hosRepository = new HosRepository(CloudConfigurationManager.GetSetting("SqlDbConnectionString"));


            dashboard.Workstates = hosRepository.FindLatestDriverWorkStates(30)
                .Select(dw => new Workstate
                {
                    DriverId = dw.DriverId,
                    DriverName = "",//GetDriverName(dw.DriverId),
                    DriverWorkStateId = dw.DriverWorkStateId,
                    Timestamp = dw.Timestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                    WorkStateId = dw.WorkStateId
                }).ToList();

            dashboard.Summaries = hosRepository.FindDriverSummariesForDrivers(
                dashboard.Workstates.Select(dw => dw.DriverId))
                .Select(ds => new Summary
                {
                    DriverId = ds.DriverId,
                    DriverName = "",//DriverName = GetDriverName(ds.DriverId),
                    WorkStateId = ds.WorkStateId,
                    TotalSeconds = ds.TotalSeconds
                });

            return View(dashboard);
        }

        public JsonResult Data()
        {
            var dashboard = new Dashboard();
            var hosRepository = new HosRepository(CloudConfigurationManager.GetSetting("SqlDbConnectionString"));


            dashboard.Workstates = hosRepository.FindLatestDriverWorkStates(30)
                .Select(dw => new Workstate
                {
                    DriverId = dw.DriverId,
                    DriverName = "",//DriverName = GetDriverName(dw.DriverId),
                    DriverWorkStateId = dw.DriverWorkStateId,
                    Timestamp = dw.Timestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                    WorkStateId = dw.WorkStateId
                }).ToList();

            dashboard.Summaries = hosRepository.FindDriverSummariesForDrivers(
                dashboard.Workstates.Select(dw => dw.DriverId))
                .Select(ds => new Summary
                {
                    DriverId = ds.DriverId,
                    DriverName = "",//DriverName = GetDriverName(ds.DriverId),
                    WorkStateId = ds.WorkStateId,
                    TotalSeconds = ds.TotalSeconds
                });


            return Json(dashboard, JsonRequestBehavior.AllowGet);
        }
    }
}