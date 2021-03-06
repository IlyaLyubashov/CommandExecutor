﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Globalization;
using System.Data.SqlClient;

namespace ConsoleCommander.Commands
{
    /// <summary>
    /// Костыли связаны с тем, что изначально заполнялся по неким негласным правилам текстовый файл вручную.
    /// Данная функциональнасть нужна для его поддержки.
    /// </summary>
    public static class TimerActions
    {
        //TODO: вынести в конфиг
        public const string PATH_TO_TXT_WORKER = @"C:\Users\днс\Desktop\trackers\work tracker.txt";
        public const string DB_CONNECTION_STRING = @"Data Source=DESKTOP-32I0HVH\SQLEXPRESS;Database=MyDataBase;Integrated Security=True";
        public const int AMOUNT_HOURS_AFTER_0000_NOT_NEXT_DAY = 5;


        public static void WriteToTxtWorker(IEnumerable<string> arguments, IEnumerable<object> additions)
        {
            var workerPath = arguments.First();
            var workedTime = (TimeSpan)additions.First();

            string txt;
            WorkTrackerEntity lastEntity;

            using (var fileReader = new StreamReader(workerPath))
            {
                txt = fileReader.ReadToEnd();
                lastEntity = WorkTrackerEntityInterctions.ParseLine(txt.Split('\n').Last());
            }


            using (var fileWriter = new StreamWriter(workerPath, false))
            {
                var trulyCompareDate = DateTime.Now.Hour < AMOUNT_HOURS_AFTER_0000_NOT_NEXT_DAY ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day - 1) : DateTime.Today; 
                if ((lastEntity.Date.Year, lastEntity.Date.Month, lastEntity.Date.Day)
                    .CompareTo((trulyCompareDate.Year, trulyCompareDate.Month, trulyCompareDate.Day)) == 0)
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

        public static void WriteToDbWorker(IEnumerable<string> arguments, IEnumerable<object> additions)
        {
            void InsertToday(SqlCommand cmd, string timeToWrite)
            {
                cmd.CommandText = "insert into WorkTracker(date,real_time) values(@date,@time)";
                cmd.Parameters.AddWithValue("@date", DateTime.Now);
                cmd.Parameters.AddWithValue("@time", timeToWrite);
                cmd.ExecuteNonQuery();
            }


            var timeToWriteInHours = ((TimeSpan)additions.First()).TotalHours.ToString("0.00", CultureInfo.GetCultureInfo("en-US"));
            using (var con = new SqlConnection(arguments.First()))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = "select date,real_time from WorkTracker order by date desc";
                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    var lastDay = reader.GetDateTime(0);
                    reader.Close();
                    var trulyCompareDate = DateTime.Now.Hour < AMOUNT_HOURS_AFTER_0000_NOT_NEXT_DAY ?
                        new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day - 1) : DateTime.Today;
                    if ((lastDay.Year, lastDay.Month, lastDay.Day)
                        .CompareTo((trulyCompareDate.Year, trulyCompareDate.Month, trulyCompareDate.Day)) == 0)
                    {
                        cmd.CommandText = "update WorkTracker set real_time = real_time + @time where date = @today";
                        cmd.Parameters.AddWithValue("@time", timeToWriteInHours);
                        cmd.Parameters.AddWithValue("@today", trulyCompareDate);
                        cmd.ExecuteNonQuery();
                    }
                    else
                        InsertToday(cmd, timeToWriteInHours);
                }
                else
                {
                    reader.Close();
                    InsertToday(cmd, timeToWriteInHours);
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
            return $"{Date.Day}.{Date.Month}.{Date.Year}\t{(Planned == null ? WorkTrackerEntityInterctions.NULL_SIGN : Planned.ToString())}\t" +
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
                EstimatedTime = fields[2] == null ? null : (TimeSpan?)TimeSpan.FromHours(double.Parse(fields[2], CultureInfo.InvariantCulture)),
                Done = fields[3] == null ? null : (int?)int.Parse(fields[1]),
                RealTime = fields[4] == null ? null : (TimeSpan?)TimeSpan.FromHours(double.Parse(fields[4], CultureInfo.InvariantCulture))
            };
            return entity;
        }

        public static IEnumerable<WorkTrackerEntity> ParseText(string text)
            => text.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line => ParseLine(line));
    }

}

