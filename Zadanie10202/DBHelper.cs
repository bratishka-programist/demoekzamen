using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zadanie10202
{
    /// <summary>
    /// Класс для работы с базой данных
    /// </summary>
    public static class DB
    {
        private static readonly Zadanie10202Entities context = new Zadanie10202Entities();

        /// <summary>Контекст БД</summary>
        public static Zadanie10202Entities Context => context;
    }
}