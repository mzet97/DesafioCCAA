namespace SistemaLivro.Shared.Settings;

public class CorsOptions
{
    public string[] Development { get; set; } = Array.Empty<string>();
    public string[] Production { get; set; } = Array.Empty<string>();
}
