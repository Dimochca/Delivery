using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }

    public Address(string street, string city, string postalCode, string country = "Russia")
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        Country = country;
    }

    public override string ToString()
    {
        return $"{PostalCode}, {Country}, {City}, {Street}";
    }
}

public class PhoneNumber
{
    public string CountryCode { get; }
    public string Number { get; }

    public PhoneNumber(string countryCode, string number)
    {
        CountryCode = countryCode;
        Number = number;
    }

    public override string ToString()
    {
        return $"+{CountryCode} {Number}";
    }
}

public abstract class Product
{
    public int Id { get; }
    public string Name { get; set; }
    public string Description { get; set; }
    public abstract decimal BasePrice { get; }
    public virtual decimal Price => BasePrice;

    protected Product(int id, string name, string description = "")
    {
        Id = id;
        Name = name;
        Description = description;
    }

    public abstract void DisplayInfo();
}

public class PhysicalProduct : Product
{
    public double Weight { get; set; }
    public override decimal BasePrice { get; }

    public PhysicalProduct(int id, string name, decimal basePrice, double weight, string description = "")
        : base(id, name, description)
    {
        BasePrice = basePrice;
        Weight = weight;
    }

    public override void DisplayInfo()
    {
        Console.WriteLine($"Физический товар: {Name} (Вес: {Weight}кг) - {Price:C}");
    }
}

public class DigitalProduct : Product
{
    public string DownloadLink { get; set; }
    public override decimal BasePrice { get; }

    public DigitalProduct(int id, string name, decimal basePrice, string downloadLink, string description = "")
        : base(id, name, description)
    {
        BasePrice = basePrice;
        DownloadLink = downloadLink;
    }

    public override void DisplayInfo()
    {
        Console.WriteLine($"Цифровой товар: {Name} - {Price:C}");
        Console.WriteLine($"Ссылка для скачивания: {DownloadLink}");
    }
}

public abstract class Delivery
{
    public Address Address { get; protected set; }
    public abstract string DeliveryType { get; }
    public abstract DateTime EstimatedDeliveryDate { get; }

    public abstract void DisplayDeliveryInfo();
}

public class HomeDelivery : Delivery
{
    public CourierService CourierService { get; }
    public string CourierPhone { get; set; }
    public override string DeliveryType => "Доставка на дом";
    public override DateTime EstimatedDeliveryDate => DateTime.Now.AddDays(3);

    public HomeDelivery(Address address, CourierService courierService)
    {
        Address = address;
        CourierService = courierService;
    }

    public override void DisplayDeliveryInfo()
    {
        Console.WriteLine($"Тип доставки: {DeliveryType}");
        Console.WriteLine($"Адрес: {Address}");
        Console.WriteLine($"Курьерская служба: {CourierService.Name}");
        Console.WriteLine($"Примерная дата доставки: {EstimatedDeliveryDate:dd.MM.yyyy}");
    }
}

public class PickPointDelivery : Delivery
{
    public string CompanyName { get; }
    public string PointId { get; }
    public override string DeliveryType => "Доставка в пункт выдачи";
    public override DateTime EstimatedDeliveryDate => DateTime.Now.AddDays(5);

    public PickPointDelivery(Address address, string companyName)
    {
        Address = address;
        CompanyName = companyName;
    }

    public override void DisplayDeliveryInfo()
    {
        Console.WriteLine($"Тип доставки: {DeliveryType}");
        Console.WriteLine($"Пункт выдачи: {Address}");
        Console.WriteLine($"Компания: {CompanyName}");
        Console.WriteLine($"Примерная дата доставки: {EstimatedDeliveryDate:dd.MM.yyyy}");
    }
}

public class ShopDelivery : Delivery
{
    public string ShopName { get; }
    public override string DeliveryType => "Доставка в магазин";
    public override DateTime EstimatedDeliveryDate => DateTime.Now.AddDays(2);

    public ShopDelivery(Address address, string shopName)
    {
        Address = address;
        ShopName = shopName;
    }

    public override void DisplayDeliveryInfo()
    {
        Console.WriteLine($"Тип доставки: {DeliveryType}");
        Console.WriteLine($"Магазин: {ShopName}");
        Console.WriteLine($"Адрес: {Address}");
        Console.WriteLine($"Примерная дата доставки: {EstimatedDeliveryDate:dd.MM.yyyy}");
    }
}

public class CourierService
{
    public string Name { get; }
    public PhoneNumber ContactPhone { get; }

    public CourierService(string name, PhoneNumber contactPhone)
    {
        Name = name;
        ContactPhone = contactPhone;
    }
}

public class OrderItem
{
    public Product Product { get; }
    public int Quantity { get; private set; }

    public decimal TotalPrice => Product.Price * Quantity;

    public OrderItem(Product product, int quantity = 1)
    {
        Product = product;
        Quantity = quantity;
    }

    public void IncreaseQuantity(int amount = 1)
    {
        if (amount > 0)
            Quantity += amount;
    }

    public void DecreaseQuantity(int amount = 1)
    {
        if (amount > 0 && Quantity - amount >= 0)
            Quantity -= amount;
    }
}

