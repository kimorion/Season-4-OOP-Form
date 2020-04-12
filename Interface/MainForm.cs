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

        private Label customersLabel;
        private Label itemsLabel;
        private Label orderLabel;
        private Button addOrderButton;
        ContextMenu userContextMenu;
        ContextMenu itemContextMenu;
        ContextMenu orderContextMenu;
        ContextMenu orderLineContextMenu;

        private TableLayoutPanel orderTable;
        private TableLayoutPanel mainTable;

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

            // Workaround to avoid strange visual bug after TreeView editing
            customerTree.LabelEdit = false;
            itemsTree.LabelEdit = false;
            ordersTree.LabelEdit = false;

            ordersTree.Nodes.Clear();
            customerTree.ExpandAll();
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
                        e.CancelEdit = true;
                        e.Node.EndEdit(true);
                        //UpdateView();
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

        private void DrawCustomerNode(object sender, DrawTreeNodeEventArgs e)
        {
            var tree = sender as TreeView;
            TextRenderer.DrawText(e.Graphics,
                e.Node.Text,
                e.Node.NodeFont,
                new Point(e.Node.Bounds.Left, e.Node.Bounds.Top),
                tree.SelectedNode == e.Node && tree.Focused ? SystemColors.HighlightText : SystemColors.WindowText);
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
            #region ContextMenu
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
            #endregion
        }

        private void CreateWindowControls()
        {
            #region WINDOW CONTROLS

            customersLabel = new Label()
            {
                Text = "Покупатели",
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

            orderTable = new TableLayoutPanel();
            orderTable.Dock = DockStyle.Fill;
            orderTable.RowStyles.Clear();
            orderTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            orderTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            orderTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
            orderTable.Controls.Add(orderLabel, 0, 0);
            orderTable.Controls.Add(addOrderButton, 1, 0);
            #endregion
        }

        private void CreateTrees()
        {            
            customerTree = new BufferedTreeView()
            {
                Dock = DockStyle.Fill,
                ShowNodeToolTips = true,
                CausesValidation = false
            };

            itemsTree = new BufferedTreeView()
            {
                Dock = DockStyle.Fill,
                ShowNodeToolTips = true,
                AllowDrop = true
            };

            ordersTree = new BufferedTreeView()
            {
                Dock = DockStyle.Fill,
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

            customerTree.DrawMode = TreeViewDrawMode.OwnerDrawText;
            itemsTree.DrawMode = TreeViewDrawMode.OwnerDrawText;
            ordersTree.DrawMode = TreeViewDrawMode.OwnerDrawText;

            customerTree.DrawNode += (sender, args) => DrawCustomerNode(sender, args);
            itemsTree.DrawNode += (sender, args) => DrawCustomerNode(sender, args);
            ordersTree.DrawNode += (sender, args) => DrawCustomerNode(sender, args);
        }

        private void CreateLayout()
        {
            #region LAYOUT
            mainTable = new TableLayoutPanel();
            mainTable.RowStyles.Clear();
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));

            mainTable.Controls.Add(customersLabel, 0, 0);
            mainTable.Controls.Add(itemsLabel, 1, 0);
            mainTable.Controls.Add(orderTable, 2, 0);
            mainTable.Controls.Add(customerTree, 0, 1);
            mainTable.Controls.Add(itemsTree, 1, 1);
            mainTable.Controls.Add(ordersTree, 2, 1);

            mainTable.Dock = DockStyle.Fill;
            mainTable.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;

            Controls.Add(mainTable);
            #endregion
        }

        public MainForm()
        {
            CreateMenu();
            CreateContextMenu();
            CreateWindowControls();
            CreateTrees();
            CreateLayout();            

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
