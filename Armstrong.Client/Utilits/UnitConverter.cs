namespace Armstrong.Client.Utilits
{
    struct DeviceType
    {
        public const int EquivalenDoseRate = 1;
        public const int GasVolumetricActivity = 2;
        public const int AerosolVolumetricActivity = 3;
        public const int Impulse = 4;
    };

    /// <summary>
    /// Предоставляет методы для конвертации импульсов и других значений в требуемую величину.
    /// </summary>
    public static class UnitConverter
    {
        /// <summary>
        /// Преобразует количество импульсов в требуемую величину, в зависимости от типа блока детектирования.
        /// </summary>
        /// <param name="type">Тип блока детектирования.</param>
        /// <param name="coefficient">Коэффициент преобразования.</param>
        /// <param name="n">Количество импульсов полученных с блока детектирования.</param>
        /// <returns>Пересчитанное значение из импульсов в величину в зависимости от тиба блока детектирования.</returns>
        static public double Convert(int type, double coefficient, double n)
        {
            //BDMG coefficient = 1, BDGB coefficient = 0.0000019f, BDAS coefficient = 2.0592f;

            switch (type)
            {
                case DeviceType.EquivalenDoseRate: return n * coefficient * 0.001f;
                case DeviceType.GasVolumetricActivity: return n != 0 ? 1 / (n * coefficient) : 0;
                case DeviceType.AerosolVolumetricActivity: return n != 0 ? n / coefficient : 0;
                case DeviceType.Impulse: return n != 0 ? n * coefficient : 0;
                default: return 1;
            }
        }

        /// <summary>
        /// Преобразует системные единицы, такие как Зиверты и Бикерели в внесистемные единицы, такие как Рентген и Кюри.
        /// </summary>
        /// <param name="type">Тип блока детектирования.</param>
        /// <param name="value">Значение МЭД или ОА аэрозолей или газов, поученное с блока детектирования.</param>
        /// <returns>Значение, пересчитанное из системных единиц в внесистемные единицы.</returns>
        static public double Convert(int type, double value)
        {
            // Пересчет из мкЗв/ч в мкР/с и Бк/м.куб в Ки/л
            // 1 мкЗв/ч     = 27.777        мкР/с
            // 1 Бк/м.куб   = 370000000000  Ки/л

            double curie = 37000000000000;
            double roentgen = 27.777f;

            switch (type)
            {
                case DeviceType.EquivalenDoseRate: return value * roentgen;
                case DeviceType.GasVolumetricActivity: return value / curie;
                case DeviceType.AerosolVolumetricActivity: return value / curie;
                default: return value;
            }
        }
    }
}
