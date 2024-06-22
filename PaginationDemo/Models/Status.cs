namespace PaginationDemo.Models
{
    [Flags]
    public enum Status
    {
        Created = 0b_0000_0000, // 0
        PendingApproval = 0b_0000_0001, // 1
        Approved = 0b_0000_0010, // 2
        Rejected = 0b_0000_0100 // 4
    }
}
