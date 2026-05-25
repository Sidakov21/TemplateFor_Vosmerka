using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TemplateFor_Vosmerka.Pages
{
    public partial class AddEditProductPage : Page
    {
        private Product _currentProduct;
        private string _selectedImagePath;
        private bool _isEditMode;

        public AddEditProductPage(Product product = null)
        {
            InitializeComponent();

            if (Core.LoggedUser == null || Core.LoggedUser.RoleId != 3)
            {
                MessageHelper.ShowError("Доступ запрещен! Только администратор может изменять продукцию.");
                NavigationService.GoBack();
                return;
            }

            _isEditMode = product != null;
            _currentProduct = product ?? new Product();

            LoadCombos();

            if (_isEditMode)
            {
                IdPanel.Visibility = Visibility.Visible;
                LoadProductData();
                BtnSave.Content = "Изменить";
            }
            else
            {
                BtnSave.Content = "Добавить";
            }
        }

        private void LoadCombos()
        {
            CbProductType.ItemsSource = Core.Context.ProductType.ToList();
            CbProductPlace.ItemsSource = Core.Context.ProductPlace.ToList();
        }

        private void LoadProductData()
        {
            TbProductId.Text = _currentProduct.ProductID.ToString();
            TbProductName.Text = _currentProduct.ProductName;

            if (_currentProduct.ProductType != null)
                CbProductType.SelectedItem = _currentProduct.ProductType;
            else
                CbProductType.SelectedValue = _currentProduct.ProductTypeID;

            TbArticle.Text = _currentProduct.Article;

            if (_currentProduct.MinCost.HasValue)
                TbCost.Text = _currentProduct.MinCost.Value.ToString();

            if (_currentProduct.HumanResourses.HasValue)
                TbHumanResourses.Text = _currentProduct.HumanResourses.Value.ToString();

            if (_currentProduct.ProductPlace != null)
                CbProductPlace.SelectedItem = _currentProduct.ProductPlace;
            else
                CbProductPlace.SelectedValue = _currentProduct.ProductPlaseId;

            if (!string.IsNullOrEmpty(_currentProduct.Image))
            {
                string cleanPath = _currentProduct.Image.TrimStart('\\', '/');
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, cleanPath);
                if (File.Exists(fullPath))
                {
                    ImgPreview.Source = new BitmapImage(new Uri(fullPath));
                }
            }
        }

        private void BtnLoadImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Выберите изображение для продукции"
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedImagePath = dialog.FileName;
                ImgPreview.Source = new BitmapImage(new Uri(_selectedImagePath));
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TbProductName.Text))
            {
                MessageHelper.ShowError("Введите наименование продукции");
                return;
            }
            if (CbProductType.SelectedItem == null)
            {
                MessageHelper.ShowError("Выберите тип продукции");
                return;
            }
            if (string.IsNullOrWhiteSpace(TbArticle.Text))
            {
                MessageHelper.ShowError("Введите артикул");
                return;
            }

            int? cost = null;
            if (!string.IsNullOrWhiteSpace(TbCost.Text))
            {
                if (!decimal.TryParse(TbCost.Text, out decimal costVal) || costVal < 0)
                {
                    MessageHelper.ShowError("Стоимость должна быть неотрицательным числом");
                    return;
                }
                cost = (int)Math.Round(costVal);
            }

            int? hr = null;
            if (!string.IsNullOrWhiteSpace(TbHumanResourses.Text))
            {
                if (!int.TryParse(TbHumanResourses.Text, out int hrVal) || hrVal < 0)
                {
                    MessageHelper.ShowError("Количество человек должно быть целым неотрицательным числом");
                    return;
                }
                hr = hrVal;
            }

            if (CbProductPlace.SelectedItem == null)
            {
                MessageHelper.ShowError("Выберите номер цеха");
                return;
            }

            _currentProduct.ProductName = TbProductName.Text.Trim();
            _currentProduct.ProductTypeID = ((ProductType)CbProductType.SelectedItem).ProductTypeID;
            _currentProduct.Article = TbArticle.Text.Trim();
            _currentProduct.MinCost = cost;
            _currentProduct.HumanResourses = hr;
            _currentProduct.ProductPlaseId = ((ProductPlace)CbProductPlace.SelectedItem).Id;

            try
            {
                if (!_isEditMode)
                    Core.Context.Product.Add(_currentProduct);

                Core.Context.SaveChanges();

                if (!string.IsNullOrEmpty(_selectedImagePath))
                {
                    if (!string.IsNullOrEmpty(_currentProduct.Image))
                    {
                        string oldPath = Path.Combine(
                            AppDomain.CurrentDomain.BaseDirectory,
                            _currentProduct.Image.TrimStart('\\', '/'));
                        if (File.Exists(oldPath))
                            File.Delete(oldPath);
                    }

                    string productDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "products");
                    if (!Directory.Exists(productDir))
                        Directory.CreateDirectory(productDir);

                    string fileName = $"tire_{_currentProduct.ProductID}.jpg";
                    string savePath = Path.Combine(productDir, fileName);

                    SaveResizedImage(_selectedImagePath, savePath, 300, 200);
                    _currentProduct.Image = $@"\products\{fileName}";
                    Core.Context.SaveChanges();
                }

                MessageHelper.ShowInfo("Данные сохранены успешно!");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Ошибка при сохранении: {ex.Message}");
            }
        }

        private void SaveResizedImage(string sourcePath, string destPath, int maxWidth, int maxHeight)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(sourcePath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            double scale = Math.Min((double)maxWidth / bitmap.PixelWidth, (double)maxHeight / bitmap.PixelHeight);
            int newWidth = (int)(bitmap.PixelWidth * scale);
            int newHeight = (int)(bitmap.PixelHeight * scale);

            var resized = new RenderTargetBitmap(newWidth, newHeight, 96, 96, PixelFormats.Pbgra32);
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                context.DrawImage(bitmap, new Rect(0, 0, newWidth, newHeight));
            }
            resized.Render(visual);

            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(resized));
            using (var stream = new FileStream(destPath, FileMode.Create))
            {
                encoder.Save(stream);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
