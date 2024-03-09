namespace ServerORT.Models
{
    public class User
    {
        public int id { get; set; }
        public string login { get; set; } = string.Empty;
        public string pass {  get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;


    }
}
