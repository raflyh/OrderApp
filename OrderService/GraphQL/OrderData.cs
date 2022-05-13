namespace OrderService.GraphQL
{
    public class OrderData
    {
        public int? UserId { get; set; }

        public List<OrderDetailData> Details { get; set; }

    }
}
