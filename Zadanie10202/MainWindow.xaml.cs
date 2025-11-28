using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Zadanie10202
{
    /// <summary>
    /// Главное окно приложения для управления заявками
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Конструктор главного окна
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            loadZaivki();
        }

        /// <summary>
        /// Загружает список заявок партнеров
        /// </summary>
        public void loadZaivki()
        {
            var zaivki = DB.Context.Данные_партнёров
                .Include("Типы_партнёров")
                .Include("Юридические_адреса")
                .ToList()
                .Select(p => new Class1
                {
                    Партнер = p,
                    adresPartnera = getAdresString(p),
                    summaZaivki = calculateTotalSum(p)
                })
                .ToList();
            listBoxZaivki.ItemsSource = zaivki;
        }

        /// <summary>
        /// Формирует строку адреса партнера
        /// </summary>
        /// <param name="партнер">Партнер</param>
        /// <returns>Строка адреса</returns>
        public string getAdresString(Данные_партнёров партнер)
        {
            var adres = DB.Context.Юридические_адреса
                .FirstOrDefault(a => a.Номер_юридического_адреса == партнер.Юридический_адрес_партнера);
            return adres != null
                ? $"{adres.Индекс}, {adres.Область}, {adres.Город}, {adres.Улица}, {adres.Дом}"
                : "Адрес не указан";
        }

        /// <summary>
        /// Расчет общей суммы заявок партнера
        /// </summary>
        /// <param name="партнер">Партнер</param>
        /// <returns>Сумма в рублях</returns>
        public string calculateTotalSum(Данные_партнёров партнер)
        {
            try
            {
                var поставки = DB.Context.Количества_продукции
                    .Where(k => k.Наименование_партнера == партнер.Номер_партнера)
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

        /// <summary>
        /// Создание новой заявки
        /// </summary>
        private void addMaterial_Click(object sender, RoutedEventArgs e)
        {
            ДобавлениеРедактирование_заявки добавлениеРедактирование_Заявки = new ДобавлениеРедактирование_заявки();
            добавлениеРедактирование_Заявки.Show();
            добавлениеРедактирование_Заявки.Title = "Создание заявки";
            this.Close();
        }

        /// <summary>
        /// Просмотр предложений продукции
        /// </summary>
        private void ShowProductOffersButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = listBoxZaivki.SelectedItem as Class1;
            if (selectedItem != null)
            {
                ПредложениеПродукции предложениеОкно = new ПредложениеПродукции(selectedItem.Партнер);
                предложениеОкно.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Выберите партнера для просмотра предложений продукции!");
            }
        }

        /// <summary>
        /// Редактирование заявки по двойному клику
        /// </summary>
        private void listBoxZaivki_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = listBoxZaivki.SelectedItem as Class1;
            if (selectedItem != null)
            {
                ДобавлениеРедактирование_заявки добавлениеРедактирование_Заявки = new ДобавлениеРедактирование_заявки(selectedItem.Партнер);
                добавлениеРедактирование_Заявки.Show();
                добавлениеРедактирование_Заявки.Title = "Редактирование заявки";
                this.Close();
            }
            else
            {
                MessageBox.Show("Партер не выбран!");
            }
        }
    }
}