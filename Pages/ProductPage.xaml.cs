using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TemplateFor_Vosmerka.Pages
{
    public partial class ProductPage : Page
    {
        public ProductPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            // Загрузка типов продуктов в ComboBox фильтрации
            var types = Core.Context.ProductType.ToList();
            types.Insert(0, new ProductType { ProductTypeName = "Все типы" });
            FiltrCmbBx.ItemsSource = types;
            FiltrCmbBx.SelectedIndex = 0;

            // Привязка обработчиков событий после инициализации данных, чтобы избежать лишних вызовов
            SearchTxtBx.TextChanged += FilterChanged;
            SortCmbBx.SelectionChanged += FilterChanged;
            FiltrCmbBx.SelectionChanged += FilterChanged;

            UpdateList();
            CheckRole();
        }

        private void CheckRole()
        {
            // Проверка роли пользователя
            if (Core.LoggedUser == null)
            {
                // Гость: скрываем панель администратора
                AdminPanel.Visibility = Visibility.Collapsed;
            }
            else if (Core.LoggedUser.RoleId == 1) // Например, Администратор
            {
                AdminPanel.Visibility = Visibility.Visible;
            }
            else if (Core.LoggedUser.RoleId == 2) // Например, Менеджер
            {
                AdminPanel.Visibility = Visibility.Visible;
                BtnDelete.Visibility = Visibility.Collapsed; // Менеджер не может удалять
            }
            else
            {
                AdminPanel.Visibility = Visibility.Collapsed;

            }
        }

        private void UpdateList()
        {
            var products = Core.Context.Product.AsEnumerable();

            // 1. Поиск по ProductName, ProductType.Name, MaterialName
            string searchText = SearchTxtBx.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchText))
            {
                products = products.Where(p =>
                    p.ProductName.ToLower().Contains(searchText) ||
                    (p.ProductType != null && p.ProductType.ProductTypeName.ToLower().Contains(searchText)) ||
                    (p.Make != null && p.Make.Any(m => m.Material != null && m.Material.MaterialName.ToLower().Contains(searchText)))
                );
            }

            // 2. Фильтрация
            if (FiltrCmbBx.SelectedItem is ProductType selectedType && selectedType.ProductTypeID != 0)
            {
                products = products.Where(p => p.ProductTypeID == selectedType.ProductTypeID);
            }

            // 3. Сортировка
            if (SortCmbBx.SelectedIndex == 1) // По возрастанию цены
            {
                products = products.OrderBy(p => p.MinCost);
            }
            else if (SortCmbBx.SelectedIndex == 2) // По убыванию цены
            {
                products = products.OrderByDescending(p => p.MinCost);
            }

            ProductsListView.ItemsSource = products.ToList();
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            UpdateList();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // NavigationService.Navigate(new Uri("Pages/AddEditProductPage.xaml", UriKind.Relative));
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsListView.SelectedItem is Product selectedProduct)
            {
                // Core.SelectedProduct = selectedProduct;
                // NavigationService.Navigate(new Uri("Pages/AddEditProductPage.xaml", UriKind.Relative));
            }
            else
            {
                MessageHelper.ShowError("Выберите продукцию для редактирования!");
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsListView.SelectedItem is Product selectedProduct)
            {
                if (MessageBox.Show($"Вы уверены, что хотите удалить {selectedProduct.Name}?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        Core.Context.Product.Remove(selectedProduct);
                        Core.Context.SaveChanges();
                        MessageHelper.ShowInfo("Продукция удалена успешно!");
                        UpdateList();
                    }
                    catch (Exception ex)
                    {
                        MessageHelper.ShowError($"Ошибка при удалении: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageHelper.ShowError("Выберите продукцию для удаления!");
            }
        }
    }
}
