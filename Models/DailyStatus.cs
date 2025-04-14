namespace DailyStatusApp.Models
{
    public class DailyStatus
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public string? EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public string? InProgressProject { get; set; }
        public string? InProgressModule { get; set; }
        public string? InProgressTask { get; set; }
        public string? PlannedProject { get; set; }
        public string? PlannedModule { get; set; }
        public string? PlannedTask { get; set; }
        public string? Blockage { get; set; }
        public string SubmittedByEmail { get; set; }
    }
}
