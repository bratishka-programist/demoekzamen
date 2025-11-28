using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zadanie10202
{
        public static class MaterialCalculator
        {
            /// <summary>
            /// Расчет количества материала для производства продукции
            /// </summary>
            /// <param name="productTypeId">Идентификатор типа продукции</param>
            /// <param name="materialTypeId">Идентификатор типа материала</param>
            /// <param name="requiredQuantity">Требуемое количество продукции</param>
            /// <param name="stockQuantity">Количество продукции на складе</param>
            /// <param name="parameter1">Параметр продукции 1 (вещественное число)</param>
            /// <param name="parameter2">Параметр продукции 2 (вещественное число)</param>
            /// <returns>Количество необходимого материала или -1 при ошибке</returns>
            public static int CalculateMaterialRequired(
                int productTypeId,
                int materialTypeId,
                int requiredQuantity,
                int stockQuantity,
                double parameter1,
                double parameter2)
            {
                try
                {
                    // Проверка входных параметров
                    if (parameter1 <= 0 || parameter2 <= 0 || requiredQuantity < 0 || stockQuantity < 0)
                        return -1;

                    // Получение данных из базы
                    using (var context = new Zadanie10202Entities())
                    {
                        var productType = context.Типы_продукции
                            .FirstOrDefault(pt => pt.Номер_типа_продукции == productTypeId);

                        var materialType = context.Типы_материалов
                            .FirstOrDefault(mt => mt.Номер_типа_материала == materialTypeId);

                        // Проверка существования типов
                        if (productType == null || materialType == null)
                            return -1;

                        // Расчет количества продукции для производства
                        int productionQuantity = Math.Max(0, requiredQuantity - stockQuantity);

                        if (productionQuantity == 0)
                            return 0;

                        // Расчет материала на единицу продукции
                        double materialPerUnit = parameter1 * parameter2 *
                                               (productType.Коэффициент_типа_продукции ?? 1.0);

                        // Учет брака материала
                        double defectMultiplier = 1.0 + (materialType.Процент_брака_материала ?? 0.0);

                        // Общее количество материала с учетом брака
                        double totalMaterial = productionQuantity * materialPerUnit * defectMultiplier;

                        // Округление до целого в большую сторону
                        return (int)Math.Ceiling(totalMaterial);
                    }
                }
                catch (Exception)
                {
                    return -1;
                }
            }
        }
}
