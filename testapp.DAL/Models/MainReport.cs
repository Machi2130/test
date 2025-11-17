namespace testapp.DAL.Models
{
    public class MainReport
    {
        public int ReportId { get; set; }
        public int OperationId { get; set; }
        public int SupervisorId { get; set; }
        public DateTime OperationDate { get; set; }
        public DateTime OperationStartTime { get; set; }
        public DateTime OperationEndTime { get; set; }
        public string UGST { get; set; } = "";
        public decimal InitialLevelAtUGST { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
