using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Program.Promotion;

namespace Program
{
    public class OrderArgs : ICloneable
    {
        public string id;
        public uint orderNumber;
        public string itemArticle;
        public string discountName;

        public OrderArgs() { }
        public OrderArgs(Order order) { orderNumber = order.Number; }
        public OrderArgs(Order order, OrderLine line)
        {
            orderNumber = order.Number;
            itemArticle = line.Item.Article;
        }
        public OrderArgs(Customer customer) { id = customer.ID; }
        public OrderArgs(Customer customer, Order order) { id = customer.ID; orderNumber = order.Number; }
        public OrderArgs(Customer customer, Order order, OrderLine line)
        {
            id = customer.ID;
            orderNumber = order.Number;
            itemArticle = line.Item.Article;
        }
        public OrderArgs(Customer customer, Order order, Discount discount)
        {
            id = customer.ID;
            orderNumber = order.Number;
            discountName = discount.Name;
        }
        public OrderArgs(Item item) { itemArticle = item.Article; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class TreeViewGenerator
    {
        public void GenerateCustomerTree(TreeView customerTree, List<Customer> customers,
            ContextMenu menu)
        {
            customerTree.BeginUpdate();
            customerTree.Nodes.Clear();

            foreach (var customer in customers)
            {
                var customerRoot = new TreeNode()
                {
                    Name = "name",
                    Text = customer.Name.ToString(),
                    ToolTipText = "ФИО клиента",
                    Tag = new OrderArgs(customer)
                };

                customerRoot.ContextMenu = menu;

                customerRoot.Nodes.Add(new TreeNode()
                {
                    Name = "code",
                    Text = customer.ID,
                    ToolTipText = "Уникальный код клиента"
                });
                customerRoot.Nodes.Add(new TreeNode()
                {
                    Name = "phone",
                    Text = customer.ContactPhone,
                    ToolTipText = "Номер телефона",
                    Tag = new OrderArgs(customer)
                });
                customerRoot.Nodes.Add(new TreeNode()
                {
                    Name = "privilege",
                    Text = customer.Privilege.ToString(),
                    ToolTipText = "Статус аккаунта",
                    Tag = new OrderArgs(customer)
                });

                customerTree.Nodes.Add(customerRoot);
            }
            customerTree.EndUpdate();
        }

        public void GenerateItemTree(TreeView itemsTree, List<Item> items,
            ContextMenu itemMenu)
        {
            itemsTree.BeginUpdate();
            itemsTree.Nodes.Clear();
            foreach (var item in items)
            {
                var itemRoot = new TreeNode(item.Name);
                itemRoot.ContextMenu = itemMenu;
                itemRoot.Name = "item";
                itemRoot.Nodes.Add("Article: " + item.Article);
                itemRoot.Nodes.Add("Price: " + item.UnitPrice.ToString("0.00"));
                itemRoot.Tag = new OrderArgs(item);
                itemsTree.Nodes.Add(itemRoot);
            }
            itemsTree.EndUpdate();
        }

        public TreeNode GenerateOrderLineNode(Customer customer, Order order, OrderLine orderLine)
        {
            var lineRoot = new TreeNode()
            {
                Name = "item",
                Text = orderLine.Item.Name,
                ToolTipText = "Название товара",
                Tag = new OrderArgs(customer, order, orderLine)
            };

            lineRoot.Nodes.Add(new TreeNode()
            {
                Name = "article",
                Text = orderLine.Item.Article,
                ToolTipText = "Артикул товара"
            });

            lineRoot.Nodes.Add(new TreeNode()
            {
                Name = "unitPrice",
                Text = orderLine.Item.UnitPrice.ToString("0.00"),
                ToolTipText = "Цена за единицу товара"
            });

            lineRoot.Nodes.Add(new TreeNode()
            {
                Name = "quantity",
                Text = orderLine.Quantity.ToString(),
                ToolTipText = "Заказанное количество товара",
                Tag = new OrderArgs(customer, order, orderLine)
            });

            lineRoot.Nodes.Add(new TreeNode()
            {
                Name = "orderLinePrice",
                Text = orderLine.Cost.ToString("0.00"),
                ToolTipText = "Цена строки заказа"
            });

            return lineRoot;
        }

        public TreeNode GenerateOrderNode(Customer customer, Order order, ContextMenu discountContextMenu)
        {
            var orderRoot = new TreeNode()
            {
                Name = "number",
                Text = order.Number.ToString(),
                ToolTipText = "Уникальный номер заказа",
                Tag = new OrderArgs(customer, order),
                NodeFont = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold)
            };

            orderRoot.Nodes.Add(new TreeNode()
            {
                Name = "state",
                Text = "Текущее состояние: "+order.state.ToString(),
                ToolTipText = "Состояние заказа"
            });
            orderRoot.Nodes.Add(new TreeNode()
            {
                Name = "address",
                Text = order.Address,
                ToolTipText = "Адрес заказчика",
                Tag = new OrderArgs(customer, order)
            });
            orderRoot.Nodes.Add(new TreeNode()
            {
                Name = "creationDate",
                Text = "Создан: "+order.CreationDate.ToString("dd.MM.yyyy"),
                ToolTipText = "Дата создания заказа",
                Tag = new OrderArgs(customer, order)
            });

            if(order.state >= OrderState.Processing)
            {
                orderRoot.Nodes.Add(new TreeNode()
                {
                    Name = "formationDate",
                    Text = "Подтвержден: " + order.FormationDate.ToString("dd.MM.yyyy"),
                    ToolTipText = "Дата подтверждения заказа",
                    Tag = new OrderArgs(customer, order)
                });
            }
            if (order.state >= OrderState.Delivery)
            {
                orderRoot.Nodes.Add(new TreeNode()
                {
                    Name = "transferToDeliveryDate",
                    Text = "Передан в службу доставки: " + order.TransferredToDeliveryDate.ToString("dd.MM.yyyy"),
                    ToolTipText = "Дата передачи товара службе доставки",
                    Tag = new OrderArgs(customer, order)
                });
            }
            if (order.state >= OrderState.Done)
            {
                orderRoot.Nodes.Add(new TreeNode()
                {
                    Name = "deliveredDate",
                    Text = "Доставлен: " + order.DeliveredDate.ToString("dd.MM.yyyy"),
                    ToolTipText = "Дата передачи товара клиенту",
                    Tag = new OrderArgs(customer, order),
                    NodeFont = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold)
                });
            }

            orderRoot.Nodes.Add(new TreeNode()
            {
                Name = "deliveryType",
                Text = order.DeliveryType.ToString(),
                ToolTipText = "Тип доставки",
                Tag = new OrderArgs(customer, order)
            });

            var discountRoot = new TreeNode()
            {
                Name = "discounts",
                Text = "Примененные скидки",
                ToolTipText = "Скидки, примененные к заказу",
                Tag = new OrderArgs(customer, order)
            };

            foreach (var discount in order.discounts.Values)
            {
                discountRoot.Nodes.Add(new TreeNode()
                {
                    Name = "discount",
                    Text = discount.Name + " : " + discount.GetDiscountAmount(order).ToString("0.00"),
                    ToolTipText = discount.Description,
                    Tag = new OrderArgs(customer, order, discount),
                    ContextMenu = discountContextMenu
                });
            }
            orderRoot.Nodes.Add(discountRoot);

            orderRoot.Nodes.Add(new TreeNode()
            {
                Name = "totalCost",
                Text = "Суммарная стоимость заказа: " + order.TotalCost.ToString("0.00"),
                ToolTipText = "Стоимость заказа"
            });

            orderRoot.Nodes.Add(new TreeNode()
            {
                Name = "totalDiscount",
                Text = "Суммарная скидка на заказ: " + order.TotalDiscount.ToString("0.00"),
                ToolTipText = "Суммарная скидка"
            });
            orderRoot.Nodes.Add(new TreeNode()
            {
                Name = "totalDiscount",
                Text = "Итоговая цена заказа: " + (order.TotalCost - order.TotalDiscount).ToString("0.00"),
                ToolTipText = "Итоговая стоимость заказа",
                NodeFont = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold)
            }); ;

