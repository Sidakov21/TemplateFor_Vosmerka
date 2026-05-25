using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TemplateFor_Vosmerka.Pages
{
    public partial class AddEditMaterialPage : Page
    {
        private Material _currentMaterial;
        private bool _isEditMode;

        public AddEditMaterialPage(Material material = null)
        {
            InitializeComponent();

            if (Core.LoggedUser == null || Core.LoggedUser.RoleId != 3)
            {
                MessageHelper.ShowError("Доступ запрещен! Только администратор может изменять материалы.");
                NavigationService.GoBack();
                return;
            }

            _isEditMode = material != null;
            _currentMaterial = material ?? new Material();

            LoadCombos();

            if (_isEditMode)
            {
                IdPanel.Visibility = Visibility.Visible;
                LoadMaterialData();
                BtnSave.Content = "Изменить";
            }
            else
            {
                BtnSave.Content = "Добавить";
            }
        }

        private void LoadCombos()
        {
            CbMaterialType.ItemsSource = Core.Context.MaterialType.ToList();
            CbQuantityType.ItemsSource = Core.Context.QuantityType.ToList();
        }

        private void LoadMaterialData()
        {
            TbMaterialId.Text = _currentMaterial.MaterialID.ToString();
            TbMaterialName.Text = _currentMaterial.MaterialName;

            if (_currentMaterial.MaterialType != null)
                CbMaterialType.SelectedItem = _currentMaterial.MaterialType;
            else
                CbMaterialType.SelectedValue = _currentMaterial.MaterialTypeID;

            if (_currentMaterial.QuantityInPac.HasValue)
                TbQuantityInPac.Text = _currentMaterial.QuantityInPac.Value.ToString();

            if (_currentMaterial.QuantityType != null)
                CbQuantityType.SelectedItem = _currentMaterial.QuantityType;
            else
                CbQuantityType.SelectedValue = _currentMaterial.QuantityTypeID;

            if (_currentMaterial.QuantityInHub.HasValue)
                TbQuantityInHub.Text = _currentMaterial.QuantityInHub.Value.ToString();

            if (_currentMaterial.MinQuantity.HasValue)
                TbMinQuantity.Text = _currentMaterial.MinQuantity.Value.ToString();

            if (_currentMaterial.Cost.HasValue)
                TbCost.Text = _currentMaterial.Cost.Value.ToString("F2");
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TbMaterialName.Text))
            {
                MessageHelper.ShowError("Введите наименование материала");
                return;
            }
            if (CbMaterialType.SelectedItem == null)
            {
                MessageHelper.ShowError("Выберите тип материала");
                return;
            }
            if (CbQuantityType.SelectedItem == null)
            {
                MessageHelper.ShowError("Выберите единицу измерения");
                return;
            }

            int? qtyInPac = null;
            if (!string.IsNullOrWhiteSpace(TbQuantityInPac.Text))
            {
                if (!int.TryParse(TbQuantityInPac.Text, out int val) || val < 0)
                {
                    MessageHelper.ShowError("Количество в упаковке должно быть целым неотрицательным числом");
                    return;
                }
                qtyInPac = val;
            }

            int? qtyInHub = null;
            if (!string.IsNullOrWhiteSpace(TbQuantityInHub.Text))
            {
                if (!int.TryParse(TbQuantityInHub.Text, out int val) || val < 0)
                {
                    MessageHelper.ShowError("Количество на складе должно быть целым неотрицательным числом");
                    return;
                }
                qtyInHub = val;
            }

            int? minQty = null;
            if (!string.IsNullOrWhiteSpace(TbMinQuantity.Text))
            {
                if (!int.TryParse(TbMinQuantity.Text, out int val) || val < 0)
                {
                    MessageHelper.ShowError("Минимальный остаток должен быть целым неотрицательным числом");
                    return;
                }
                minQty = val;
            }

            double? cost = null;
            if (!string.IsNullOrWhiteSpace(TbCost.Text))
            {
                if (!double.TryParse(TbCost.Text, out double val) || val < 0)
                {
                    MessageHelper.ShowError("Стоимость должна быть неотрицательным числом");
                    return;
                }
                cost = val;
            }

            _currentMaterial.MaterialName = TbMaterialName.Text.Trim();
            _currentMaterial.MaterialTypeID = ((MaterialType)CbMaterialType.SelectedItem).MaterialTypeID;
            _currentMaterial.QuantityInPac = qtyInPac;
            _currentMaterial.QuantityTypeID = ((QuantityType)CbQuantityType.SelectedItem).QuantityTypeID;
            _currentMaterial.QuantityInHub = qtyInHub;
            _currentMaterial.MinQuantity = minQty;
            _currentMaterial.Cost = cost;

            try
            {
                if (!_isEditMode)
                    Core.Context.Material.Add(_currentMaterial);

                Core.Context.SaveChanges();
                MessageHelper.ShowInfo("Данные сохранены успешно!");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Ошибка при сохранении: {ex.Message}");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
