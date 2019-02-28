using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSeriesForecastExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var inputTimeSeries = new List<TimeSeriesItem<double>>();
            var count = 0;
            var dateNow = DateTime.UtcNow;
            for (int i = 1; i <= 150; i++)
            {
                count++;
                inputTimeSeries.Add(new TimeSeriesItem<double>() { Time = dateNow.AddSeconds(count), Value = count });
            }

            var predictedTimeSeries = TimeSeriesPrediction.HoltWinters(inputTimeSeries, TimeSpan.FromSeconds(1));

            for (int i = 0; i < predictedTimeSeries["Prediction"].Count; i++)
            {
                Console.WriteLine($"Predicted = {predictedTimeSeries["Prediction"][i].Value}, Upper = {predictedTimeSeries["Upper"][i].Value}, Lower = {predictedTimeSeries["Lower"][i].Value}");
            }


        }
    }
}
