namespace DesafioCCAA.Shared.Settings;

public class ElasticSettings
{
    public string Uri { get; set; } = "http://localhost:9200";
    public string Username { get; set; } = "elastic";
    public string Password { get; set; } = "changeme";
    public string DataSet { get; set; } = "desafio";
}