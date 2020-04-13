using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Program.Promotion;

namespace Program
{
    public static class Prompt
    {
        public static int ShowDialog(string text, string caption)
        {
            Form prompt = new Form();
            prompt.Width = 300;
            prompt.Height = 150;
            prompt.Text = caption;
            prompt.StartPosition = FormStartPosition.CenterParent;
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            NumericUpDown inputBox = new NumericUpDown() { Left = 50, Top = 50, Width = 200 };
            Button confirmation = new Button() { Text = "Ok", Left = 100, Width = 100, Top = 80 };
            inputBox.Minimum = 1;
            inputBox.Maximum = 100000;
            confirmation.Click += (sender, e) =>
            {
                prompt.Close();
            };
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(inputBox);
            prompt.ShowDialog();
            return (int)inputBox.Value;
        }
    }

    class MainForm : Form
    {
        Random random = new Random();
        DataBase db = new DataBase();
        FileLoader fileLoader = new FileLoader();
        TreeViewGenerator treeGenerator = new TreeViewGenerator();
        TreeView customerTree;
        TreeView itemTree;
        TreeView orderTree;
        TreeView discountTree;

        Label customersLabel;
        Label itemsLabel;
        Label orderLabel;
        Label discountLabel;
        Button addOrderButton;
        ContextMenu userContextMenu;
        ContextMenu itemContextMenu;
        ContextMenu orderContextMenu;
        ContextMenu orderLineContextMenu;
        ContextMenu discountContextMenu;

        TableLayoutPanel orderHeaderTable;
        TableLayoutPanel mainTable;
        TableLayoutPanel middleTable;