public class Order<TDelivery> where TDelivery : Delivery
{
    private static int _nextOrderNumber = 1;

    public int Number { get; }
    public TDelivery Delivery { get; }
    public string Description { get; set; }
    public DateTime OrderDate { get; }
    public OrderStatus Status { get; private set; }

    private List<OrderItem> _items;
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    public decimal TotalPrice => _items.Sum(item => item.TotalPrice);

    public OrderItem this[int productId]
    {
        get { return _items.FirstOrDefault(item => item.Product.Id == productId); }
    }

    public Order(TDelivery delivery, string description = "")
    {
        Number = _nextOrderNumber++;
        Delivery = delivery;
        Description = description;
        OrderDate = DateTime.Now;
        Status = OrderStatus.Created;
        _items = new List<OrderItem>();
    }

    public void AddProduct(Product product, int quantity = 1)
    {
        var existingItem = this[product.Id];
        if (existingItem != null)
        {
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            _items.Add(new OrderItem(product, quantity));
        }
    }

    public void RemoveProduct(int productId, int quantity = 1)
    {
        var item = this[productId];
        if (item != null)
        {
            item.DecreaseQuantity(quantity);
            if (item.Quantity == 0)
            {
                _items.Remove(item);
            }
        }
    }

    public void DisplayAddress()
    {
        Console.WriteLine(Delivery.Address);
    }

    public void DisplayOrderInfo()
    {
        Console.WriteLine($"Заказ №{Number}");
        Console.WriteLine($"Дата заказа: {OrderDate:dd.MM.yyyy HH:mm}");
        Console.WriteLine($"Статус: {Status}");
        Console.WriteLine($"Общая стоимость: {TotalPrice:C}");
        Console.WriteLine("Состав заказа:");

        foreach (var item in _items)
        {
            Console.WriteLine($"  {item.Product.Name} x {item.Quantity} = {item.TotalPrice:C}");
        }

        Delivery.DisplayDeliveryInfo();
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        Status = newStatus;
        Console.WriteLine($"Статус заказа №{Number} изменен на: {newStatus}");
    }
}

public enum OrderStatus
{
    Created,
    Confirmed,
    InDelivery,
    Delivered,
    Cancelled
}

public class OrderCollection
{
    private List<Order<Delivery>> _orders;

    public int Count => _orders.Count;

    public Order<Delivery> this[int index]
    {
        get
        {
            if (index >= 0 && index < _orders.Count)
                return _orders[index];
            throw new IndexOutOfRangeException();
        }
    }

    public OrderCollection()
    {
        _orders = new List<Order<Delivery>>();
    }

    public void AddOrder<TDelivery>(Order<TDelivery> order) where TDelivery : Delivery
    {
        _orders.Add(order as Order<Delivery>);
    }

    public IEnumerable<Order<Delivery>> GetOrdersByStatus(OrderStatus status)
    {
        return _orders.Where(order => order.Status == status);
    }
}

public static class OrderExtensions
{
    public static bool IsDelivered<TDelivery>(this Order<TDelivery> order) where TDelivery : Delivery
    {
        return order.Status == OrderStatus.Delivered;
    }

    public static decimal CalculateDeliveryCost<TDelivery>(this Order<TDelivery> order, decimal baseCost = 300m) where TDelivery : Delivery
    {
        if (order.Delivery is HomeDelivery homeDelivery)
        {
            return baseCost + 100m;
        }
        return baseCost;
    }
}

public class Program
{
    public static void Main()
    {
        var laptop = new PhysicalProduct(1, "Монитор ARDOR GAMING", 30000m, 2.5, "Игровой монитор");
        var gamingchair = new DigitalProduct(2, "Кресло ARDOR GAMING", 20000m, "Игровое кресло");

        var homeAddress = new Address("ул. Ленина, д. 123", "Томск", "101000");
        var shopAddress = new Address("ул. Сибирская, д. 47", "Томск", "101002");

        var courierService = new CourierService("CDEK", new PhoneNumber("7", "9991234567"));

        var homeDeliveryOrder = new Order<HomeDelivery>(
            new HomeDelivery(homeAddress, courierService),
            "Срочный заказ"
        );
        homeDeliveryOrder.AddProduct(laptop, 1);
        homeDeliveryOrder.AddProduct(gamingchair, 1);
        homeDeliveryOrder.DisplayOrderInfo();
        Console.WriteLine(new string('-', 50));
        Console.WriteLine($"Стоимость доставки домашнего заказа: {homeDeliveryOrder.CalculateDeliveryCost()}");
        Console.WriteLine($"Заказ доставлен: {homeDeliveryOrder.IsDelivered()}");
        homeDeliveryOrder.UpdateStatus(OrderStatus.Confirmed);
        homeDeliveryOrder.UpdateStatus(OrderStatus.InDelivery);
        var product = homeDeliveryOrder[1];
        if (product != null)
        {
            Console.WriteLine($"Найден товар с ID 1: {product.Product.Name}");
        }

        var orderCollection = new OrderCollection();
        orderCollection.AddOrder(homeDeliveryOrder);

        Console.WriteLine($"Всего заказов: {orderCollection.Count}");
    }
}
