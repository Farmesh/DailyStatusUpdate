using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using System.Linq;
using DailyStatusApp.Data;
using System.Collections.Generic;
using static DailyStatusApp.Data.ApplicationDbContxt;

namespace DailyStatusApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AdminController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var analyticsRaw = _context.DailyStatus
                .Where(d => !string.IsNullOrEmpty(d.EmployeeName))
                .GroupBy(d => d.EmployeeName)
                .Select(g => new
                {
                    EmployeeName = g.Key,
                    DailyEntries = g
                        .GroupBy(x => x.Date.Date) // Strip the time part for grouping
                        .Select(dg => new
                        {
                            Date = dg.Key,
                            Statuses = dg.Select(s => new
                            {
                                s.InProgressProject,
                                s.InProgressModule,
                                s.InProgressTask,
                                s.PlannedProject,
                                s.PlannedModule,
                                s.PlannedTask,
                                s.Blockage
                            }).ToList()
                        })
                        .OrderByDescending(d => d.Date)
                        .ToList()
                })
                .OrderBy(x => x.EmployeeName)
                .ToList();

            // Convert to dynamic objects for Razor compatibility
            var analytics = new List<ExpandoObject>();

            foreach (var item in analyticsRaw)
            {
                dynamic obj = new ExpandoObject();
                obj.EmployeeName = item.EmployeeName;
                obj.DailyEntries = new List<ExpandoObject>();

                foreach (var entry in item.DailyEntries)
                {
                    dynamic entryObj = new ExpandoObject();
                    entryObj.Date = entry.Date;
                    entryObj.Statuses = new List<ExpandoObject>();

                    foreach (var status in entry.Statuses)
                    {
                        dynamic statusObj = new ExpandoObject();
                        statusObj.InProgressProject = status.InProgressProject;
                        statusObj.InProgressModule = status.InProgressModule;
                        statusObj.InProgressTask = status.InProgressTask;
                        statusObj.PlannedProject = status.PlannedProject;
                        statusObj.PlannedModule = status.PlannedModule;
                        statusObj.PlannedTask = status.PlannedTask;
                        statusObj.Blockage = status.Blockage;

                        ((List<ExpandoObject>)entryObj.Statuses).Add(statusObj);
                    }

                    ((List<ExpandoObject>)obj.DailyEntries).Add(entryObj);
                }

                analytics.Add(obj);
            }

            ViewBag.Analytics = analytics;

            return View();
        }

        public IActionResult GetFilteredAnalytics(string employeeName, string date)
        {
            try
            {
                DateTime? filterDate = string.IsNullOrEmpty(date) ? (DateTime?)null : DateTime.Parse(date);

                var filteredAnalytics = _context.DailyStatus
                    .Where(d => !string.IsNullOrEmpty(d.EmployeeName))
                    .GroupBy(d => d.EmployeeName)
                    .Select(g => new
                    {
                        EmployeeName = g.Key,
                        DailyEntries = g
                            .Where(d => (string.IsNullOrEmpty(employeeName) || d.EmployeeName == employeeName) &&
                                        (!filterDate.HasValue || d.Date.Date == filterDate.Value.Date)) // Use Date for comparison
                            .GroupBy(x => x.Date.Date)  // Strip time part for grouping
                            .Select(dg => new
                            {
                                Date = dg.Key,
                                Statuses = dg.Select(s => new
                                {
                                    s.InProgressProject,
                                    s.InProgressModule,
                                    s.InProgressTask,
                                    s.PlannedProject,
                                    s.PlannedModule,
                                    s.PlannedTask,
                                    s.Blockage
                                }).ToList()
                            })
                            .OrderByDescending(d => d.Date)
                            .ToList()
                    })
                    .OrderBy(x => x.EmployeeName)
                    .ToList();

                if (filteredAnalytics.Count == 0)
                {
                    return PartialView("_NoDataFound");
                }
                return PartialView("_FilteredAnalytics", filteredAnalytics);
            }
            catch (Exception ex)
            {
                // Log the exception (you can log it to a file, database, or an error tracking service)
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Internal Server Error: " + ex.Message);
            }
        }
    }
}
