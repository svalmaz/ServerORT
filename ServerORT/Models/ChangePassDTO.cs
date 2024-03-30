namespace ServerORT.Models
{
    public class ChangePassDTO
    {
        public required int userId { get; set; }
        public required string userOldPass { get; set; }
        public required string userNewPass { get; set; }
    }
}
