namespace HealthCareAI.Application.Models.OpenAI;

public class OpenAITranscriptionResponse
{
    public string Text { get; set; } = string.Empty;
}

public class OpenAIChatResponse
{
    public Choice[]? Choices { get; set; }
}

public class Choice
{
    public Message? Message { get; set; }
}

public class Message
{
    public string? Content { get; set; }
} 