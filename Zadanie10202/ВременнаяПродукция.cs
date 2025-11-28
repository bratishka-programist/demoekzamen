using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zadanie10202
{
    /// <summary>
    /// Временная продукция для заявки
    /// </summary>
    public class ВременнаяПродукция
    {
        /// <summary>Продукция</summary>
        public Продукции Продукция { get; set; }

        /// <summary>Количество</summary>
        public int Количество { get; set; }

        /// <summary>Стоимость</summary>
        public decimal Стоимость => Количество * (Продукция?.Минимальная_стоимость_для_партнера ?? 0);
    }
}