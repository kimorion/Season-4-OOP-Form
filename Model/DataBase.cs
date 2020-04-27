using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Program.Promotion;

namespace Program
{
    public delegate void DiscountEventHandler(uint orderNumber, string discountName, string reason);

    class Database
    {
        public event Action StateChanged;
        public event DiscountEventHandler DiscountDenied;
        public event Action<string> UserWarning;

        private Dictionary<string, Customer> customers;
        private Dictionary<string, Item> items;
        private Dictionary<string, Discount> discounts;

        public bool IsAvailable
        {
            get { return customers != null && items != null; }
        }

        private void InitializeDiscount(Discount discount)
        {
            discounts.Add(discount.Name, discount);
        }

        public void Initialize()
        {
            customers = new Dictionary<string, Customer>();
            items = new Dictionary<string, Item>();
            discounts = new Dictionary<string, Discount>();

            InitializeDiscount(new OrderCostDiscount_1K());
            InitializeDiscount(new OrderCostDiscount_1d5K());
            InitializeDiscount(new OrderCostPremiumDiscount_1d5K());
            InitializeDiscount(new PremiumCustomerDiscount());
            InitializeDiscount(new WinterDiscount());
        }

        public void Reset()
        {
            customers = null;
            items = null;
            discounts = null;
        }

        public void CheckCustomerDiscounts(string id)
        {
            if (!customers.TryGetValue(id, out Customer customer))
                throw new KeyNotFoundException();
            foreach (var order in customer.OrderManager.Orders)
            {
                Dictionary<string, Discount> oldDiscounts = order.discounts;
                order.discounts = new Dictionary<string, Discount>();

                foreach (var discount in oldDiscounts.Values)
                {
                    var checkResult = discount.Check(customer, order);
                    if (!checkResult.Item1)
                    {
                        DiscountDenied?.Invoke(order.Number, discount.Name, checkResult.Item2);
                    }
                    else order.discounts.Add(discount.Family, discount);
                }
            }
        }

        // Customer

        public bool TryEditPrivilege(string id, Privilege newPrivilege)
        {
            if (!customers.TryGetValue(id, out Customer customer))
            {
                UserWarning?.Invoke("Клиент с указанным id не найден в базе");
                return false;
            }

            customer.Privilege = newPrivilege;
            CheckCustomerDiscounts(id);
            StateChanged?.Invoke();
            return true;
        }

        public bool TryEditName(string id, FullName name)
        {
            if (!customers.TryGetValue(id, out Customer customer))
            {
                UserWarning?.Invoke("Клиент с указанным id не найден в базе");
                return false;
            }

            customer.Name = name;
            StateChanged?.Invoke();
            return true;
        }

        public bool TryEditPhoneNumber(string id, string newNumber)
        {
            if (!customers.TryGetValue(id, out Customer customer))
            {
                UserWarning?.Invoke("Клиент с указанным id не найден в базе");
                return false;
            }

            customer.ContactPhone = newNumber;
            StateChanged?.Invoke();
            return true;
        }

        public bool TryAddOrder(string id, Order order)
        {
            if (!customers.TryGetValue(id, out Customer customer))
            {
                UserWarning?.Invoke("Клиент с указанным id не найден в базе");
                return false;
            }
            customer.OrderManager.AddOrder(order);
            StateChanged?.Invoke();
            return true;
        }

        public bool TryDeleteOrder(string id, uint orderNumber)
        {
            if (!customers.TryGetValue(id, out Customer customer))
            {
                UserWarning?.Invoke("Клиент с указанным id не найден в базе");
                return false;
            }
            if (!customer.OrderManager.ContainsOrder(orderNumber))
            {
                UserWarning?.Invoke("У клиента нет заказа с указанным номером");
                return false;
            }
            customer.OrderManager.RemoveOrder(orderNumber);
            StateChanged?.Invoke();
            return true;
        }

        public void AddCustomer(Customer customer)
        {
            if (customer == null)
                throw new Exception("Attempt to add null customer to the DB");
            if (customers == null)
            {
                UserWarning("База данных не инициализирована");
                return;
            }
            customers.Add(customer.ID, customer);
            StateChanged?.Invoke();
        }

        public bool TryGetCustomer(string id, out Customer customer)
        {
            customer = null;
            if (customers == null)
            {
                UserWarning("База данных не инициализирована");
                return false;
            }
            var result = customers.TryGetValue(id, out Customer original);
            customer = original?.Clone() as Customer;
            return result;
        }

        public List<Customer> GetCustomers()
        {
            return customers.Values.ToList().Clone() as List<Customer>;
        }

        public IEnumerable<Customer> Customers
        {
            get
            {
                if (customers == null)
                {
                    UserWarning("База данных не инициализирована");
                    yield break;
                }
                foreach (var customer in customers)
                {
                    yield return customer.Value.Clone() as Customer;
                }
            }
        }


        // Item

