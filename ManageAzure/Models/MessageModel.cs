namespace ManageAzure.Models
{
    public class MessageModel
    {
        public int WorkItemId { get; set; }
        public FileModel File { get; set; }

        public Ticket Ticket { get; set; }
    }

    public class Ticket
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Priority { get; set; }
        public string AssignedTo { get; set; }
        public string CreationUser { get; set; }
    }
}
