using RDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSeriesForecastExample
{
    public class TimeSeriesItem<T>
    {
        public DateTime Time { get; set; }
        public T Value { get; set; }
    }
    public class TimeSeriesPrediction
    {
        // Cannot assume that Visual KPI brings us evenly sampled data as input!
        public static bool IsEvenlySpaced(List<TimeSeriesItem<double>> sampledInputValues)
        {
            var even = true;

            if (sampledInputValues.Count < 3) return false;

            var previousDifference = 0.0;
            for (int i = 1; i <= sampledInputValues.Count -1; i++)
            {
                if (i == 1)
                {
                    previousDifference = (sampledInputValues[1].Time - sampledInputValues[0].Time).TotalSeconds;
                }
                else
                {
                    if (previousDifference != (sampledInputValues[i].Time - sampledInputValues[i-1].Time).TotalSeconds)
                    {
                        even = false;
                        break;
                    }
                }
            }

            return even;
        }
        public static List<TimeSeriesItem<double>> InterpolateRawValuesEvenly(List<TimeSeriesItem<double>> sampledInputValues)
        {
            var evenlySpaced = new List<TimeSeriesItem<double>>();

            evenlySpaced = sampledInputValues.Resample(
                sampledInputValues.Min(a => a.Time), 
                sampledInputValues.Max(a => a.Time), 
                TimeSpan.FromSeconds(1), 
                data => data.Time, 
                (curDate, data1, data2, t) => new TimeSeriesItem<double> { Time = curDate, Value = data1.Value + ((data2.Value - data1.Value) * t) }).ToList();

            return evenlySpaced;
        }

        public static Dictionary<string, List<TimeSeriesItem<double>>> HoltWinters(List<TimeSeriesItem<double>> sampledInputValues, TimeSpan sampleInterval)
        {
            // Need evenly spaced data.
            // If evenly spaced data is supplied then great, otherwise we need to interpolate the data (from first value to the last value).
            if (!IsEvenlySpaced(sampledInputValues))
            {
                sampledInputValues = InterpolateRawValuesEvenly(sampledInputValues);
            }

            // Our Return Value
            var predictedValues = new Dictionary<string, List<TimeSeriesItem<double>>>();

            // Use and Dispose the R Engine
            REngine engine = REngine.GetInstance();

            // Make sure the R Engine is ready
            engine.Initialize();

            // Numeric Vector based on input sampled values
            NumericVector numericVector = engine.CreateNumericVector(sampledInputValues.Select(si => si.Value));

            //engine.Evaluate("install.packages(\"forecast\")");
            engine.Evaluate("library(forecast)");

            // Set the Symbol within the R Engine
            engine.SetSymbol("timeseries", numericVector);

            // R Engine Time Series compilation
            int freq = int.Parse((sampledInputValues.Count / 16).ToString("0"));
            engine.Evaluate($"istm = ts(timeseries, freq={freq})");

            // INFO: We might in the future interchange prediction models
            // INFO: For now it is only HoltWinters

            // Fit the Time Series to HoltWinters
            engine.Evaluate($"t = HoltWinters(istm,seasonal=\"additive\")");
            //engine.Evaluate($"t = ets(istm)");

            // Make the Prediction
            engine.Evaluate($"p = predict(t,{freq},prediction.interval = TRUE,interval=\"confidence\")");
            //engine.Evaluate($"p = forecast(t,h={freq})");

            // Assign the results to a symbol in R so that we can extract it
            engine.Evaluate($"output = as.numeric(p)");
            //engine.Evaluate($"outputUpper = as.numeric(p$upper)");
            //engine.Evaluate($"outputLower = as.numeric(p$lower)");
            //engine.Evaluate($"outputFitted = as.numeric(p$fitted)");

            // Get the numeric vector out
            var timeSeriesPrediction = engine.GetSymbol("output").AsNumeric().ToArray();
            //var timeSeriesPredictionUpper = engine.GetSymbol("outputUpper").AsNumeric().ToArray();
            //var timeSeriesPredictionLower = engine.GetSymbol("outputLower").AsNumeric().ToArray();
            //var timeSeriesPredictionFitted = engine.GetSymbol("outputFitted").AsNumeric().ToArray();

            // Make the results into a Time Series Item Array
            var maxDateTime = sampledInputValues.Max(si => si.Time);

            // Returned Time Series Predictions include Upper/Lower Intervals
            // Array size = 3 x Requested Prediction Count
            var prediction = new List<TimeSeriesItem<double>>();
            for (int index = 0; index < freq; index++)
            {
                // Set the Time depending on where we are in the index
                // Larger the index then further out the prediction
                prediction.Add(new TimeSeriesItem<double>() { Time = maxDateTime.AddMilliseconds(sampleInterval.TotalMilliseconds * (index + 1)), Value = timeSeriesPrediction[index] });
            }
            predictedValues.Add("Prediction", prediction);

            var upper = new List<TimeSeriesItem<double>>();
            for (int index = freq; index < (freq * 2); index++)
            {
                // Set the Time depending on where we are in the index
                // Larger the index then further out the prediction
                upper.Add(new TimeSeriesItem<double>() { Time = maxDateTime.AddMilliseconds(sampleInterval.TotalMilliseconds * (index + 1)), Value = timeSeriesPrediction[index] });
            }
            predictedValues.Add("Upper", upper);

            var lower = new List<TimeSeriesItem<double>>();
            for (int index = (freq * 2); index < (freq * 3); index++)
            {
                // Set the Time depending on where we are in the index
                // Larger the index then further out the prediction
                lower.Add(new TimeSeriesItem<double>() { Time = maxDateTime.AddMilliseconds(sampleInterval.TotalMilliseconds * (index + 1)), Value = timeSeriesPrediction[index] });
            }
            predictedValues.Add("Lower", lower);


            return predictedValues;
        }
    }
}
