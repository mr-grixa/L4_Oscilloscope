using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L4_Oscilloscope
{
    public class NoiseFilter
    {
        // Функция для фильтрации шума с использованием выборочного среднего
        public static double[] ApplyMeanFilter(double[] data, int windowSize)
        {
            double[] filteredData = new double[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                int start = Math.Max(0, i - windowSize / 2);
                int end = Math.Min(data.Length - 1, i + windowSize / 2);

                double sum = 0;
                for (int j = start; j <= end; j++)
                {
                    sum += data[j];
                }

                filteredData[i] = sum / (end - start + 1);
            }

            return filteredData;
        }

        // Функция для фильтрации шума с использованием скользящего среднего (простое среднее)
        public static double[] ApplySimpleMovingAverageFilter(double[] data, int windowSize)
        {
            double[] filteredData = new double[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                int start = Math.Max(0, i - windowSize + 1);
                int end = i;

                double sum = 0;
                for (int j = start; j <= end; j++)
                {
                    sum += data[j];
                }

                filteredData[i] = sum / (end - start + 1);
            }

            return filteredData;
        }

        // Функция для фильтрации шума с использованием скользящего среднего (взвешенное среднее)
        public static double[] ApplyWeightedMovingAverageFilter(double[] data, int windowSize)
        {
            double[] filteredData = new double[data.Length];

            double[] weights = Enumerable.Repeat(1.0 / windowSize, windowSize).ToArray();

            for (int i = 0; i < data.Length; i++)
            {
                int start = Math.Max(0, i - windowSize + 1);
                int end = i;

                double sum = 0;
                for (int j = start; j <= end; j++)
                {
                    sum += data[j] * weights[j - start];
                }

                filteredData[i] = sum;
            }

            return filteredData;
        }

        // Функция для фильтрации шума с использованием медианной фильтрации
        public static double[] ApplyMedianFilter(double[] data, int windowSize)
        {
            double[] filteredData = new double[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                int start = Math.Max(0, i - windowSize / 2);
                int end = Math.Min(data.Length - 1, i + windowSize / 2);

                List<double> values = new List<double>();
                for (int j = start; j <= end; j++)
                {
                    values.Add(data[j]);
                }

                values.Sort();
                filteredData[i] = values[windowSize / 2];
            }

            return filteredData;
        }
        public static double[] None(double[] data, int windowSize)
        {
            return data;
        }
    }
}
