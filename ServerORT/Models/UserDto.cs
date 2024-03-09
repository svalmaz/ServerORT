namespace ServerORT.Models
{
    public class UserRegisterDto
    {
        public required string userName {  get; set; }
        public required string userPass {  get; set; }
        public required string userEmail { get; set; }
        public required string userStatus { get; set; }

    }
    public class UserLoginDto
    {
        public required string userName { get; set; }
        public required string userPass { get; set; }
      

    }
}
