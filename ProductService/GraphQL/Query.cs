using HotChocolate.AspNetCore.Authorization;
using OrderApp.Domain;

namespace ProductService.GraphQL
{
    public class Query
    {
        [Authorize]
        public async Task<IQueryable<Product>> GetProducts([Service] OrderDbContext context)
        {
            var ret = context.Products;
            return ret.AsQueryable();
        }
           
    }
}
