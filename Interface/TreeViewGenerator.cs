using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Program
{
    public class OrderArgs
    {
        public Customer customer;
        public Order order;
        public OrderLine orderLine;

        public OrderArgs(Customer customer, Order order, OrderLine line)
        {
            this.customer = customer;
            this.order = order;
            this.orderLine = line;
        }
        public OrderArgs(Customer customer)
        {
            this.customer = customer;
        }
        public OrderArgs(Order order)
        {
            this.order = order;
        }
        public OrderArgs(OrderLine line)
        {
            this.orderLine = line;
        }
        public OrderArgs() { }

    }

    public class TreeViewGenerator
    {
        public void GenerateCustomersTree(TreeView customerTree, List<Customer> customers,
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
                    Tag = customer
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
                    Tag = customer
                });
                customerRoot.Nodes.Add(new TreeNode()
                {
                    Name = "privilege",
                    Text = customer.Privilege.ToString(),
                    ToolTipText = "Статус аккаунта",
                    Tag = customer
                });

                customerTree.Nodes.Add(customerRoot);
            }
            customerTree.EndUpdate();
        }

        public void GenerateItemsTree(TreeView itemsTree, List<Item> items,
            ContextMenu itemMenu)
        {
            itemsTree.BeginUpdate();
            itemsTree.Nodes.Clear();
            foreach (var item in items)
            {
                var itemRoot = new TreeNode(item.Article);
                itemRoot.ContextMenu = itemMenu;
                itemRoot.Name = "item";
                itemRoot.Nodes.Add("Name: " + item.Name);
                itemRoot.Nodes.Add("Price: " + item.UnitPrice.ToString());
                itemRoot.Tag = item;
                itemsTree.Nodes.Add(itemRoot);
            }
            itemsTree.EndUpdate();
        }

        public TreeNode GenerateOrderLineNode(OrderArgs args)
        {
            var lineRoot = new TreeNode()
            {
                Name = "item",
                Text = args.orderLine.Item.Name,
                ToolTipText = "Название товара",
                Tag = args
            };

            lineRoot.Nodes.Add(new TreeNode()
            {
                Name = "article",
                Text = args.orderLine.Item.Article,
                ToolTipText = "Артикул товара",
                Tag = args
            });

            lineRoot.Nodes.Add(new TreeNode()
            {
                Name = "unitPrice",
                Text = args.orderLine.Item.UnitPrice.ToString(),
                ToolTipText = "Цена за единицу товара",
                Tag = args
            });

            lineRoot.Nodes.Add(new TreeNode()
            {
                Name = "quantity",
                Text = args.orderLine.Quantity.ToString(),
                ToolTipText = "Заказанное количество товара",
                Tag = args
            });

            lineRoot.Nodes.Add(new TreeNode()
            {
                Name = "orderLinePrice",
                Text = args.orderLine.Cost.ToString(),
                ToolTipText = "Цена строки заказа"
            });

            return lineRoot;
        }

        public TreeNode GenerateOrderNode(OrderArgs args)
        {
            var orderRoot = new TreeNode()
            {
                Name = "number",
                Text = args.order.Number.ToString(),
                ToolTipText = "Уникальный номер заказа",
                Tag = args
            };

            orderRoot.Nodes.Add(new TreeNode()
            {
                Name = "address",
                Text = args.order.Address,
                ToolTipText = "Адрес заказчика",
                Tag = args
            });
            orderRoot.Nodes.Add(new TreeNode()
            {
                Name = "date",
                Text = args.order.CreationDate.ToString(),
                ToolTipText = "Дата оформления заказа",
                Tag = args
            });
            orderRoot.Nodes.Add(new TreeNode()
            {
                Name = "deliveryType",
                Text = args.order.DeliveryType.ToString(),
                ToolTipText = "Тип доставки",
                Tag = args
            });

            return orderRoot;
        }

        public void GenerateOrdersTree(TreeView ordersTree, Customer customer,
            ContextMenu orderMenu, ContextMenu orderLineMenu)
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
                var orderRoot = GenerateOrderNode(new OrderArgs(customer, order, null));
                orderRoot.ContextMenu = orderMenu;

                var orderLinesRoot = new TreeNode()
                {
                    Name = "items",
                    Text = "Заказанные товары",
                    ToolTipText = "Товары, заказанные пользователем",
                    Tag = new OrderArgs(customer, order, null)
                };

                foreach (var orderLine in order.OrderLines)
                {
                    var newLine = GenerateOrderLineNode(new OrderArgs(customer, order, orderLine));
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
    }
}
