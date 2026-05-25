using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TemplateFor_Vosmerka.Pages
{
    public partial class MaterialPage : Page
    {
        public MaterialPage()
        {
            InitializeComponent();
            Loaded += (s, e) => { UpdateList(); CheckRole(); };
            LoadData();
        }

        private void LoadData()
        {
            var types = Core.Context.MaterialType.ToList();
            types.Insert(0, new MaterialType { MaterialTypeName = "Все типы" });
            FiltrCmbBx.ItemsSource = types;
            FiltrCmbBx.SelectedIndex = 0;

            SearchTxtBx.TextChanged += FilterChanged;
            SortCmbBx.SelectionChanged += FilterChanged;
            FiltrCmbBx.SelectionChanged += FilterChanged;
        }

        private void CheckRole()
        {
            if (Core.LoggedUser == null)
            {
                AdminPanel.Visibility = Visibility.Collapsed;
            }
            else if (Core.LoggedUser.RoleId == 3)
            {
                AdminPanel.Visibility = Visibility.Visible;
            }
            else
            {
                AdminPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateList()
        {
            var materials = Core.Context.Material.AsEnumerable();

            string searchText = SearchTxtBx.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchText))
            {
                materials = materials.Where(m =>
                    m.MaterialName.ToLower().Contains(searchText) ||
                    (m.MaterialType != null && m.MaterialType.MaterialTypeName.ToLower().Contains(searchText))
                );
            }

            if (FiltrCmbBx.SelectedItem is MaterialType selectedType && selectedType.MaterialTypeID != 0)
            {
                materials = materials.Where(m => m.MaterialTypeID == selectedType.MaterialTypeID);
            }

            if (SortCmbBx.SelectedIndex == 1)
                materials = materials.OrderBy(m => m.Cost);
            else if (SortCmbBx.SelectedIndex == 2)
                materials = materials.OrderByDescending(m => m.Cost);
            else if (SortCmbBx.SelectedIndex == 3)
                materials = materials.OrderBy(m => m.QuantityInHub);

            MaterialsListView.ItemsSource = materials.ToList();
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            UpdateList();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditMaterialPage());
        }

        private void MaterialsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (MaterialsListView.SelectedItem is Material selected)
            {
                NavigationService.Navigate(new AddEditMaterialPage(selected));
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MaterialsListView.SelectedItem is Material selected)
            {
                if (MessageBox.Show($"Вы уверены, что хотите удалить \"{selected.MaterialName}\"?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        Core.Context.Material.Remove(selected);
                        Core.Context.SaveChanges();
                        MessageHelper.ShowInfo("Материал удалён успешно!");
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
                MessageHelper.ShowError("Выберите материал для удаления!");
            }
        }

        private void BtnBackToProducts_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