        void LoadCustomersFromFile()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = @"C:\Users\Elizabethh\YandexDisk\Documents\Bauman\Programming\Season4\hw1";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var customers = fileLoader.LoadCustomersFromFile(openFileDialog.FileName);
                    foreach (var customer in customers)
                    {
                        db.AddCustomer(customer);
                    }
                    MessageBox.Show("Было загружено " + customers.Count + " профилей клиентов.", "Загрузка клиентов из файла", MessageBoxButtons.OK);
                }
            }

        }

        void LoadCustomersFromFile(string fileName)
        {
            var customers = fileLoader.LoadCustomersFromFile(fileName);
            foreach (var customer in customers)
            {
                db.AddCustomer(customer);
            }
        }

        void LoadItemsFromFile()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = @"C:\Users\Elizabethh\YandexDisk\Documents\Bauman\Programming\Season4\hw1";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var items = fileLoader.LoadItemsFromFile(openFileDialog.FileName);
                    foreach (var item in items)
                    {
                        db.AddItem(item);
                    }
                    MessageBox.Show("Было загружено " + items.Count + " товаров.", "Загрузка товаров из файла", MessageBoxButtons.OK);
                }
            }


        }

        void LoadItemsFromFile(string fileName)
        {
            var items = fileLoader.LoadItemsFromFile(fileName);
            foreach (var item in items)
            {
                db.AddItem(item);
            }
        }

        void UpdateView()
        {
            if (!db.IsAvailable)
            {
                MessageBox.Show(
                    "База данных не доступна (или не создана)",
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            treeGenerator.GenerateItemTree(itemTree, db.GetItems(), itemContextMenu);
            treeGenerator.GenerateCustomerTree(customerTree, db.GetCustomers(), userContextMenu);
            treeGenerator.GenerateDiscountTree(discountTree, db.GetDiscounts());

            // Workaround to avoid strange visual bug after TreeView editing
            customerTree.LabelEdit = false;
            itemTree.LabelEdit = false;
            orderTree.LabelEdit = false;

            if (orderTree.TopNode != null)
            {
                var args = orderTree.TopNode.Tag as OrderArgs;
                orderTree.Nodes.Clear();
                LoadCustomerOrders(db.GetCustomer(args.customer.ID));
                orderTree.ExpandAll();
            }
            customerTree.ExpandAll();
        }

        void ShowDiscountWarning(int orderNumber, string discountName, string reason)
        {
            MessageBox.Show(string.Format("{0} была удалена из заказа '{1}'.\nПричина: {2}", discountName, orderNumber, reason),
                "Внимание");
        }

        void LoadCustomerOrders(Customer customer)
        {
            treeGenerator.GenerateOrderTree(orderTree, customer, orderContextMenu, orderLineContextMenu, discountContextMenu);
        }

        void InitializeDB(bool isExplicit)
        {
            db.Initialize();
            if (isExplicit)
                MessageBox.Show("База данных была создана.", "", MessageBoxButtons.OK);
        }

        void ResetDB()
        {
            db.Reset();
            MessageBox.Show("База данных была удалена.", "", MessageBoxButtons.OK);
        }

        void CreateOrder(Customer customer)
        {
            Order testOrder = new Order(random.Next(1000000000, 2000000000), "Please, enter address", DeliveryType.Standard);
            customer.OrderManager.AddOrder(testOrder);
            db.EditCustomer(customer);
        }

        private void tree_BeginDrag(object sender, ItemDragEventArgs e)
        {
            var node = e.Item as TreeNode;
            if (node.Name == "item" || node.Name == "discount")
            {
                DoDragDrop(e.Item, DragDropEffects.Copy);
            }
        }

        private void tree_DragOver(object sender, DragEventArgs e)
        {
            TreeView tree = (TreeView)sender;

            e.Effect = DragDropEffects.None;

            TreeNode nodeSource = (TreeNode)e.Data.GetData(typeof(TreeNode));
            if (nodeSource != null)
            {

                Point pt = new Point(e.X, e.Y);
                pt = tree.PointToClient(pt);
                TreeNode nodeTarget = tree.GetNodeAt(pt);
                if (nodeTarget == null) return;

                if (nodeTarget.Name.Equals("items"))
                {
                    e.Effect = DragDropEffects.Copy;
                    tree.SelectedNode = nodeTarget;
                }

                if (nodeTarget.Name.Equals("discounts"))
                {
                    e.Effect = DragDropEffects.Copy;
                    tree.SelectedNode = nodeTarget;
                }

            }
        }

        private void orderTree_EndItemDrag(object sender, DragEventArgs e)
        {
            TreeView tree = (TreeView)sender;
            Point pt = new Point(e.X, e.Y);
            pt = tree.PointToClient(pt);

            TreeNode nodeTarget = tree.GetNodeAt(pt);
            TreeNode nodeSource = (TreeNode)e.Data.GetData(typeof(TreeNode));

            if (nodeSource.Name == "item" && nodeTarget.Name == "items")
            {
                uint quantity = (uint)Prompt.ShowDialog("Enter quantity", "Enter quantity");
                var newLine = new OrderLine((nodeSource.Tag as Item).Clone() as Item, quantity);

                var orderArgs = nodeTarget.Tag as OrderArgs;
                orderArgs.order.AddOrderLine(newLine);
                if (!db.EditOrder(orderArgs.customer.ID, orderArgs.order))
                    MessageBox.Show("Клиент не найден в базе данных", "Ошибка");
            }
            if (nodeSource.Name == "discount" && nodeTarget.Name == "discounts")
            {
                var targetArgs = nodeTarget.Tag as OrderArgs;
                var sourceArgs = nodeSource.Tag as OrderArgs;
                var result = db.AddDiscount(targetArgs.customer.ID, targetArgs.order.Number, sourceArgs.discount);
                if (!result.Item1)
                    ShowDiscountWarning(targetArgs.order.Number, sourceArgs.discount.Name, result.Item2);
            }
        }


        private void tree_AfterDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var tree = sender as TreeView;
            var node = e.Node;
            if (node != null && node.Tag != null)
            {
                tree.SelectedNode = node;
                tree.LabelEdit = true;
                if (!node.IsEditing)
                {
                    node.BeginEdit();
                }
            }
            else
            {
                MessageBox.Show("Выбранную запись нельзя редактировать", "Invalid selection");
            }
        }


        private void customerTree_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label != null)
            {

                switch (e.Node.Name)
                {
                    case "name":
                        customerTree_AfterNameEdit(sender, e);
                        break;
                    case "phone":
                        customerTree_AfterPhoneEdit(sender, e);
                        break;
                    case "privilege":
                        customerTree_AfterPrivilegeEdit(sender, e);
                        break;

                    default:
                        MessageBox.Show("Выбранный пункт нельзя редактировать.\n",
                           "Редактирование профиля клиента");
                        e.CancelEdit = true;
                        e.Node.EndEdit(true);
                        //UpdateView();
                        break;
                }

            }
        }

        private void customerTree_AfterPhoneEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label.IndexOfAny(new char[] { '@', '.', ',', '!' }) == -1)
            {
                // Stop editing without canceling the label change.
                e.Node.EndEdit(false);
                var customer = e.Node.Tag as Customer;
                customer.ContactPhone = e.Label;
                if (db.EditCustomer(customer))
                    MessageBox.Show("Телефон успешно изменен.\n", "Операция успешна");
                else
                    MessageBox.Show("Клиент не найден в базе данных.\n", "Ошибка");
            }
            else
            {
                /* Cancel the label edit action, inform the user, and 
                   place the node in edit mode again. */
                e.CancelEdit = true;
                e.Node.EndEdit(true);
                MessageBox.Show("Неверный формат телефона.\n" +
                   "Запрещены символы: '@','.', ',', '!'",
                   "Ошибка");
            }
        }

        private void customerTree_AfterPrivilegeEdit(object sender, NodeLabelEditEventArgs e)
        {
            Privilege newStatus;

            if (e.Label.IndexOfAny(new char[] { '@', '.', ',', '!' }) == -1
                && Enum.TryParse(e.Label, out newStatus))
            {
                e.Node.EndEdit(false);
                var customer = e.Node.Tag as Customer;
                customer.Privilege = newStatus;
                if (db.EditCustomer(customer))
                    MessageBox.Show("Статус успешно изменен.\n", "Операция успешна");
                else
                    MessageBox.Show("Клиент не найден в базе данных.\n", "Ошибка");
            }
            else
            {
                /* Cancel the label edit action, inform the user, and 
                   place the node in edit mode again. */
                e.CancelEdit = true;
                e.Node.EndEdit(true);
                MessageBox.Show("Неверный статус клиента.\n" +
                   "Возможные статусы: 'Common', 'Premium'",
                   "Ошибка");
            }
        }

        private void customerTree_AfterNameEdit(object sender, NodeLabelEditEventArgs e)
        {
            var name = e.Label.Split(' ');
            if (e.Label.IndexOfAny(new char[] { '@', '.', ',', '!' }) == -1 && name.Length == 3)
            {
                // Stop editing without canceling the label change.
                e.Node.EndEdit(false);
                var customer = e.Node.Tag as Customer;
                customer.Name = new FullName(name);
                if (db.EditCustomer(customer))
                {

                    MessageBox.Show("ФИО успешно изменено.\n", "Операция успешна");

                }
                else
                    MessageBox.Show("Клиент не найден в базе данных.\n", "Ошибка");
            }
            else
            {
                /* Cancel the label edit action, inform the user, and 
                   place the node in edit mode again. */
                e.CancelEdit = true;
                e.Node.EndEdit(true);
                MessageBox.Show("Неверный формат ФИО.\n" +
                   "Запрещены символы: '@','.', ',', '!'",
                   "Ошибка");
            }
        }


        private void orderTree_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label != null)
            {

                switch (e.Node.Name)
                {
                    case "address":
                        orderTree_AfterAddressEdit(sender, e);
                        break;
                    case "date":
                        orderTree_AfterDateEdit(sender, e);
                        break;
                    case "deliveryType":
                        orderTree_AfterDeliveryTypeEdit(sender, e);
                        break;
                    case "quantity":
                        orderTree_AfterItemQuantityEdit(sender, e);
                        break;

                    default:
                        MessageBox.Show("Выбранный пункт нельзя редактировать.\n",
                           "Редактирование заказов клиента");
                        e.CancelEdit = true;
                        e.Node.EndEdit(true);
                        break;
                }

            }
        }

        private void orderTree_AfterAddressEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label.IndexOfAny(new char[] { '@', '!', '?' }) == -1)
            {
                // Stop editing without canceling the label change.
                e.Node.EndEdit(false);
                var args = e.Node.Tag as OrderArgs;
                args.order.Address = e.Label;

                if (db.EditOrder(args.customer.ID, args.order))
                {
                    MessageBox.Show("Адрес успешно изменен.\n", "Операция успешна");
                }
                else
                    MessageBox.Show("Клиент не найден в базе данных.\n", "Ошибка");
            }
            else
            {
                e.CancelEdit = true;
                e.Node.EndEdit(true);
                MessageBox.Show("Неверный формат адреса.\n" +
                   "Запрещены символы: '@', '!', '?'",
                   "Ошибка");
            }
        }

        private void orderTree_AfterDateEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label.IndexOfAny(new char[] { '@', '!', '?' }) == -1
                && DateTimeOffset.TryParse(e.Label, out DateTimeOffset newDate))
            {
                e.Node.EndEdit(false);
                var args = e.Node.Tag as OrderArgs;
                args.order.CreationDate = newDate;

                if (db.EditOrder(args.customer.ID, args.order))
                {
                    MessageBox.Show("Дата формирования заказа успешно изменена.\n", "Операция успешна");
                }
                else
                    MessageBox.Show("Клиент не найден в базе данных.\n", "Ошибка");
            }
            else
            {
                e.CancelEdit = true;
                e.Node.EndEdit(true);
                MessageBox.Show("Неверный формат даты.\n" +
                   "Требуемый формат: \"dd.mm.yyyy\"",
                   "Ошибка");
            }
        }

        private void orderTree_AfterDeliveryTypeEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label.IndexOfAny(new char[] { '@', '.', ',', '!' }) == -1
                && Enum.TryParse(e.Label, out DeliveryType newType))
            {
                e.Node.EndEdit(false);
                var args = e.Node.Tag as OrderArgs;
                args.order.DeliveryType = newType;
                if (db.EditOrder(args.customer.ID, args.order))
                    MessageBox.Show("Статус успешно изменен.\n", "Операция успешна");
                else
                    MessageBox.Show("Клиент не найден в базе данных.\n", "Ошибка");
            }
            else
            {
                /* Cancel the label edit action, inform the user, and 
                   place the node in edit mode again. */
                e.CancelEdit = true;
                e.Node.EndEdit(true);
                MessageBox.Show("Неверный статус доставки.\n" +
                   "Возможные статусы: 'Standard', 'Express'",
                   "Ошибка");
            }
        }

        private void orderTree_AfterItemQuantityEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label.IndexOfAny(new char[] { '@', '.', ',', '!' }) == -1
                && uint.TryParse(e.Label, out uint newAmount))
            {
                e.Node.EndEdit(false);
                var args = e.Node.Tag as OrderArgs;
                args.order.SetItemQuantity(args.orderLine.Item, newAmount);
                if (db.EditOrder(args.customer.ID, args.order))
                    MessageBox.Show("Количество успешно изменено.\n", "Операция успешна");
                else
                    MessageBox.Show("Клиент не найден в базе данных.\n", "Ошибка");
            }
            else
            {
                /* Cancel the label edit action, inform the user, and 
                   place the node in edit mode again. */
                e.CancelEdit = true;
                e.Node.EndEdit(true);
                MessageBox.Show("Вы ввели не целое число.\n", "Ошибка");
            }
        }


        private void DrawCustomerNode(object sender, DrawTreeNodeEventArgs e)
        {
            var tree = sender as TreeView;
            var font = e.Node.NodeFont;
            if (font == null)
                font = new Font("Arial", 10);

            //TextRenderer.DrawText()
            TextRenderer.DrawText(e.Graphics,
                e.Node.Text,
                font,
                new Point(e.Node.Bounds.Left, e.Node.Bounds.Top),
                tree.SelectedNode == e.Node && tree.Focused ? SystemColors.HighlightText : SystemColors.WindowText,
                tree.SelectedNode == e.Node && tree.Focused ? SystemColors.Highlight : SystemColors.Window);
        }

        private void CreateMenu()
        {
            #region MENU
            // MENU
            MenuItem loadCustomersMenuItem = new MenuItem("Загрузить список клиентов");
            MenuItem loadItemsMenuItem = new MenuItem("Загрузить список товаров");
            MenuItem initializeDBItem = new MenuItem("Пересоздать базу данных");
            MenuItem resetDBItem = new MenuItem("Удалить базу данных");
            MenuItem updateViewItem = new MenuItem("Обновить списки");
            MenuItem exitProgramItem = new MenuItem("Выход");

            loadCustomersMenuItem.Click += (sender, args) => { LoadCustomersFromFile(); UpdateView(); };
            initializeDBItem.Click += (sender, args) => InitializeDB(true);
            loadItemsMenuItem.Click += (sender, args) => { LoadItemsFromFile(); UpdateView(); };
            updateViewItem.Click += (sender, args) => UpdateView();
            resetDBItem.Click += (sender, args) => ResetDB();
            exitProgramItem.Click += (sender, args) => Application.Exit();

            var mainMenu = new MainMenu(new[] {
                new MenuItem("Файл", new[] {
                    initializeDBItem,
                    loadCustomersMenuItem,
                    loadItemsMenuItem,
                    resetDBItem,
                    exitProgramItem
                }),
                new MenuItem("Вид", new[]
                {
                    updateViewItem
                })
            });
            Menu = mainMenu;
            Width = 1200;
            Height = 800;
            CenterToScreen();
            #endregion
        }

        private void CreateContextMenu()
        {
            orderContextMenu = new ContextMenu();
            var deleteOrderItem = new MenuItem("Удалить заказ");
            deleteOrderItem.Click += (sender, args) =>
            {
                if (orderTree.SelectedNode != null)
                    if (orderTree.SelectedNode.Name == "number")
                    {
                        var orderArgs = orderTree.SelectedNode.Tag as OrderArgs;
                        orderArgs.customer.OrderManager.Remove(orderArgs.order.Number);
                        db.EditOrder(orderArgs.customer.ID, orderArgs.order);
                    }
            };
            orderContextMenu.MenuItems.Add(deleteOrderItem);

            orderLineContextMenu = new ContextMenu();
            var deleteOrderLineItem = new MenuItem("Удалить товар из заказа");
            deleteOrderLineItem.Click += (sender, args) =>
            {
                if (orderTree.SelectedNode != null)
                    if (orderTree.SelectedNode.Name == "item")
                    {
                        var orderArgs = orderTree.SelectedNode.Tag as OrderArgs;
                        orderArgs.order.DeleteItem(orderArgs.orderLine.Item);
                        db.EditOrder(orderArgs.customer.ID, orderArgs.order);
                    }
            };
            orderLineContextMenu.MenuItems.Add(deleteOrderLineItem);

            itemContextMenu = new ContextMenu();
            var deleteItemItem = new MenuItem("Удалить товар из каталога");
            deleteItemItem.Click += (sender, args) =>
            {
                if (itemTree.SelectedNode != null)
                    if (itemTree.SelectedNode.Name == "item")
                    {
                        var item = itemTree.SelectedNode.Tag as Item;
                        db.DeleteItem(item);
                    }
            };
            itemContextMenu.MenuItems.Add(deleteItemItem);

            userContextMenu = new ContextMenu();
            var editOrdersItem = new MenuItem("Редактировать заказы пользователя");
            editOrdersItem.Click += (sender, args) =>
            {
                if (customerTree.SelectedNode != null)
                    if (customerTree.SelectedNode.Name == "name")
                    {
                        var customer = customerTree.SelectedNode.Tag as Customer;
                        LoadCustomerOrders(db.GetCustomer(customer.ID));
                    }
            };
            userContextMenu.MenuItems.Add(editOrdersItem);

            discountContextMenu = new ContextMenu();
            var deleteDiscountItem = new MenuItem("Удалить скидку");
            deleteDiscountItem.Click += (sender, args) =>
            {
                if (orderTree.SelectedNode != null)
                    if (orderTree.SelectedNode.Name == "discount")
                    {
                        var orderArgs = orderTree.SelectedNode.Tag as OrderArgs;
                        orderArgs.order.discounts.Remove(orderArgs.discount.Family);
                        db.EditOrder(orderArgs.customer.ID, orderArgs.order);
                    }
            };
            discountContextMenu.MenuItems.Add(deleteDiscountItem);
        }

        private void CreateWindowControls()
        {
            customersLabel = new Label()
            {
                Text = "Покупатели",
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Fill
            };

            discountLabel = new Label()
            {
                Text = "Скидки",
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Fill
            };

            itemsLabel = new Label()
            {
                Text = "Товары",
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Fill
            };

            orderLabel = new Label()
            {
                Text = "Заказы",
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Fill
            };

            addOrderButton = new Button()
            {
                Text = "+",
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Width = 40,
                Dock = DockStyle.Left
            };

            addOrderButton.Click += (sender, args) =>
            {
                if (orderTree.Nodes.Count != 0)
                {
                    var orderArgs = orderTree.Nodes[0].Tag as OrderArgs;
                    CreateOrder(orderArgs.customer);
                    LoadCustomerOrders(db.GetCustomer(orderArgs.customer.ID));
                }
                else MessageBox.Show(
                    "Сначала выберите клиента в левой части окна,\nщелкнув по нему правой кнопкой мыши и выбрав\n\"Редактировать заказы\"",
                    "Сообщение");
            };

        }

        private void CreateTrees()
        {
            customerTree = new BufferedTreeView()
            {
                Dock = DockStyle.Fill,
                ShowNodeToolTips = true,
                CausesValidation = false
            };

            itemTree = new BufferedTreeView()
            {
                Dock = DockStyle.Fill,
                ShowNodeToolTips = true
            };

            orderTree = new BufferedTreeView()
            {
                Dock = DockStyle.Fill,
                ShowNodeToolTips = true,
                AllowDrop = true
            };

            discountTree = new BufferedTreeView()
            {
                Dock = DockStyle.Fill,
                ShowNodeToolTips = true
            };

            customerTree.NodeMouseDoubleClick += (sender, args) => tree_AfterDoubleClick(sender, args);
            customerTree.AfterLabelEdit += (sender, args) => customerTree_AfterLabelEdit(sender, args);
            customerTree.NodeMouseClick += (sender, args) => customerTree.SelectedNode = args.Node;

            itemTree.ItemDrag += (sender, args) => tree_BeginDrag(sender, args);
            //itemsTree.DragOver += (sender, args) => tree_DragOver(sender, args);
            //itemsTree.DragDrop += (sender, args) => tree_EndDrag(sender, args);
            //itemsTree.NodeMouseDoubleClick += (sender, args) => tree_AfterDoubleClick(sender, args);
            itemTree.NodeMouseClick += (sender, args) => itemTree.SelectedNode = args.Node;

            //ordersTree.ItemDrag += (sender, args) => tree_BeginDrag(sender, args);
            orderTree.DragOver += (sender, args) => tree_DragOver(sender, args);
            orderTree.DragDrop += (sender, args) => orderTree_EndItemDrag(sender, args);
            orderTree.NodeMouseClick += (sender, args) => orderTree.SelectedNode = args.Node;
            orderTree.NodeMouseDoubleClick += (sender, args) => tree_AfterDoubleClick(sender, args);
            orderTree.AfterLabelEdit += (sender, args) => orderTree_AfterLabelEdit(sender, args);

            discountTree.ItemDrag += (sender, args) => tree_BeginDrag(sender, args);
            discountTree.NodeMouseClick += (sender, args) => discountTree.SelectedNode = args.Node;

            customerTree.DrawMode = TreeViewDrawMode.OwnerDrawText;
            itemTree.DrawMode = TreeViewDrawMode.OwnerDrawText;
            orderTree.DrawMode = TreeViewDrawMode.OwnerDrawText;
            discountTree.DrawMode = TreeViewDrawMode.OwnerDrawText;
            customerTree.DrawNode += (sender, args) => DrawCustomerNode(sender, args);
            itemTree.DrawNode += (sender, args) => DrawCustomerNode(sender, args);
            orderTree.DrawNode += (sender, args) => DrawCustomerNode(sender, args);
            discountTree.DrawNode += (sender, args) => DrawCustomerNode(sender, args);
        }

        private void CreateLayout()
        {
            //Супер кривое решение, но переделывать просто сил уже нет
            middleTable = new TableLayoutPanel();
            middleTable.Dock = DockStyle.Fill;
            middleTable.RowStyles.Clear();
            middleTable.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            middleTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            middleTable.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            middleTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            middleTable.Controls.Add(itemTree, 0, 0);
            middleTable.Controls.Add(discountLabel, 0, 1);
            middleTable.Controls.Add(discountTree, 0, 2);

            orderHeaderTable = new TableLayoutPanel();
            orderHeaderTable.Dock = DockStyle.Fill;
            orderHeaderTable.RowStyles.Clear();
            orderHeaderTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            orderHeaderTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            orderHeaderTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
            orderHeaderTable.Controls.Add(orderLabel, 0, 0);
            orderHeaderTable.Controls.Add(addOrderButton, 1, 0);

            mainTable = new TableLayoutPanel();
            mainTable.RowStyles.Clear();
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));

            mainTable.Controls.Add(customersLabel, 0, 0);
            mainTable.Controls.Add(itemsLabel, 1, 0);
            mainTable.Controls.Add(orderHeaderTable, 2, 0);
            mainTable.Controls.Add(customerTree, 0, 1);
            mainTable.Controls.Add(middleTable, 1, 1);
            mainTable.Controls.Add(orderTree, 2, 1);

            mainTable.Dock = DockStyle.Fill;
            mainTable.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;

            Controls.Add(mainTable);

        }

        public MainForm()
        {
            var obj = new object();
            CreateMenu();
            CreateContextMenu();
            CreateWindowControls();
            CreateTrees();
            CreateLayout();

            InitializeDB(false);
            LoadCustomersFromFile("CUSTOMERS.DAT");
            LoadItemsFromFile("ITEMS.DAT");
            UpdateView();

            db.StateChanged += UpdateView;
            db.DiscountDenied += ShowDiscountWarning;
        }


    }
}
