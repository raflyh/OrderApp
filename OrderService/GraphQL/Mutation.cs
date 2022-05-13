using HotChocolate.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrderApp.Domain;
using OrderApp.Domain.Helpers;
using System.Security.Claims;

namespace OrderService.GraphQL
{
    public class Mutation
    {
        [Authorize]
        public async Task<OrderData> AddOrderAsync(
            OrderData input,
            ClaimsPrincipal claimsPrincipal,
            [Service] OrderDbContext context)
        {
            using var transaction = context.Database.BeginTransaction();
            var userName = claimsPrincipal.Identity.Name;

            try
            {
                var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
                if (user != null)
                {
                    // EF
                    var order = new Order
                    {
                        Code = Guid.NewGuid().ToString(), // generate random chars using GUID
                        UserId = user.Id
                    };

                    foreach (var item in input.Details)
                    {
                        var detail = new OrderDetail
                        {
                            OrderId = order.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity
                        };
                        order.OrderDetails.Add(detail);
                    }
                    context.Orders.Add(order);
                    context.SaveChanges();
                    await transaction.CommitAsync();

                    //input.Id = order.Id;
                    //input.Code = order.Code;
                }
                else
                    throw new Exception("user was not found");
            }
            catch (Exception err)
            {
                transaction.Rollback();
            }
            return input;
        }

        [Authorize]
        public async Task<TransactionStatus> SubmitOrderAsync(
           OrderData input,
           //[Service] OrderQLContext context,
           [Service] IOptions<KafkaSettings> settings)
        {
            var key = "Submit-Order-" + DateTime.Now.ToString();
            var val = JsonConvert.SerializeObject(input);
            var result = await KafkaHelper.SendMessage(settings.Value, "SubmitOrder", key, val);

            var ret = new TransactionStatus(result, "Success to Submit Order");
            if (!result)
                ret = new TransactionStatus(result, "Failed to Submit Order");

            return await Task.FromResult(ret);
        }
    }
}
