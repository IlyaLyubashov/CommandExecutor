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
        private Dictionary<string, TimerSettings> workingTimers = new Dictionary<string, TimerSettings>();
        private Dictionary<string, TimerSettings> stoppedTimers = new Dictionary<string, TimerSettings>();

        public AppTimer() : base() { }


        public override string Info => throw new NotImplementedException();


        public override string Name => "timer";

        private string StringWorkingTimers => string.Join(", ", workingTimers.Keys);

        private string StringStoppedTimers => string.Join(", ", stoppedTimers.Keys);

        protected override void InitializeOptions()
        {
            base.InitializeOptions();
            AddFunctionArguments(1, 2);
            AddOption("-m", "--measure", 1, new string[] { "minutes" });
            AddOption("-mp", "--melody-path", 1, new string[] { @"C:\Windows\Media\Alarm04.wav" });
            AddOption("-r", "--repeat", 1, new string[] { "1" });
            AddOption("-ns", "--no-sound");
            AddOption("-n", "--name", 1);

            writeToWorker = new ActionOption("-w", "--write-to-worker", 1, TimerActions.WriteToDbWorker);
            writeToWorker.SetDefaultArguments(new string[] { TimerActions.DB_CONNECTION_STRING });
            AddPostFuncOption(writeToWorker);
        }


        //void WriteToWorkTracker(int timeInHours)
        //{

        //}

        void TimerActivity(TimerSettings settings)
        {
            if (settings.TimerName != null && stoppedTimers.ContainsKey(settings.TimerName))
            {
                stoppedTimers.Remove(settings.TimerName);
                workingTimers.Add(settings.TimerName, settings);
                SentOut($"Timer \"{settings.TimerName}\" continued at {DateTime.Now.ToShortTimeString()} for {settings.TimeToSleep}.");
            }
            else
            {
                if (!AppendTimerToDict(settings))
                    return;

                SentOut($"Timer with name \"{settings.TimerName}\" started for {settings.TimeToSleep.TotalMinutes} minutes at {settings.TimerStarted.ToShortTimeString()}.");
            }
            
            var task = Task.Delay(settings.TimeToSleep, settings.CancelTokenSource.Token);
            try
            {
                task.Wait();
            }
            catch (AggregateException ex)
            {
                var timerWorked = DateTime.Now - settings.TimerStarted;
                writeToWorker.AdditionalArgumentsForNextInvoke = new object[] { timerWorked };               
                return;
            }


            var rep = settings.RepeatTimes;
            if (!settings.IsNoSound)
            {
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
            }
            SentOut($"Timer with name \"{settings.TimerName}\" ended at {DateTime.Now.ToShortTimeString()}.");
            writeToWorker.AdditionalArgumentsForNextInvoke = new object[] { settings.TimeToSleep };

            workingTimers.Remove(settings.TimerName);
        }


        protected override void Invoke(IEnumerable<string> funcArguments, IDictionary<string, Option> fullNameToOption)
        {
            var argsCount = funcArguments.Count();
            var firstArg = funcArguments.First();
            if (int.TryParse(firstArg, out int mean))
            {
                TimerActivity(new TimerSettings(funcArguments, fullNameToOption));
                return;
            }
            else if (firstArg == "shutdown" || firstArg == "stop")
            {
                ShutdownStopController(firstArg, funcArguments.Skip(1));
                return;
            }
            else if (firstArg == "continue")
            {
                ContinueController(funcArguments.Skip(1));
                return;
            }
        }

        private void NotifyShutdownStop(bool isStop, string timerName, int totalMinutes)
            => SentOut($"Timer with name \"{timerName}\" {(isStop ? "stopped" : "shutdowned")} at {DateTime.Now.ToShortTimeString()}. It worked for {totalMinutes} minutes.");

        private void ShutdownStopController(string commandName,IEnumerable<string> funcArgumentsExeptControllerName)
        {
            void TimerShutDownActivity(TimerSettings timer)
            {
                timer.CancelTokenSource.Cancel();
                var timerWorked = DateTime.Now - timer.TimerStarted;
                if (commandName == "stop")
                {
                    stoppedTimers.Add(timer.TimerName, timer);
                    timer.MinusTimeToSleep(timerWorked);
                }
                workingTimers.Remove(timer.TimerName);
                NotifyShutdownStop(commandName == "stop", timer.TimerName, (int)timerWorked.TotalMinutes);
            }

            var funcArgsList = funcArgumentsExeptControllerName.ToList();
            if (workingTimers.Count == 0)
            {
                SentOut("There is no working timers to shutdown.");
                return;
            }
           

            if (funcArgsList.Count == 0)
            {
                if (workingTimers.Count > 1)
                {
                    SentOut($"There is more then one timer. Specify name to shutdown.(working timers: {StringWorkingTimers})");
                    return;
                }
                var timerFirst = workingTimers.First().Value;
                TimerShutDownActivity(timerFirst);
                return;
            }


            if (!IsTimerExistsAndNotificateIfNot(funcArgsList[0]))
                return;

            var timer = workingTimers[funcArgsList[0]];
            TimerShutDownActivity(timer);
        }



        private void ContinueController(IEnumerable<string> funcArgumentsExeptControllerName)
        {
            void ContinueTimerActivity(TimerSettings timer)
            {
                var nameOpt = timer.Options.FirstOrDefault(o => o.FullName == "--name");
                nameOpt?.SetArgument(timer.TimerName);
                nameOpt?.MakeOptionSet();
                base.Invoke(timer.Options);                
            }

            var funcArgsList = funcArgumentsExeptControllerName.ToList();
            if (stoppedTimers.Count == 0)
            {
                SentOut("There is no stopped timers to continue.");
                return;
            }

            if (funcArgsList.Count == 0)
            {
                if (stoppedTimers.Count > 1)
                {
                    SentOut($"There is more then one timer. Specify name to shutdown.(working timers: {StringStoppedTimers})");
                    return;
                }
                var timerFirst = stoppedTimers.First().Value;
                timerFirst.ResetTimerStartToNow();
                ContinueTimerActivity(timerFirst);
                return;
            }


            if (!IsTimerExistsAndNotificateIfNot(funcArgsList[0]))
                return;

            var timer = workingTimers[funcArgsList[0]];
            ContinueTimerActivity(timer);
        }


        private bool IsTimerExistsAndNotificateIfNot(string name)
        {
            if (workingTimers.Keys.Contains(name))
                return false;

            SentOut($"Timer with such name {name} doesn't exist.(existing timers: {StringWorkingTimers})");
            return true;
        }

        private bool AppendTimerToDict(TimerSettings settings)
        {
            var allTimers = workingTimers.Keys.ToList();
            allTimers.AddRange(stoppedTimers.Keys.ToList());
            //мб, не экспешн, а самому обработать как-то
            if (settings.TimerName != null && allTimers.Contains(settings.TimerName))
            {
                SentOut($"Timer with such name has already been started!( existing timers: {StringWorkingTimers})");
                return false;
            }

            if (string.IsNullOrEmpty(settings.TimerName))
            {
                var potentialName = "timer" + workingTimers.Count;
                var incr = 1;
                while (allTimers.Contains(potentialName))
                    potentialName = "timer" +( workingTimers.Count + incr);
                settings.TimerName = potentialName;
            }
            workingTimers.Add(settings.TimerName, settings);
            return true;
        }
    }

    class TimerSettings : MappedCommandSettings
    {
        [OptionMapProp("repeat")]
        public int RepeatTimes { get; private set; }

        public string MelodyPath { get; private set; }

        public TimeSpan TimeToSleep { get; private set; }

        [OptionMapProp("no-sound")]
        public bool IsNoSound { get; set; } // можно добавить в конструктор if_reverse для маппинга бул значений и вернуть проперти IsSound

        [OptionMapProp("name")]
        public string TimerName { get; set; }

        public CancellationTokenSource CancelTokenSource { get; } = new CancellationTokenSource();

        public DateTime TimerStarted { get; private set; } = DateTime.Now;

        public IEnumerable<Option> Options { get; }


        //TODO: сделать настраиваемый маппинг на атрибутах
        public TimerSettings(IEnumerable<string> funcArguments, IDictionary<string, Option> fullNameToOption) :base(fullNameToOption.Values)
        {
            if (!int.TryParse(funcArguments.First(), out int meanSleep))
                throw new ArgumentException("Value can be only integer.");
            TimeToSleep = ChooseTime(meanSleep, fullNameToOption["--measure"].GetArguments().First());

            //if (!int.TryParse(fullNameToOption["--repeat"].GetArguments().First(), out int meanRep))
            //    throw new ArgumentException("Value can be only integer.");
            //RepeatTimes = meanRep;

            //var path = fullNameToOption["--melody-path"].GetArguments().First();
            //if (File.Exists(path))
            //    MelodyPath = path;

            //if (fullNameToOption["--no-sound"].IsOptionSet)
            //    IsSound = false;

            //if (fullNameToOption["--name"].IsOptionSet)
            //    TimerName = fullNameToOption["--name"].GetArguments().First();

            Options = fullNameToOption.Values;
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


        public void ResetTimerStartToNow()
        {
            TimerStarted = DateTime.Now;
        }


        public void MinusTimeToSleep(TimeSpan minusTime)
            => TimeToSleep -= minusTime;

    }
}
