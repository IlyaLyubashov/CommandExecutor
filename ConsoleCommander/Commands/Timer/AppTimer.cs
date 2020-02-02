using System;
using System.Threading.Tasks;
using ConsoleCommander.Interfaces;
using System.Threading;
using NAudio.Wave;
using ConsoleCommander.Utils;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ConsoleCommander.Options;


namespace ConsoleCommander.Commands
{
    //опция для смены мелодии
    //опция для перманентной смены настроек
    //опция для смены повторения
    //при создании читать настройки из json
    //опция для единици измерения
    public class AppTimer : CommandBase
    {
        private ActionOption writeToWorker;


        public AppTimer() : base() { }


        public override string Info => throw new NotImplementedException();


        public override string Name => "timer";


        protected override void InitializeOptions()
        {
            base.InitializeOptions();
            AddFunctionArguments(1);
            AddOption("-m", "--measure", 1, new string[] { "minutes" });
            AddOption("-mp", "--melody-path", 1, new string[] { @"C:\Windows\Media\Alarm04.wav" });
            AddOption("-r", "--repeat", 1, new string[] { "1" });

            writeToWorker = new ActionOption("-w", "--write-to-worker", 1, TimerActions.WriteToDbWorker);
            writeToWorker.SetDefaultArguments(new string[] { TimerActions.DB_CONNECTION_STRING });
            AddPostFuncOption(writeToWorker);
        }


        //void WriteToWorkTracker(int timeInHours)
        //{

        //}

        void TimerActivity(TimerSettings settings)
        {
            SentOut($"Timer started for {settings.TimeToSleep.TotalMinutes} minutes.");
            Thread.Sleep(settings.TimeToSleep);
            var rep = settings.RepeatTimes;
            while (rep-- != 0)
            {
                using (var audioFile = new AudioFileReader(settings.MelodyPath))
                {
                    using (var device = new WaveOutEvent())
                    {
                        device.Init(audioFile);
                        device.Play();
                        Thread.Sleep(audioFile.TotalTime);
                    }
                }
            }
            writeToWorker.AdditionalArgumentsForNextInvoke = new object[] { settings.TimeToSleep };
        }


        protected override void Invoke(IEnumerable<string> funcArguments, IDictionary<string, Option> fullNameToOption)
            => TimerActivity(new TimerSettings(funcArguments, fullNameToOption));
        //Task.Run(() => TimerActivity(new TimerSettings(funcArguments, fullNameToOption)));
    }

    class TimerSettings
    {
        public int RepeatTimes { get; private set; }

        public string MelodyPath { get; private set; }

        public TimeSpan TimeToSleep { get; private set; }

        public TimerSettings(IEnumerable<string> funcArguments, IDictionary<string, Option> fullNameToOption)
        {
            if (!int.TryParse(funcArguments.First(), out int meanSleep))
                throw new ArgumentException("Value can be only integer.");
            TimeToSleep = ChooseTime(meanSleep, fullNameToOption["--measure"].GetArguments().First());

            if (!int.TryParse(fullNameToOption["--repeat"].GetArguments().First(), out int meanRep))
                throw new ArgumentException("Value can be only integer.");
            RepeatTimes = meanRep;

            var path = fullNameToOption["--melody-path"].GetArguments().First();
            if (File.Exists(path))
                MelodyPath = path;
        }

        private TimeSpan ChooseTime(int mean, string measure)
        {
            switch (measure)
            {
                case ("minutes"):
                    return TimeSpan.FromMinutes(mean);
                case ("seconds"):
                    return TimeSpan.FromSeconds(mean);
                case ("milliseconds"):
                    return TimeSpan.FromMilliseconds(mean);
                default:
                    throw new ArgumentException("Such measurement for timer doesn't exist. Make your choice from this 'minutes', 'seconds', 'milliseconds'");
            }
        }
    }
}
