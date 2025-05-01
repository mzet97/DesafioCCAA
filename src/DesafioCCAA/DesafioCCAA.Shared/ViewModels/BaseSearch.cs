namespace DesafioCCAA.Shared.ViewModels;

public abstract class BaseSearch
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public string? Order { get; set; }

    public int PageIndex { get; set; }
    public int PageSize { get; set; }

    protected BaseSearch()
    {
        PageIndex = 1;
        PageSize = 10;
    }

    public override string ToString()
    {
        return $"BaseSearch: Id={Id}, CreatedAt={CreatedAt:yyyy-MM-dd HH:mm:ss}, UpdatedAt={UpdatedAt:yyyy-MM-dd HH:mm:ss}, " +
               $"DeletedAt={(DeletedAt.HasValue ? DeletedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "null")}, " +
               $"Order={Order ?? "null"}, PageIndex={PageIndex}, PageSize={PageSize}";
    }
}

