namespace PaginationDemo.Models
{
    [Flags]
    public enum Gender
    {
        Male = 0b_0000_0000,
        Female = 0b_0000_0001,
        Others = 0b_0000_0010
    }
}
