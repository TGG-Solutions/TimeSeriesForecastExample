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
            //var inputTimeSeries = new List<TimeSeriesItem<double>>();
            //var count = 0;
            //var dateNow = DateTime.UtcNow;
            //for (int i = 1; i <= 150; i++)
            //{
            //    count++;
            //    inputTimeSeries.Add(new TimeSeriesItem<double>() { Time = dateNow.AddSeconds(count), Value = count });
            //}

            var inputTimeSeries = new List<TimeSeriesItem<double>>();
            var count = 0;
            var dateNow = DateTime.UtcNow;
            for (int i = 1; i <= 5; i++)
            {
                count++;
                //inputTimeSeries.Add(new TimeSeriesItem<double>() { Time = dateNow.AddSeconds(count), Value = count });

                if (i == 1) inputTimeSeries.Add(new TimeSeriesItem<double>() { Time = dateNow.AddSeconds(count), Value = count });
                if (i == 2) inputTimeSeries.Add(new TimeSeriesItem<double>() { Time = dateNow.AddSeconds(count+7), Value = count });
                if (i == 3) inputTimeSeries.Add(new TimeSeriesItem<double>() { Time = dateNow.AddSeconds(count+100), Value = count });
                if (i == 4) inputTimeSeries.Add(new TimeSeriesItem<double>() { Time = dateNow.AddSeconds(count+544), Value = count });
                if (i == 5) inputTimeSeries.Add(new TimeSeriesItem<double>() { Time = dateNow.AddSeconds(count+1010), Value = count });
            }

            var predictedTimeSeries = TimeSeriesPrediction.HoltWinters(inputTimeSeries);

            for (int i = 0; i < predictedTimeSeries["Prediction"].Count; i++)
            {
                Console.WriteLine($"Predicted = {predictedTimeSeries["Prediction"][i].Value}, Upper = {predictedTimeSeries["Upper"][i].Value}, Lower = {predictedTimeSeries["Lower"][i].Value}");
            }



        }
    }
}
