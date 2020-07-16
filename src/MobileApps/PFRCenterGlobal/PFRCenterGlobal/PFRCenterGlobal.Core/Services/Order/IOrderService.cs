using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PFRCenterGlobal.Core.Models.Basket;

namespace PFRCenterGlobal.Core.Services.Order
{
    public interface IOrderService
    {
        Task CreateOrderAsync(Models.Orders.Order newOrder, string token);
        Task<ObservableCollection<Models.Orders.Order>> GetOrdersAsync(string token);
        Task<Models.Orders.Order> GetOrderAsync(int orderId, string token);
        Task<bool> CancelOrderAsync(int orderId, string token);
        BasketCheckout MapOrderToBasket(Models.Orders.Order order);
    }
}