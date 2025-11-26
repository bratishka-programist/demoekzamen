using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zadanie10202
{
    public static class DB
    {
        private static readonly Zadanie10202Entities context = new Zadanie10202Entities();
        public static Zadanie10202Entities Context => context;
    }
}
