namespace ServerORT.Models
{
    public class Tests
    {
    }
    public class TestList
    {
        public int id { get; set; }
        public string title { get; set; } = String.Empty;
        public int place { get; set; } 
        public string status { get; set; } = String.Empty;
        public string category { get; set; } = String.Empty;
        public int teacherId { get; set; }
        public string videoUrl { get; set; } = String.Empty;
        public string description { get; set; } = String.Empty;
    }
    public class Question
    {
        public int id { get; set; }
        public string title { get; set; } = String.Empty;
        public string question { get; set; }
        public string a { get; set; } = String.Empty;
        public string b { get; set; } = String.Empty;
        public string c { get; set; } = String.Empty;
        public string d { get; set; } = String.Empty;
        public string answer { get; set; } = String.Empty;
        public string explanation { get; set; } = String.Empty;
        public int testId { get; set; } 


    }
}
