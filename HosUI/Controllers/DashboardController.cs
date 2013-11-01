using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HosUI.Models;
using Microsoft.WindowsAzure;
using SqlRepository;

namespace HosUI.Controllers
{
    public class DashboardController : Controller
    {
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
                    DriverName = "NA",
                    DriverWorkStateId = dw.DriverWorkStateId,
                    Timestamp = dw.Timestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                    WorkStateId = dw.WorkStateId
                }).ToList();

            dashboard.Summaries = hosRepository.FindDriverSummariesForDrivers(dashboard.Workstates.Select(dw => dw.DriverId))
                .Select(ds => new Summary
                {
                    DriverId = ds.DriverId,
                    DriverName = "NA",
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
                    DriverName = "NA",
                    DriverWorkStateId = dw.DriverWorkStateId,
                    Timestamp = dw.Timestamp.ToString("dd/MM/yyyy HH:mm:ss"),
                    WorkStateId = dw.WorkStateId
                }).ToList();

            dashboard.Summaries = hosRepository.FindDriverSummariesForDrivers(dashboard.Workstates.Select(dw => dw.DriverId))
                .Select(ds => new Summary
                {
                    DriverId = ds.DriverId,
                    DriverName = "NA",
                    WorkStateId = ds.WorkStateId,
                    TotalSeconds = ds.TotalSeconds
                });

            return Json(dashboard, JsonRequestBehavior.AllowGet);
        }
    }
}