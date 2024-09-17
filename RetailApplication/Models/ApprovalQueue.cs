namespace RetailApplication.Models
{
    public class ApprovalQueue
    {
        public int ApprovalQueueId { get; set; }
        public int ProductId { get; set; }
        public string RequestReason { get; set; }
        public DateTime RequestDate { get; set; }
        public string ActionType { get; set; }
    }

}
