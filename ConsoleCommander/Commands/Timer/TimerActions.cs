using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace ConsoleCommander.Commands
{
    /// <summary>
    /// Костыли связаны с тем, что изначально заполнялся по неким негласным правилам текстовый файл вручную.
    /// Данная функциональнасть нужна для его поддержки.
    /// </summary>
    public static class TimerActions
    {
        public const string PATH_TO_TXT_WORKER = @"C:\Users\днс\Desktop\trackers\work tracker.txt";


        public static void WriteToWorker(IEnumerable<string> arguments, IEnumerable<object> additions)
        {
            var workerPath = arguments.First();
            var workedTime = (TimeSpan)additions.First();

            string txt;
            WorkTrackerEntity lastEntity;

            using (var fileReader = new StreamReader(workerPath))
            {
                txt = fileReader.ReadToEnd();
                lastEntity = WorkTrackerEntityInterctions.ParseLine(txt.Split('n').Last());
            }


            using (var fileWriter = new StreamWriter(workerPath, false))
            {
                if ((lastEntity.Date.Year, lastEntity.Date.Month, lastEntity.Date.Date)
                    .CompareTo((DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Date)) == 0)
                {
                    lastEntity.RealTime += workedTime;
                    var txtSplit = txt.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    var txtModifiedLastString = string.Join('\n', txtSplit.Take(txtSplit.Length - 1));
                    txtModifiedLastString += "\n" + lastEntity.ToString();
                    fileWriter.Write(txtModifiedLastString);
                }
                else
                {
                    var entity = new WorkTrackerEntity()
                    {
                        Date = DateTime.Now,
                        RealTime = workedTime
                    };
                    fileWriter.Write(txt + "\n" + entity.ToString());
                }
            }
        }
    }


    public class WorkTrackerEntity
    {
        public DateTime Date { get; set; }
        public int? Planned { get; set; }
        public TimeSpan? EstimatedTime { get; set; }
        public int? Done { get; set; }
        public TimeSpan? RealTime { get; set; }

        public override string ToString()
        {
            return $"{Date.Date}.{Date.Month}.{Date.Year}\t{(Planned == null ? WorkTrackerEntityInterctions.NULL_SIGN : Planned.ToString())}\t" +
                $"{(EstimatedTime == null ? WorkTrackerEntityInterctions.NULL_SIGN : EstimatedTime.Value.TotalHours.ToString())}\t" +
                $"{(Done == null ? WorkTrackerEntityInterctions.NULL_SIGN : Done.ToString())}\t" +
                $"{(RealTime == null ? WorkTrackerEntityInterctions.NULL_SIGN : RealTime.Value.TotalHours.ToString())}\n";
        }
    }

    /// <summary>
    ///Такая реализация в связи с особенностями строения моего личного блокнота 
    /// </summary>
    public static class WorkTrackerEntityInterctions
    {
        public const string NULL_SIGN = "undef";
        public static WorkTrackerEntity ParseLine(string line)
        {
            var fields = line.Split('\t', StringSplitOptions.RemoveEmptyEntries).Select(str =>
            {
                if (str == NULL_SIGN)
                    return null;
                return str.Trim();
            }).ToArray();
            var date = fields[0].Split('.').Select(d => int.Parse(d)).ToArray();
            var entity = new WorkTrackerEntity()
            {
                Date = new DateTime(date[2], date[1], date[0]),
                Planned = fields[1] == null ? null : (int?)int.Parse(fields[1]),
                EstimatedTime = fields[2] == null ? null : (TimeSpan?)TimeSpan.FromHours(double.Parse(fields[2])),
                Done = fields[3] == null ? null : (int?)int.Parse(fields[1]),
                RealTime = fields[4] == null ? null : (TimeSpan?)TimeSpan.FromHours(double.Parse(fields[4]))
            };
            return entity;
        }

        public static IEnumerable<WorkTrackerEntity> ParseText(string text)
            => text.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line => ParseLine(line));
    }

}