        public void AddItem(Item item)
        {
            if (item == null)
                throw new Exception("Attempt to add null item to the DB");
            if (items == null)
            {
                UserWarning("База данных не инициализирована");
                return;
            }
            items.Add(item.Article, item);
            StateChanged?.Invoke();
        }

        public Item GetItem(string article)
        {
            if (items == null)
            {
                UserWarning("База данных не инициализирована");
                return null;
            }
            items.TryGetValue(article, out Item item);
            return item?.Clone() as Item;
        }

        public IEnumerable<Item> Items
        {
            get
            {
                if (items == null)
                {
                    UserWarning("База данных не инициализирована");
                    yield break;
                }
                foreach (var item in items)
                {
                    yield return item.Value.Clone() as Item;
                }
            }
        }

        public List<Item> GetItems()
        {
            if (items == null)
            {
                UserWarning("База данных не инициализирована");
                return null;
            }
            return items.Values.ToList().Clone() as List<Item>;
        }

        public bool DeleteItem(string itemArticle)
        {
            if (!items.ContainsKey(itemArticle))
            {
                UserWarning?.Invoke("В базе нет товара с данным артикулом");
                return false;
            }
            items.Remove(itemArticle);
            StateChanged?.Invoke();
            return true;
        }

        // Order

        public bool TryAddItemToOrder(string id, uint orderNumber, string itemArticle, uint quantity)
        {
            if (!customers.TryGetValue(id, out Customer customer))
            {
                UserWarning?.Invoke("Клиент с указанным id не найден в базе");
                return false;
            }

            if (!customer.OrderManager.TryGetOrder(orderNumber, out Order order))
            {
                UserWarning?.Invoke("Заказ с указанным номером не найден в базе");
                return false;
            }

            if (order.state != OrderState.Formation)
            {
                UserWarning?.Invoke("Состав заказа можно менять только во время формирования заказа");
                return false;
            }

            if (!items.TryGetValue(itemArticle, out Item item))
            {
                UserWarning?.Invoke("В базе нет товара с данным артикулом");
                return false;
            }

            order.AddItem(item, quantity);
            CheckCustomerDiscounts(id);
            StateChanged?.Invoke();
            return true;
        }

        public bool TryDeleteItemFromOrder(string id, uint orderNumber, string itemArticle)
        {
            if (!customers.TryGetValue(id, out Customer customer))
            {
                UserWarning?.Invoke("Клиент с указанным id не найден в базе");
                return false;
            }

            if (!customer.OrderManager.TryGetOrder(orderNumber, out Order order))
            {
                UserWarning?.Invoke("Заказ с указанным номером не найден в базе");
                return false;
            }

            if (order.state != OrderState.Formation)
            {
                UserWarning?.Invoke("Состав заказа можно менять только во время формирования заказа");
                return false;
            }

            if (!items.TryGetValue(itemArticle, out Item item))
            {
                UserWarning?.Invoke("В базе нет товара с данным артикулом");
                return false;
            }

            order.DeleteItem(item);
            CheckCustomerDiscounts(id);
            StateChanged?.Invoke();
            return true;
        }

        public bool TryEditItemQuantity(string id, uint orderNumber, string itemArticle, uint newQuantity)
        {
            if (!customers.TryGetValue(id, out Customer customer))
            {
                UserWarning?.Invoke("Клиент с указанным id не найден в базе");
                return false;
            }

            if (!customer.OrderManager.TryGetOrder(orderNumber, out Order order))
            {
                UserWarning?.Invoke("Заказ с указанным номером не найден в базе");
                return false;
            }

            if (order.state != OrderState.Formation)
            {
                UserWarning?.Invoke("Состав заказа можно менять только во время формирования заказа");
                return false;
            }

            if (!items.TryGetValue(itemArticle, out Item item))
            {
                UserWarning?.Invoke("В базе нет товара с данным артикулом");
                return false;
            }

            order.SetItemQuantity(item, newQuantity);
            CheckCustomerDiscounts(id);
            StateChanged?.Invoke();
            return true;
        }

        public bool TryEditAddress(string id, uint orderNumber, string address)
        {
            if (!customers.TryGetValue(id, out Customer customer))
            {
                UserWarning?.Invoke("Клиент с указанным id не найден в базе");
                return false;
            }

            if (!customer.OrderManager.TryGetOrder(orderNumber, out Order order))
            {
                UserWarning?.Invoke("Заказ с указанным номером не найден в базе");
                return false;
            }

            if (order.state > OrderState.Processing)
            {
                UserWarning?.Invoke("Адрес доставки можно менять только до передачи службе доставки");
                return false;
            }

            order.Address = address;
            StateChanged?.Invoke();
            return true;
        }

        public bool TryEditDeliveryType(string id, uint orderNumber, DeliveryType newType)
        {
            if (!customers.TryGetValue(id, out Customer customer))
            {
                UserWarning?.Invoke("Клиент с указанным id не найден в базе");
                return false;
            }

            if (!customer.OrderManager.TryGetOrder(orderNumber, out Order order))
            {
                UserWarning?.Invoke("Заказ с указанным номером не найден в базе");
                return false;
            }

            if (order.state != OrderState.Formation)
            {
                UserWarning?.Invoke("Тип доставки можно менять только во время формирования заказа");
                return false;
            }

            order.DeliveryType = newType;
            CheckCustomerDiscounts(id);
            StateChanged?.Invoke();
            return true;
        }

