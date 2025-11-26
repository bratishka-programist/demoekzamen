using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zadanie10202
{
    public class ВременнаяПродукция
    {
        public Продукции Продукция { get; set; }
        public int Количество { get; set; }
        public decimal Стоимость => Количество * (Продукция?.Минимальная_стоимость_для_партнера ?? 0);
    }
}
