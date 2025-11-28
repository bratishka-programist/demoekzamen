using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace Zadanie10202
{
    /// <summary>
    /// Окно добавления и редактирования заявок
    /// </summary>
    public partial class ДобавлениеРедактирование_заявки : Window
    {
        private Данные_партнёров currentPartner;
        private bool isEditMode;
        private List<ВременнаяПродукция> selectedProducts = new List<ВременнаяПродукция>();

        /// <summary>
        /// Конструктор для создания заявки
        /// </summary>
        public ДобавлениеРедактирование_заявки()
        {
            InitializeComponent();
            isEditMode = false;
            loadComboTypePartner();
            loadComboProducts();
            infoText.Text = "Предложена вся продукция компании";

            dobProd.Visibility = Visibility.Collapsed;
            prodVZaivki.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Конструктор для редактирования заявки
        /// </summary>
        /// <param name="партнер">Партнер для редактирования</param>
        public ДобавлениеРедактирование_заявки(Данные_партнёров партнер)
        {
            InitializeComponent();
            currentPartner = партнер;
            isEditMode = true;
            loadComboTypePartner();
            loadComboDataPartner();
            loadComboProductsKotoriyBilZakazan();
            infoText.Text = "Предложена продукция для конкретного партнера";

            loadDataGridData();
            itogoSum.Text = "Итого: " + calculateTotalSum();
        }

        /// <summary>Загрузка типов партнеров в комбобокс</summary>
        private void loadComboTypePartner()
        {
            typeOfPartner.ItemsSource = DB.Context.Типы_партнёров.ToList();
            typeOfPartner.DisplayMemberPath = "Тип_партнёров";
            typeOfPartner.SelectedValuePath = "Номер_типа_партнёров";
        }

        /// <summary>Загрузка всей продукции</summary>
        private void loadComboProducts()
        {
            productsComboBox.ItemsSource = DB.Context.Продукции.ToList();
            productsComboBox.SelectedValuePath = "Номер_продукции";
            productsComboBox.DisplayMemberPath = "Наименование_продукции";
        }

        /// <summary>Загрузка продукции из заявок партнера</summary>
        private void loadComboProductsKotoriyBilZakazan()
        {
            try
            {
                var productBilZakazaniy = DB.Context.Количества_продукции
                    .Where(p => p.Наименование_партнера == currentPartner.Номер_партнера)
                    .Select(p => p.Продукция)
                    .Distinct()
                    .ToList();

                var filterProducts = DB.Context.Продукции
                    .Where(p => productBilZakazaniy.Contains(p.Номер_продукции))
                    .ToList();

                productsComboBox.ItemsSource = filterProducts;
                productsComboBox.SelectedValuePath = "Номер_продукции";
                productsComboBox.DisplayMemberPath = "Наименование_продукции";
            }
            catch (Exception exe)
            {
                MessageBox.Show($"Ошибка загрузки{exe.Message}");
            }
        }

        /// <summary>Загрузка данных в DataGrid</summary>
        private void loadDataGridData()
        {
            if (!isEditMode) return;

            var zaivkiPartnera = DB.Context.Количества_продукции
                .Where(z => z.Наименование_партнера == currentPartner.Номер_партнера)
                .Include("Продукции")
                .ToList();

            selectedProducts = zaivkiPartnera.Select(z => new ВременнаяПродукция
            {
                Продукция = z.Продукции,
                Количество = int.TryParse(z.Количество_продукции, out int qty) ? qty : 0
            }).ToList();

            UpdateDataGrid();
            UpdateTotalSum();
        }

        /// <summary>Расчет общей суммы заявки</summary>
        /// <returns>Сумма в рублях</returns>
        public string calculateTotalSum()
        {
            try
            {
                var поставки = DB.Context.Количества_продукции
                    .Where(k => k.Наименование_партнера == currentPartner.Номер_партнера)
                    .ToList();
                decimal total = 0;
                foreach (var поставка in поставки)
                {
                    var продукция = DB.Context.Продукции.FirstOrDefault(p => p.Номер_продукции == поставка.Продукция);
                    if (продукция != null && int.TryParse(поставка.Количество_продукции, out int количество))
                    {
                        total += количество * (продукция.Минимальная_стоимость_для_партнера ?? 0);
                    }
                }
                return $"{total:N2} руб.";
            }
            catch (Exception)
            {
                return "Ошибка расчета";
            }
        }

        /// <summary>Загрузка данных партнера в форму</summary>
        private void loadComboDataPartner()
        {
            if (currentPartner == null) return;
            try
            {
                currentPartner = DB.Context.Данные_партнёров
                    .Include("Типы_партнёров")
                    .Include("Юридические_адреса")
                    .FirstOrDefault(p => p.Номер_партнера == currentPartner.Номер_партнера);

                if (currentPartner != null)
                {
                    typeOfPartner.SelectedValue = currentPartner.Тип_партнера;
                    namePartner.Text = currentPartner.Наименование_партнера;
                    nameDirector.Text = currentPartner.Директор;
                    reitPart.Text = currentPartner.Рейтинг.ToString();
                    telPart.Text = currentPartner.Телефон_партнера;
                    emailPart.Text = currentPartner.Электронная_почта_партнера;
                    innPart.Text = currentPartner.ИНН;

                    if (currentPartner.Юридические_адреса != null)
                    {
                        var adress = currentPartner.Юридические_адреса;
                        indexPart.Text = adress.Индекс;
                        oblastPart.Text = adress.Область;
                        cityPart.Text = adress.Город;
                        streetPart.Text = adress.Улица;
                        domPart.Text = adress.Дом.ToString();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Ошибка загрузки: {e.Message}");
            }
        }

        /// <summary>Сохранение заявки</summary>
        private void saveRequest_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(reitPart.Text) && (!int.TryParse(reitPart.Text, out int rating) || rating < 0))
            {
                MessageBox.Show("Рейтинг должен быть целым неотрицательным числом", "Ошибка");
                reitPart.Text = null;
                return;
            }

            if (!string.IsNullOrEmpty(domPart.Text) && (!int.TryParse(domPart.Text, out int domic) || domic < 0))
            {
                MessageBox.Show("Дом должен быть целым неотрицательным числом", "Ошибка");
                domPart.Text = null;
                return;
            }

            if (!string.IsNullOrEmpty(indexPart.Text) && ((!long.TryParse(indexPart.Text, out long index)) || indexPart.Text.Length != 6))
            {
                MessageBox.Show("Индекс это комбинация из 6 чисел", "Ошибка");
                indexPart.Text = null;
                return;
            }

            if (!string.IsNullOrEmpty(innPart.Text) && ((!long.TryParse(innPart.Text, out long inn)) || innPart.Text.Length != 10))
            {
                MessageBox.Show("ИНН это комбинация из 10 чисел", "Ошибка");
                innPart.Text = null;
                return;
            }

            if (!string.IsNullOrEmpty(telPart.Text) && ((!long.TryParse(telPart.Text, out long tel)) || telPart.Text.Length != 11))
            {
                MessageBox.Show("Номер телефона это комбинация из 11 чисел", "Ошибка");
                telPart.Text = null;
                return;
            }

            try
            {
                if (isEditMode)
                {
                    updatePartner();
                    SaveProductsToDatabase(currentPartner.Номер_партнера);
                    DB.Context.SaveChanges();
                    MessageBox.Show("Заявка успешно отредактирована!");
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    if (currentPartner == null)
                    {
                        createPartner();
                        return;
                    }
                    else
                    {
                        SaveProductsToDatabase(currentPartner.Номер_партнера);
                        DB.Context.SaveChanges();
                        MessageBox.Show("Заявка успешно создана!");
                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Show();
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }

        /// <summary>Сохранение продукции в БД</summary>
        /// <param name="partnerId">ID партнера</param>
        private void SaveProductsToDatabase(int partnerId)
        {
            if (selectedProducts.Count == 0) return;

            foreach (var продукт in selectedProducts)
            {
                var новаяЗапись = new Количества_продукции
                {
                    Наименование_партнера = partnerId,
                    Продукция = продукт.Продукция.Номер_продукции,
                    Количество_продукции = продукт.Количество.ToString()
                };

                DB.Context.Количества_продукции.Add(новаяЗапись);
            }
        }

        /// <summary>Обновление данных партнера</summary>
        private void updatePartner()
        {
            if (currentPartner == null) return;
            currentPartner.Тип_партнера = (int)typeOfPartner.SelectedValue;
            currentPartner.Наименование_партнера = namePartner.Text;
            currentPartner.Директор = nameDirector.Text;
            currentPartner.Рейтинг = int.Parse(reitPart.Text);
            currentPartner.Телефон_партнера = telPart.Text;
            currentPartner.Электронная_почта_партнера = emailPart.Text;
            currentPartner.ИНН = innPart.Text;

            if (currentPartner.Юридические_адреса != null)
            {
                currentPartner.Юридические_адреса.Индекс = indexPart.Text;
                currentPartner.Юридические_адреса.Область = oblastPart.Text;
                currentPartner.Юридические_адреса.Город = cityPart.Text;
                currentPartner.Юридические_адреса.Улица = streetPart.Text;
                currentPartner.Юридические_адреса.Дом = int.Parse(domPart.Text);
            }
        }

        /// <summary>Создание нового партнера</summary>
        private void createPartner()
        {
            var newAdress = new Юридические_адреса
            {
                Индекс = indexPart.Text,
                Область = oblastPart.Text,
                Город = cityPart.Text,
                Улица = streetPart.Text,
                Дом = int.Parse(domPart.Text)
            };
            DB.Context.Юридические_адреса.Add(newAdress);
            DB.Context.SaveChanges();
            var newParner = new Данные_партнёров
            {
                Тип_партнера = (int)typeOfPartner.SelectedValue,
                Наименование_партнера = namePartner.Text,
                Директор = nameDirector.Text,
                Юридический_адрес_партнера = newAdress.Номер_юридического_адреса,
                Рейтинг = int.Parse(reitPart.Text),
                Телефон_партнера = telPart.Text,
                Электронная_почта_партнера = emailPart.Text,
                ИНН = innPart.Text
            };
            DB.Context.Данные_партнёров.Add(newParner);
            DB.Context.SaveChanges();
            currentPartner = newParner;
            dobProd.Visibility = Visibility.Visible;
            prodVZaivki.Visibility = Visibility.Visible;
        }

        /// <summary>Отмена создания заявки</summary>
        private void cancelRequest_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        /// <summary>Добавление продукции в заявку</summary>
        private void addProduct_Click(object sender, RoutedEventArgs e)
        {
            if (currentPartner == null)
            {
                MessageBox.Show("Сначала создайте партнера");
                return;
            }

            var selectedProduct = productsComboBox.SelectedItem as Продукции;
            if (selectedProduct == null)
            {
                MessageBox.Show("Выберите продукт");
                return;
            }

            if (!int.TryParse(quantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество");
                quantityTextBox.Text = "1";
                return;
            }

            try
            {
                var новаяПродукция = new ВременнаяПродукция
                {
                    Продукция = selectedProduct,
                    Количество = quantity
                };

                selectedProducts.Add(новаяПродукция);
                UpdateDataGrid();
                UpdateTotalSum();

                quantityTextBox.Text = "1";
                productsComboBox.SelectedIndex = -1;

                MessageBox.Show("Продукция добавлена в заявку!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении продукции: {ex.Message}");
            }
        }

        /// <summary>Обновление DataGrid</summary>
        private void UpdateDataGrid()
        {
            selectedProductsDataGrid.ItemsSource = null;
            selectedProductsDataGrid.ItemsSource = selectedProducts;
        }

        /// <summary>Обновление общей суммы</summary>
        private void UpdateTotalSum()
        {
            decimal total = selectedProducts.Sum(p => p.Стоимость);
            itogoSum.Text = $"ИТОГО: {total:N2} руб.";
        }
    }
}