        public bool TryEditCreationDate(string id, uint orderNumber, DateTimeOffset newDate)
        {
            if (!customers.TryGetValue(id, out Customer customer))
            {
                UserWarning?.Invoke("Клиент с указанным id не найден в базе");
                return false;
            }

            if (!customer.OrderManager.TryGetOrder(orderNumber, out Order order))
            {
                UserWarning?.Invoke("Заказ с указанным номером не найден в базе");
                return false;
            }

            if (order.state != OrderState.Formation)
            {
                UserWarning?.Invoke("Дату создания заказа можно менять только на этапе формирования");
                return false;
            }

            order.CreationDate = newDate;
            CheckCustomerDiscounts(id);
            StateChanged?.Invoke();
            return true;
        }

        public bool TryPushNextState(string id, uint orderNumber)
        {
            if (!customers.TryGetValue(id, out Customer customer))
            {
                UserWarning?.Invoke("Клиент с указанным id не найден в базе");
                return false;
            }

            if (!customer.OrderManager.TryGetOrder(orderNumber, out Order order))
            {
                UserWarning?.Invoke("Заказ с указанным номером не найден в базе");
                return false;
            }

            if (order.state >= OrderState.Completed)
            {
                UserWarning?.Invoke("Заказ уже находится в завершенном состоянии");
                return false;
            }

            if (order.OrderLinesAmount == 0)
            {
                UserWarning?.Invoke("Заказ не должен быть пустым");
                return false;
            }
            order.NextOrderState();
            StateChanged?.Invoke();
            return true;
        }

        public bool TryCancelOrder(string id, uint orderNumber)
        {
            if (!customers.TryGetValue(id, out Customer customer))
            {
                UserWarning?.Invoke("Клиент с указанным id не найден в базе");
                return false;
            }

            if (!customer.OrderManager.TryGetOrder(orderNumber, out Order order))
            {
                UserWarning?.Invoke("Заказ с указанным номером не найден в базе");
                return false;
            }

            if (order.state == OrderState.Formation)
            {
                UserWarning?.Invoke("Нельзя отменить неподтвержденный заказ");
                return false;
            }
            if (order.state >= OrderState.Completed)
            {
                UserWarning?.Invoke("Заказ уже завершен");
                return false;
            }
            order.CancelOrder();
            StateChanged?.Invoke();
            return true;
        }

        // Discount

        public List<Discount> GetDiscounts()
        {
            return discounts.Values.ToList();
        }

        public bool TryAddDiscount(string id, uint orderNumber, string discountName)
        {
            if (!customers.TryGetValue(id, out Customer customer))
            {
                UserWarning?.Invoke("Клиент с указанным id не найден в базе");
                return false;
            }

            if (!customer.OrderManager.TryGetOrder(orderNumber, out Order order))
            {
                UserWarning?.Invoke("Заказ с указанным номером не найден в базе");
                return false;
            }

            if (order.state != OrderState.Formation)
            {
                UserWarning?.Invoke("Скидки должны быть применены на стадии формирования заказа");
                return false;
            }

            if (!discounts.TryGetValue(discountName, out Discount discount))
            {
                UserWarning?.Invoke("В базе не найдено скидки с подобным именем");
                return false;
            }

            var check = discount.Check(customer, order);
            if (!check.Item1)
            {
                UserWarning?.Invoke(check.Item2);
                return false;
            }

            if (order.discounts.TryGetValue(discount.Family, out Discount discountToRemove))
            {
                DiscountDenied(orderNumber, discountToRemove.Name, string.Format("Скидка замещена другой скидкой ({0})", discount.Name));
                order.discounts.Remove(discount.Family);
            }
            order.discounts.Add(discount.Family, discount);
            StateChanged?.Invoke();
            return true;
        }

        public bool TryRemoveDiscount(string id, uint orderNumber, string discountName)
        {
            if (!customers.TryGetValue(id, out Customer customer))
            {
                UserWarning?.Invoke("Клиент с указанным id не найден в базе");
                return false;
            }

            if (!customer.OrderManager.TryGetOrder(orderNumber, out Order order))
            {
                UserWarning?.Invoke("Заказ с указанным номером не найден в базе");
                return false;
            }

            if (order.state != OrderState.Formation)
            {
                UserWarning?.Invoke("Скидки должны быть применены на стадии формирования заказа");
                return false;
            }

            if (!discounts.TryGetValue(discountName, out Discount discount))
            {
                UserWarning?.Invoke("В базе не найдено скидки с подобным именем");
                return false;
            }

            if (!order.discounts.Remove(discount.Family))
            {
                UserWarning?.Invoke("В заказе не найдено скидки с подобным именем");
                return false;
            }
            StateChanged?.Invoke();
            return true;


        }
    }
}
