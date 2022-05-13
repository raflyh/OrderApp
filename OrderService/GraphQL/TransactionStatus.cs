namespace OrderService.GraphQL
{
    public record TransactionStatus
    (
        bool IsSucceed,
        string? message
    );
}
