namespace Stockroom.Web.Questions;

public sealed class QuestionOptions
{
    public bool Enabled { get; set; }

    public string Endpoint { get; set; } = "";

    public string Model { get; set; } = "";

    public string? ApiKey { get; set; }
}
