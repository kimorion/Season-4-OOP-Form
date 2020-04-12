using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

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
        TreeView itemsTree;
        TreeView ordersTree;

        ContextMenu userContextMenu;
        ContextMenu itemContextMenu;
        ContextMenu orderContextMenu;
        ContextMenu orderLineContextMenu;

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

            treeGenerator.GenerateItemsTree(itemsTree, db.GetItems(), itemContextMenu);
            treeGenerator.GenerateCustomersTree(customerTree, db.GetCustomers(), userContextMenu);

            ordersTree.Nodes.Clear();
            Invalidate();
            Refresh();
        }

        void LoadCustomerOrders(Customer customer)
        {
            treeGenerator.GenerateOrdersTree(ordersTree, customer, orderContextMenu, orderLineContextMenu);
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
            Order testOrder = new Order(random.Next(1000000000, 2000000000), "Please, enter address", DeliveryType.Stardard);
            customer.OrderManager.AddOrder(testOrder);
            db.EditCustomer(customer);
        }

        private void tree_BeginDrag(object sender, ItemDragEventArgs e)
        {
            var node = e.Item as TreeNode;
            if (node.Name == "item")
            {
                //tree.DoDragDrop(node, DragDropEffects.Copy);
                DoDragDrop(e.Item, DragDropEffects.Copy);
            }
        }

        private void tree_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
        {
            TreeView tree = (TreeView)sender;

            e.Effect = DragDropEffects.None;

            TreeNode nodeSource = (TreeNode)e.Data.GetData(typeof(TreeNode));
            if (nodeSource != null)
            {

                Point pt = new Point(e.X, e.Y);
                pt = tree.PointToClient(pt);
                TreeNode nodeTarget = tree.GetNodeAt(pt);
                if (nodeTarget != null && nodeTarget.Name.Equals("items"))
                {
                    e.Effect = DragDropEffects.Copy;
                    tree.SelectedNode = nodeTarget;
                }

            }
        }

        private void tree_EndDrag(object sender, System.Windows.Forms.DragEventArgs e)
        {
            TreeView tree = (TreeView)sender;
            Point pt = new Point(e.X, e.Y);
            pt = tree.PointToClient(pt);

            TreeNode nodeTarget = tree.GetNodeAt(pt);
            TreeNode nodeSource = (TreeNode)e.Data.GetData(typeof(TreeNode));

            uint quantity = (uint)Prompt.ShowDialog("Enter quantity", "Enter quantity");
            var newLine = new OrderLine((nodeSource.Tag as Item).Clone() as Item, quantity);

            var orderArgs = nodeTarget.Tag as OrderArgs;
            orderArgs.order.AddOrderLine(newLine);
            if (!db.EditOrder(orderArgs.customer.ID, orderArgs.order))
                MessageBox.Show("Ниче не работает", "ашибка");

            LoadCustomerOrders(orderArgs.customer);
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
                    Console.WriteLine("EDIT");
                }
            }
            else
            {
                MessageBox.Show("Выбранную запись нельзя редактировать", "Invalid selection");
            }
        }

        private void customerTree_AfterLabelEdit(object sender,
                 System.Windows.Forms.NodeLabelEditEventArgs e)
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
                        e.Node.EndEdit(true);
                        break;
                }

            }
        }

        private void customerTree_AfterPhoneEdit(object sender,
                 System.Windows.Forms.NodeLabelEditEventArgs e)
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

        private void customerTree_AfterPrivilegeEdit(object sender,
                 System.Windows.Forms.NodeLabelEditEventArgs e)
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

        private void customerTree_AfterNameEdit(object sender, System.Windows.Forms.NodeLabelEditEventArgs e)
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



        public MainForm()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
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

            // ContextMenu
            orderContextMenu = new ContextMenu();
            var deleteOrderItem = new MenuItem("Удалить заказ");
            deleteOrderItem.Click += (sender, args) =>
            {
                if (ordersTree.SelectedNode != null)
                    if (ordersTree.SelectedNode.Name == "number")
                    {
                        var orderArgs = ordersTree.SelectedNode.Tag as OrderArgs;
                        orderArgs.customer.OrderManager.Remove(orderArgs.order.Number);
                        LoadCustomerOrders(orderArgs.customer);
                    }
            };
            orderContextMenu.MenuItems.Add(deleteOrderItem);

            orderLineContextMenu = new ContextMenu();
            var deleteOrderLineItem = new MenuItem("Удалить товар из заказа");
            deleteOrderLineItem.Click += (sender, args) =>
            {
                if (ordersTree.SelectedNode != null)
                    if (ordersTree.SelectedNode.Name == "item")
                    {
                        var orderArgs = ordersTree.SelectedNode.Tag as OrderArgs;
                        orderArgs.order.DeleteItem(orderArgs.orderLine.Item);
                        LoadCustomerOrders(orderArgs.customer);
                    }
            };
            orderLineContextMenu.MenuItems.Add(deleteOrderLineItem);

            itemContextMenu = new ContextMenu();
            var deleteItemItem = new MenuItem("Удалить товар из каталога");
            deleteItemItem.Click += (sender, args) =>
            {
                if (itemsTree.SelectedNode != null)
                    if (itemsTree.SelectedNode.Name == "item")
                    {
                        var item = itemsTree.SelectedNode.Tag as Item;
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
                        LoadCustomerOrders(customerTree.SelectedNode.Tag as Customer);
            };
            userContextMenu.MenuItems.Add(editOrdersItem);

            // WINDOW CONTROLS
            var box1 = new TextBox
            {
                Dock = DockStyle.Fill,
                Text = "BOX1",
                Multiline = true
            };

            var customersLabel = new Label()
            {
                Text = "Покупатели",
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Fill
            };

            var itemsLabel = new Label()
            {
                Text = "Товары",
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Fill
            };

            var orderLabel = new Label()
            {
                Text = "Заказы",
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Dock = DockStyle.Fill
            };

            var addOrderButton = new Button()
            {
                Text = "+",
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 12, FontStyle.Bold),
                Width = 40,
                Dock = DockStyle.Left
            };

            addOrderButton.Click += (sender, args) =>
            {
                if (ordersTree.Nodes.Count != 0)
                {
                    var orderArgs = ordersTree.Nodes[0].Tag as OrderArgs;
                    CreateOrder(orderArgs.customer);
                    LoadCustomerOrders(orderArgs.customer);
                }
                else MessageBox.Show(
                    "Сначала выберите клиента в левой части окна,\nщелкнув по нему правой кнопкой мыши и выбрав\n\"Редактировать заказы\"",
                    "Сообщение");
            };

            var orderTable = new TableLayoutPanel();
            orderTable.Dock = DockStyle.Fill;
            orderTable.RowStyles.Clear();
            orderTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            orderTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            orderTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
            orderTable.Controls.Add(orderLabel, 0, 0);
            orderTable.Controls.Add(addOrderButton, 1, 0);

            customerTree = new TreeView()
            {
                Dock = DockStyle.Fill,
                LabelEdit = true,
                ShowNodeToolTips = true
            };

            itemsTree = new TreeView()
            {
                Dock = DockStyle.Fill,
                LabelEdit = true,
                ShowNodeToolTips = true,
                AllowDrop = true
            };

            ordersTree = new TreeView()
            {
                Dock = DockStyle.Fill,
                LabelEdit = false,
                ShowNodeToolTips = true,
                AllowDrop = true
            };

            customerTree.NodeMouseDoubleClick += (sender, args) => tree_AfterDoubleClick(sender, args);
            customerTree.AfterLabelEdit += (sender, args) => customerTree_AfterLabelEdit(sender, args);
            customerTree.NodeMouseClick += (sender, args) => customerTree.SelectedNode = args.Node;

            itemsTree.ItemDrag += (sender, args) => tree_BeginDrag(sender, args);
            //itemsTree.DragOver += (sender, args) => tree_DragOver(sender, args);
            //itemsTree.DragDrop += (sender, args) => tree_EndDrag(sender, args);
            //itemsTree.NodeMouseDoubleClick += (sender, args) => tree_AfterDoubleClick(sender, args);
            itemsTree.NodeMouseClick += (sender, args) => itemsTree.SelectedNode = args.Node;

            //ordersTree.ItemDrag += (sender, args) => tree_BeginDrag(sender, args);
            ordersTree.DragOver += (sender, args) => tree_DragOver(sender, args);
            ordersTree.DragDrop += (sender, args) => tree_EndDrag(sender, args);
            ordersTree.NodeMouseClick += (sender, args) => ordersTree.SelectedNode = args.Node;
            //ordersTree.NodeMouseDoubleClick += (sender, args) => tree_AfterDoubleClick(sender, args);

            // LAYOUT
            var table = new TableLayoutPanel();
            table.RowStyles.Clear();
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));

            table.Controls.Add(customersLabel, 0, 0);
            table.Controls.Add(itemsLabel, 1, 0);
            table.Controls.Add(orderTable, 2, 0);
            table.Controls.Add(customerTree, 0, 1);
            table.Controls.Add(itemsTree, 1, 1);
            table.Controls.Add(ordersTree, 2, 1);

            table.Dock = DockStyle.Fill;
            table.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;

            Controls.Add(table);

            InitializeDB(false);
            LoadCustomersFromFile("CUSTOMERS.DAT");
            LoadItemsFromFile("ITEMS.DAT");
            UpdateView();

            //Shown += (sender, args) => InitializeDB(false);
            //Shown += (sender, args) => LoadCustomersFromFile("CUSTOMERS.DAT");
            //Shown += (sender, args) => LoadItemsFromFile("ITEMS.DAT");
            //Shown += (sender, args) => UpdateView();

            db.StateChanged += UpdateView;
        }
    }
}
