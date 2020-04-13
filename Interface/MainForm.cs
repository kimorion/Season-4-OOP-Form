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
        NodeLabelParser parser = new NodeLabelParser();

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
                db.TryGetCustomer(args.id, out Customer customer);
                LoadCustomerOrders(customer);

                orderTree.ExpandAll();
            }
            customerTree.ExpandAll();
        }

        void ShowDiscountWarning(uint orderNumber, string discountName, string reason)
        {
            MessageBox.Show(string.Format("{0} была удалена из заказа '{1}'.\nПричина: {2}", discountName, orderNumber, reason),
                "Внимание");
        }

        void ShowWarning(string message)
        {
            MessageBox.Show(message, "Внимание");
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

        void CreateOrder(string id)
        {
            Order newOrder = new Order((uint)random.Next(1000000000, 2000000000), "Please, enter address", DeliveryType.Standard);
            db.TryAddOrder(id, newOrder);
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
            TreeNode nodeSource = (TreeNode)e.Data.GetData(typeof(TreeNode));
            e.Effect = DragDropEffects.None;

            if (nodeSource != null)
            {

                Point pt = new Point(e.X, e.Y);
                pt = tree.PointToClient(pt);
                TreeNode nodeTarget = tree.GetNodeAt(pt);
                if (nodeTarget == null) return;

                if (nodeTarget.Name.Equals("items") && nodeSource.Name == "item")
                {
                    e.Effect = DragDropEffects.Copy;
                    tree.SelectedNode = nodeTarget;
                }

                if (nodeTarget.Name.Equals("discounts") && nodeSource.Name == "discount")
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
            var sourceArgs = nodeSource.Tag as OrderArgs;
            var targetArgs = nodeTarget.Tag as OrderArgs;

            if (nodeSource.Name == "item" && nodeTarget.Name == "items")
            {
                uint quantity = (uint)Prompt.ShowDialog("Enter quantity", "Enter quantity");
                db.TryAddItemToOrder(targetArgs.id, targetArgs.orderNumber, sourceArgs.itemArticle, quantity);
            }
            if (nodeSource.Name == "discount" && nodeTarget.Name == "discounts")
            {
                db.TryAddDiscount(targetArgs.id, targetArgs.orderNumber, sourceArgs.discountName);
            }
        }


        private void tree_AfterDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var tree = sender as TreeView;
            var node = e.Node;
            if (node == null || node.Tag == null)
                return;

            tree.SelectedNode = node;

            switch (node.Name)
            {
                case "address":
                case "name":
                case "phone":
                case "privilege":
                case "creationDate":
                case "deliveryType":
                case "quantity":
                    {
                        tree.LabelEdit = true;
                        if (!node.IsEditing)
                        {
                            node.BeginEdit();
                        }
                        break;
                    }
                default:
                    return;
            }

        }

        private void Tree_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            var node = e.Node;
            if (e.Label == null)
            {
                e.CancelEdit = true;
                node.EndEdit(true);
                return;
            }

            var args = node.Tag as OrderArgs;
            switch (node.Name)
            {
                case "address":
                    {
                        if (parser.TryParseAddress(e.Label, out string result))
                            if (db.TryEditAddress(args.id, args.orderNumber, result))
                            {
                                node.EndEdit(false);
                                return;
                            }
                        ShowWarning("При вводе адреса разрешены только точка и запятая.");
                        break;
                    }
                case "name":
                    {
                        if (parser.TryParseName(e.Label, out FullName result))
                            if (db.TryEditName(args.id, result))
                            {
                                node.EndEdit(false);
                                return;
                            }
                        ShowWarning("ФИО должно быть написано через пробел и без лишних знаков");
                        break;
                    }
                case "phone":
                    {
                        if (parser.TryParsePhoneNumber(e.Label, out string result))
                            if (db.TryEditPhoneNumber(args.id, result))
                            {
                                node.EndEdit(false);
                                return;
                            }
                        ShowWarning("При вводе телефона используйте только цифры и знак '+'");
                        break;
                    }
                case "privilege":
                    {
                        if (Enum.TryParse(e.Label, out Privilege result))
                            if (db.TryEditPrivilege(args.id, result))
                            {
                                node.EndEdit(false);
                                return;
                            }
                        ShowWarning("На данный момент доступны только Common и Premium привилегии");
                        break;
                    }
                case "creationDate":
                    {
                        if (DateTimeOffset.TryParse(e.Label, out DateTimeOffset newDate))
                            if (db.TryEditCreationDate(args.id, args.orderNumber, newDate))
                            {
                                node.EndEdit(false);
                                return;
                            }
                        ShowWarning("Введите дату в формате dd.MM.yyyy");
                        break;
                    }
                case "deliveryType":
                    {
                        if (Enum.TryParse(e.Label, out DeliveryType result))
                            if (db.TryEditDeliveryType(args.id, args.orderNumber, result))
                            {
                                node.EndEdit(false);
                                return;
                            }
                        ShowWarning("На данный момент доступна только Standard и Express доставка");
                        break;
                    }
                case "quantity":
                    {
                        if (uint.TryParse(e.Label, out uint result))
                            if (db.TryEditItemQuantity(args.id, args.orderNumber, args.itemArticle, result))
                            {
                                node.EndEdit(false);
                                return;
                            }
                        break;
                    }

            }
            e.CancelEdit = true;
            e.Node.EndEdit(true);

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
                        db.TryDeleteOrder(orderArgs.id, orderArgs.orderNumber);
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
                        db.TryDeleteItemFromOrder(orderArgs.id, orderArgs.orderNumber, orderArgs.itemArticle);
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
                        var orderArgs = itemTree.SelectedNode.Tag as OrderArgs;
                        throw new NotImplementedException();
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
                        var orderArgs = customerTree.SelectedNode.Tag as OrderArgs;
                        db.TryGetCustomer(orderArgs.id, out Customer customer);
                        LoadCustomerOrders(customer);
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
                        db.TryRemoveDiscount(orderArgs.id, orderArgs.orderNumber, orderArgs.discountName);
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
                    CreateOrder(orderArgs.id);
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
            customerTree.AfterLabelEdit += (sender, args) => Tree_AfterLabelEdit(sender, args);
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
            orderTree.AfterLabelEdit += (sender, args) => Tree_AfterLabelEdit(sender, args);

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
            db.UserWarning += ShowWarning;
        }


    }
}
