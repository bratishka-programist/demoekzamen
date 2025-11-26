using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Zadanie10202
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            loadZaivki();
        }
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
        public string getAdresString(Данные_партнёров партнер)
        {
            var adres = DB.Context.Юридические_адреса
                .FirstOrDefault(a => a.Номер_юридического_адреса == партнер.Юридический_адрес_партнера);
            return adres != null
                ? $"{adres.Индекс}, {adres.Область}, {adres.Город}, {adres.Улица}, {adres.Дом}"
                : "Адрес не указан";
        }
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
        private void addMaterial_Click(object sender, RoutedEventArgs e)
        {
            ДобавлениеРедактирование_заявки добавлениеРедактирование_Заявки =  new ДобавлениеРедактирование_заявки();
            добавлениеРедактирование_Заявки.Show();
            добавлениеРедактирование_Заявки.Title = "Создание заявки";
            this.Close();
        }

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
