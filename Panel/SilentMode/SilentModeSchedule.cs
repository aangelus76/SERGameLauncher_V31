using System;
using System.ComponentModel;

namespace SERGamesLauncher_V31
{
    /// <summary>
    /// Représente une plage horaire pour le mode silencieux
    /// </summary>
    public class TimeSlot : INotifyPropertyChanged
    {
        private bool _enabled = false;
        private string _start = "08:00";
        private string _end = "12:00";

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    OnPropertyChanged(nameof(Enabled));
                }
            }
        }

        public string Start
        {
            get => _start;
            set
            {
                if (_start != value)
                {
                    _start = value;
                    OnPropertyChanged(nameof(Start));
                }
            }
        }

        public string End
        {
            get => _end;
            set
            {
                if (_end != value)
                {
                    _end = value;
                    OnPropertyChanged(nameof(End));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Représente la configuration d'une journée avec matin et après-midi
    /// </summary>
    public class DaySchedule : INotifyPropertyChanged
    {
        private TimeSlot _morning;
        private TimeSlot _afternoon;

        public TimeSlot Morning
        {
            get => _morning;
            set
            {
                if (_morning != value)
                {
                    _morning = value;
                    OnPropertyChanged(nameof(Morning));
                }
            }
        }

        public TimeSlot Afternoon
        {
            get => _afternoon;
            set
            {
                if (_afternoon != value)
                {
                    _afternoon = value;
                    OnPropertyChanged(nameof(Afternoon));
                }
            }
        }

        public DaySchedule()
        {
            Morning = new TimeSlot { Start = "08:00", End = "13:00" };
            Afternoon = new TimeSlot { Start = "13:00", End = "00:00" };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Représente la configuration complète du planning silencieux
    /// </summary>
    public class SilentModeSchedule : INotifyPropertyChanged
    {
        public DaySchedule Monday { get; set; } = new DaySchedule();
        public DaySchedule Tuesday { get; set; } = new DaySchedule();
        public DaySchedule Wednesday { get; set; } = new DaySchedule();
        public DaySchedule Thursday { get; set; } = new DaySchedule();
        public DaySchedule Friday { get; set; } = new DaySchedule();
        public DaySchedule Saturday { get; set; } = new DaySchedule();
        public DaySchedule Sunday { get; set; } = new DaySchedule();

        /// <summary>
        /// Obtient la configuration pour un jour spécifique
        /// </summary>
        public DaySchedule GetDaySchedule(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return Monday;
                case DayOfWeek.Tuesday:
                    return Tuesday;
                case DayOfWeek.Wednesday:
                    return Wednesday;
                case DayOfWeek.Thursday:
                    return Thursday;
                case DayOfWeek.Friday:
                    return Friday;
                case DayOfWeek.Saturday:
                    return Saturday;
                case DayOfWeek.Sunday:
                    return Sunday;
                default:
                    return Monday;
            }
        }

        /// <summary>
        /// Vérifie si l'heure actuelle est dans une plage active
        /// </summary>
        public bool IsInSilentMode()
        {
            DateTime now = DateTime.Now;
            DaySchedule today = GetDaySchedule(now.DayOfWeek);
            string currentTime = now.ToString("HH:mm");

            // Vérifier plage matin
            if (today.Morning.Enabled && IsTimeInRange(currentTime, today.Morning.Start, today.Morning.End))
            {
                return true;
            }

            // Vérifier plage après-midi (gérer le cas 00:00)
            if (today.Afternoon.Enabled)
            {
                if (today.Afternoon.End == "00:00")
                {
                    // Plage jusqu'à minuit
                    return IsTimeInRange(currentTime, today.Afternoon.Start, "23:59");
                }
                else
                {
                    return IsTimeInRange(currentTime, today.Afternoon.Start, today.Afternoon.End);
                }
            }

            return false;
        }

        /// <summary>
        /// Vérifie si une heure est dans une plage donnée
        /// </summary>
        private bool IsTimeInRange(string currentTime, string startTime, string endTime)
        {
            if (TimeSpan.TryParse(currentTime, out TimeSpan current) &&
                TimeSpan.TryParse(startTime, out TimeSpan start) &&
                TimeSpan.TryParse(endTime, out TimeSpan end))
            {
                return current >= start && current <= end;
            }
            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}