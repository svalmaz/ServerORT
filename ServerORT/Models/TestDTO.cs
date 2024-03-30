namespace ServerORT.Models
{
    public class TestDTO
    {
        public required string title { get; set; }
        public required string category { get; set; }
        public required string videoUrl { get; set; }
        public required string desc { get; set; }

    }
    public class QuestionDTO
    {
        public required string title { get; set; } = String.Empty;
        public required string question { get; set; }
        public required string a { get; set; } = String.Empty;
        public required string b { get; set; } = String.Empty;
        public required string c { get; set; } = String.Empty;
        public required string d { get; set; } = String.Empty;
        public required string answer { get; set; } = String.Empty;
        public required string explanation { get; set; } = String.Empty;
        public required int testId { get; set; }


    }
}
