namespace blockchain_parser.Model
{
    public class Borrowers
    {
        public int BorrowerId { get; set; }
        public int UserId { get; set; }
        public Users User { get; set; }
    }
}