namespace ProductService.GraphQL
{
    public record ProductInput
    (
        int? Id,
        string Name,
        int Stock,
        int Price
    );
}
