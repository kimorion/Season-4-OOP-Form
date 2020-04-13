using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Program.Promotion;

namespace Program
{
    public class OrderArgs
    {
        public Customer customer;
        public Order order;
        public OrderLine orderLine;
        public Promotion.Discount discount;

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
        public OrderArgs(OrderArgs args)
        {
            this.customer = args.customer;
            this.order = args.order;
            this.orderLine = args.orderLine;
            this.discount = args.discount;
        }
        public OrderArgs() { }

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

        public void GenerateItemTree(TreeView itemsTree, List<Item> items,
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
                itemRoot.Nodes.Add("Price: " + item.UnitPrice.ToString("0.00"));
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
                Text = args.orderLine.Item.UnitPrice.ToString("0.00"),
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
                Text = args.orderLine.Cost.ToString("0.00"),
                ToolTipText = "Цена строки заказа"
            });

            return lineRoot;
        }

        public TreeNode GenerateOrderNode(OrderArgs args, ContextMenu discountContextMenu)
        {
            var orderRoot = new TreeNode()
            {
                Name = "number",
                Text = args.order.Number.ToString(),
                ToolTipText = "Уникальный номер заказа",
                Tag = args,
                NodeFont = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold)
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
                Text = args.order.CreationDate.ToString("dd.MM.yyyy"),
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

            var discountRoot = new TreeNode()
            {
                Name = "discounts",
                Text = "Примененные скидки",
                ToolTipText = "Скидки, примененные к заказу",
                Tag = args
            };

            foreach (var discount in args.order.discounts.Values)
            {
                discountRoot.Nodes.Add(new TreeNode()
                {
                    Name = "discount",
                    Text = discount.Name + " : " + discount.GetDiscountAmount(args.order).ToString("0.00"),
                    ToolTipText = discount.Description,
                    Tag = new OrderArgs(args) { discount = discount },
                    ContextMenu = discountContextMenu
                });
            }
            orderRoot.Nodes.Add(discountRoot);

            orderRoot.Nodes.Add(new TreeNode()
            {
                Name = "totalCost",
                Text = "Суммарная стоимость заказа: " + args.order.TotalCost.ToString("0.00"),
                ToolTipText = "Стоимость заказа"
            });

            orderRoot.Nodes.Add(new TreeNode()
            {
                Name = "totalDiscount",
                Text = "Суммарная скидка на заказ: " + args.order.TotalDiscount.ToString("0.00"),
                ToolTipText = "Суммарная скидка"
            });
            orderRoot.Nodes.Add(new TreeNode()
            {
                Name = "totalDiscount",
                Text = "Итоговая цена заказа: " + (args.order.TotalCost - args.order.TotalDiscount).ToString("0.00"),
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
                var orderRoot = GenerateOrderNode(new OrderArgs(customer, order, null), discountContextMenu);
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
                    Tag = new OrderArgs() { discount = discount }
                });
            }
            tree.EndUpdate();
        }
    }
}
