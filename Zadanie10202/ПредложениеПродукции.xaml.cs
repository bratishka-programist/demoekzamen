using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Zadanie10202
{
    /// <summary>
    /// Окно предложений продукции партнера
    /// </summary>
    public partial class ПредложениеПродукции : Window
    {
        private Данные_партнёров _currentPartner;

        /// <summary>
        /// Конструктор окна предложений
        /// </summary>
        /// <param name="partner">Партнер</param>
        public ПредложениеПродукции(Данные_партнёров partner)
        {
            InitializeComponent();
            _currentPartner = partner;
            LoadData();
        }

        /// <summary>Загрузка всех данных</summary>
        private void LoadData()
        {
            try
            {
                LoadPartnerInfo();
                LoadProductOffers();
                LoadCalculationComboBox();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        /// <summary>Загрузка информации о партнере</summary>
        private void LoadPartnerInfo()
        {
            if (_currentPartner != null)
            {
                partnerInfoText.Text = _currentPartner.Наименование_партнера;

                var адрес = DB.Context.Юридические_адреса
                    .FirstOrDefault(a => a.Номер_юридического_адреса == _currentPartner.Юридический_адрес_партнера);

                string адресТекст = адрес != null ?
                    $"{адрес.Город}, {адрес.Улица}, {адрес.Дом}" : "Адрес не указан";

                partnerDetailsText.Text =
                    $"Директор: {_currentPartner.Директор} | " +
                    $"Телефон: {_currentPartner.Телефон_партнера} | " +
                    $"Рейтинг: {_currentPartner.Рейтинг} | " +
                    $"Адрес: {адресТекст}";
            }
        }

        /// <summary>Загрузка предложений продукции</summary>
        private void LoadProductOffers()
        {
            try
            {
                var partnerOrders = DB.Context.Количества_продукции
                    .Where(k => k.Наименование_партнера == _currentPartner.Номер_партнера)
                    .ToList();

                var productOffers = partnerOrders
                    .GroupBy(k => k.Продукция)
                    .Select(g => new
                    {
                        НомерПродукции = g.Key,
                        Продукция = DB.Context.Продукции.FirstOrDefault(p => p.Номер_продукции == g.Key),
                        ОбщееКоличество = g.Sum(x => int.TryParse(x.Количество_продукции, out int qty) ? qty : 0)
                    })
                    .Where(x => x.Продукция != null)
                    .Select(x => new ProductOfferViewModel
                    {
                        НомерПродукции = (int)x.НомерПродукции,
                        Наименование = x.Продукция.Наименование_продукции,
                        Артикул = x.Продукция.Артикул,
                        МинимальнаяСтоимость = x.Продукция.Минимальная_стоимость_для_партнера ?? 0,
                        КоличествоВЗаявках = x.ОбщееКоличество,
                        ОбщаяСтоимость = x.ОбщееКоличество * (x.Продукция.Минимальная_стоимость_для_партнера ?? 0)
                    })
                    .ToList();

                productsDataGrid.ItemsSource = productOffers;

                decimal totalSum = productOffers.Sum(p => p.ОбщаяСтоимость);
                totalSumText.Text = $"Итоговая стоимость всех заявок: {totalSum:N2} руб.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки предложений продукции: {ex.Message}");
            }
        }

        /// <summary>Загрузка комбобокса для расчета</summary>
        private void LoadCalculationComboBox()
        {
            try
            {
                var partnerProductIds = DB.Context.Количества_продукции
                    .Where(k => k.Наименование_партнера == _currentPartner.Номер_партнера)
                    .Select(k => k.Продукция)
                    .Distinct()
                    .ToList();

                var productsInOrders = DB.Context.Продукции
                    .Where(p => partnerProductIds.Contains(p.Номер_продукции))
                    .Select(p => new
                    {
                        НомерПродукции = p.Номер_продукции,
                        Наименование = p.Наименование_продукции,
                        ТипПродукции = p.Тип_продукции
                    })
                    .ToList();

                calculationProductComboBox.ItemsSource = productsInOrders;
                calculationProductComboBox.DisplayMemberPath = "Наименование";
                calculationProductComboBox.SelectedValuePath = "НомерПродукции";

                if (productsInOrders.Any())
                    calculationProductComboBox.SelectedIndex = 0;
                else
                    calculationResultText.Text = "У партнера нет продукции в заявках";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка продукции: {ex.Message}");
            }
        }

        /// <summary>Расчет материалов</summary>
        private void CalculateMaterialButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (calculationProductComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Выберите продукцию для расчета");
                    return;
                }

                if (!int.TryParse(requiredQuantityTextBox.Text, out int requiredQuantity) || requiredQuantity <= 0)
                {
                    MessageBox.Show("Введите корректное количество");
                    requiredQuantityTextBox.Focus();
                    return;
                }

                int productId = (int)calculationProductComboBox.SelectedValue;
                var продукция = DB.Context.Продукции.FirstOrDefault(p => p.Номер_продукции == productId);

                if (продукция == null)
                {
                    MessageBox.Show("Продукция не найдена");
                    return;
                }

                var материалыПродукции = DB.Context.Материалы_продукции
                    .Where(mp => mp.Номер_продукции == productId)
                    .ToList();

                if (!материалыПродукции.Any())
                {
                    calculationResultText.Text = "Для выбранной продукции не указаны материалы";
                    return;
                }

                string результат = "Расчет материалов:\n\n";

                foreach (var материал in материалыПродукции)
                {
                    var материалДанные = DB.Context.Материалы
                        .FirstOrDefault(m => m.Номер_материала == материал.Номер_материала);

                    var типМатериала = материалДанные?.Типы_материалов;

                    if (материалДанные != null && типМатериала != null && продукция.Тип_продукции != null)
                    {
                        double параметр1 = 1.0;
                        double параметр2 = 1.0;
                        int количествоНаСкладе = 0;

                        int необходимоеКоличество = MaterialCalculator.CalculateMaterialRequired(
                            продукция.Тип_продукции.Value,
                            материалДанные.Тип_материала.Value,
                            requiredQuantity,
                            количествоНаСкладе,
                            параметр1,
                            параметр2);

                        if (необходимоеКоличество >= 0)
                        {
                            результат += $"{материалДанные.Наименование}: {необходимоеКоличество} ед.\n";
                        }
                        else
                        {
                            результат += $"{материалДанные.Наименование}: ошибка расчета\n";
                        }
                    }
                }

                calculationResultText.Text = результат;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка расчета: {ex.Message}");
            }
        }

        /// <summary>Возврат к главному окну</summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        /// <summary>Изменение выбора продукции</summary>
        private void calculationProductComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (calculationProductComboBox.SelectedValue == null)
                    return;

                int productId = (int)calculationProductComboBox.SelectedValue;

                var quantities = DB.Context.Количества_продукции
                    .Where(k => k.Наименование_партнера == _currentPartner.Номер_партнера &&
                               k.Продукция == productId)
                    .ToList()
                    .Select(k => int.TryParse(k.Количество_продукции, out int qty) ? qty : 0);

                var totalQuantity = quantities.Sum();
                requiredQuantityTextBox.Text = totalQuantity.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке количества: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Модель для отображения предложений
    /// </summary>
    public class ProductOfferViewModel
    {
        /// <summary>ID продукции</summary>
        public int НомерПродукции { get; set; }

        /// <summary>Название продукции</summary>
        public string Наименование { get; set; }

        /// <summary>Артикул</summary>
        public string Артикул { get; set; }

        /// <summary>Минимальная стоимость</summary>
        public decimal МинимальнаяСтоимость { get; set; }

        /// <summary>Количество в заявках</summary>
        public int КоличествоВЗаявках { get; set; }

        /// <summary>Общая стоимость</summary>
        public decimal ОбщаяСтоимость { get; set; }
    }
}