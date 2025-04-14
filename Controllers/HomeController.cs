using Microsoft.AspNetCore.Mvc;
using DailyStatusApp.Models;
using DailyStatusApp.Data;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Linq;
using System.Threading.Tasks;
using static DailyStatusApp.Data.ApplicationDbContxt;

namespace DailyStatusApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public HomeController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            // Retrieve email from session if available
            ViewBag.Email = HttpContext.Session.GetString("Email");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitStatus(DailyStatus dailyStatus)
        {
            // Store the email in session
            //HttpContext.Session.SetString("Email", dailyStatus.SubmittedByEmail);

            // Check if the status has already been submitted today
            var existingStatus = _context.DailyStatus
                .FirstOrDefault(d => d.SubmittedByEmail == dailyStatus.SubmittedByEmail && d.Date.Date == DateTime.Now.Date);

            if (existingStatus != null)
            {
                ViewBag.Message = "You have already submitted your status for today.";
                return View("Index");
            }

            // Save the status to the database
            dailyStatus.Date = DateTime.Now;
            _context.DailyStatus.Add(dailyStatus);
            await _context.SaveChangesAsync();

            // Send an email to the fixed recipients
            var emailMessage = new MimeMessage();

            // Use the email from the 'dailyStatus' parameter (not from 'DailyStatus.SubmittedByEmail')
            emailMessage.From.Add(new MailboxAddress("Daily Status App", dailyStatus.SubmittedByEmail)); 

            // Add the recipient(s)
            emailMessage.To.Add(new MailboxAddress("", "thakurfarmesh123@gmail.com"));
             emailMessage.To.Add(new MailboxAddress("", "vermashreya216@gmail.com"));
            // emailMessage.To.Add(new MailboxAddress("", "manager3@example.com"));

            // Set the subject of the email
            emailMessage.Subject = $"DWR-{DateTime.Now:dd-MM-yy}";

            // Compose the email body
            emailMessage.Body = new TextPart("plain")
            {
                Text = $"📝 Daily Status Report\n" +
       $"====================\n\n" +
       $"👤 Employee: {dailyStatus.EmployeeName}\n" +
       $"🔑 Employee ID: {dailyStatus.EmployeeId}\n\n" +

    
       $"{(string.IsNullOrEmpty(dailyStatus.InProgressProject) ? "" : $"💼 In Progress - Project: {dailyStatus.InProgressProject}\n")}" +
       $"{(string.IsNullOrEmpty(dailyStatus.InProgressModule) ? "" : $"📦 In Progress - Module: {dailyStatus.InProgressModule}\n")}" +
       $"{(string.IsNullOrEmpty(dailyStatus.InProgressTask) ? "" : $"🔨 In Progress - Task: {dailyStatus.InProgressTask}\n \n ")}" +

       $"{(string.IsNullOrEmpty(dailyStatus.PlannedProject) ? "" : $"📅 Planned for Tomorrow - Project: {dailyStatus.PlannedProject}\n")}" +
       $"{(string.IsNullOrEmpty(dailyStatus.PlannedModule) ? "" : $"🗂️ Planned for Tomorrow - Module: {dailyStatus.PlannedModule}\n")}" +
       $"{(string.IsNullOrEmpty(dailyStatus.PlannedTask) ? "" : $"📌 Planned for Tomorrow - Task: {dailyStatus.PlannedTask}\n \n")}" +

     
       $"{(string.IsNullOrEmpty(dailyStatus.Blockage) ? "" : $"🚧 Blockage: {dailyStatus.Blockage}\n")}\n\n" 
            };


            using (var smtpClient = new SmtpClient())  // Use SmtpClient from MailKit.Net.Smtp
            {
                // Connect to the SMTP server (Gmail in this case)
                await smtpClient.ConnectAsync("smtp.gmail.com", 587, false);

                // Authenticate with your Gmail credentials
                await smtpClient.AuthenticateAsync("thakurfarmesh123@gmail.com", "uqrt wxhv gkpz nbso");

                // Send the email
                await smtpClient.SendAsync(emailMessage);

                // Disconnect from the SMTP server
                await smtpClient.DisconnectAsync(true);
            }

       
            ViewBag.Message = "Your daily status has been successfully submitted and emailed.";

            return View("Index");
        }
    }
}