            return orderRoot;
        }

        public void GenerateOrderTree(TreeView ordersTree, Customer customer,
            ContextMenu orderMenu, ContextMenu orderLineMenu, ContextMenu discountContextMenu)
        {
            ordersTree.BeginUpdate();
            ordersTree.Nodes.Clear();

            ordersTree.Nodes.Add(new TreeNode()
            {
                Name = "name",
                Text = customer.Name.ToString(),
                ToolTipText = "ФИО заказчика",
                Tag = new OrderArgs(customer)
            });

            ordersTree.Nodes.Add(new TreeNode()
            {
                Name = "code",
                Text = customer.ID,
                ToolTipText = "Уникальный номер заказчика",
                Tag = new OrderArgs(customer)
            });

            foreach (var order in customer.OrderManager.Orders)
            {
                var orderRoot = GenerateOrderNode(customer, order, discountContextMenu);
                orderRoot.ContextMenu = orderMenu;

                var orderLinesRoot = new TreeNode()
                {
                    Name = "items",
                    Text = "Заказанные товары",
                    ToolTipText = "Товары, заказанные пользователем",
                    Tag = new OrderArgs(customer, order)
                };

                foreach (var orderLine in order.OrderLines)
                {
                    var newLine = GenerateOrderLineNode(customer, order, orderLine);
                    newLine.ContextMenu = orderLineMenu;
                    newLine.Expand();
                    orderLinesRoot.Nodes.Add(newLine);
                }

                orderRoot.Nodes.Add(orderLinesRoot);
                orderRoot.Expand();
                orderLinesRoot.Expand();
                ordersTree.Nodes.Add(orderRoot);
            }
            ordersTree.EndUpdate();
        }

        public void GenerateDiscountTree(TreeView tree, List<Discount> discounts)
        {
            tree.BeginUpdate();
            tree.Nodes.Clear();
            foreach (var discount in discounts)
            {
                tree.Nodes.Add(new TreeNode()
                {
                    Name = "discount",
                    Text = discount.Name,
                    ToolTipText = discount.Description,
                    Tag = new OrderArgs() { discountName = discount.Name }
                });
            }
            tree.EndUpdate();
        }
    }
}
