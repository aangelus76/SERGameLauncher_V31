using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace SERGamesLauncher_V31
{
    public partial class SilentModeScheduleControl : UserControl
    {
        // Configuration du planning
        private SilentModeSchedule schedule;

        // Dictionnaires pour mapper les contrôles aux jours
        private Dictionary<string, CheckBox> morningCheckBoxes;
        private Dictionary<string, CheckBox> afternoonCheckBoxes;
        private Dictionary<string, TextBox> morningStartBoxes;
        private Dictionary<string, TextBox> morningEndBoxes;
        private Dictionary<string, TextBox> afternoonStartBoxes;
        private Dictionary<string, TextBox> afternoonEndBoxes;

        // Flag pour éviter les sauvegardes en boucle lors du chargement
        private bool isLoading = true;

        public SilentModeScheduleControl()
        {
            InitializeComponent();
            InitializeControlMappings();
            AttachTimeBoxEvents(); // Ajouter les événements GotFocus
            LoadSchedule();
            isLoading = false;
        }

        // Dictionnaire pour stocker les valeurs originales des champs
        private Dictionary<TextBox, string> originalValues = new Dictionary<TextBox, string>();

        /// <summary>
        /// Attache les événements aux TextBox d'heure
        /// </summary>
        private void AttachTimeBoxEvents()
        {
            // Attacher les événements à tous les TextBox
            foreach (var textBox in morningStartBoxes.Values)
            {
                textBox.GotFocus += TimeBox_GotFocus;
                textBox.LostFocus += TimeBox_LostFocus;
            }
            foreach (var textBox in morningEndBoxes.Values)
            {
                textBox.GotFocus += TimeBox_GotFocus;
                textBox.LostFocus += TimeBox_LostFocus;
            }
            foreach (var textBox in afternoonStartBoxes.Values)
            {
                textBox.GotFocus += TimeBox_GotFocus;
                textBox.LostFocus += TimeBox_LostFocus;
            }
            foreach (var textBox in afternoonEndBoxes.Values)
            {
                textBox.GotFocus += TimeBox_GotFocus;
                textBox.LostFocus += TimeBox_LostFocus;
            }
        }

        /// <summary>
        /// Vide le champ et sauvegarde l'ancienne valeur au focus
        /// </summary>
        private void TimeBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Sauvegarder la valeur actuelle
                originalValues[textBox] = textBox.Text;

                // Vider le champ
                textBox.Text = "";
            }
        }

        /// <summary>
        /// Restaure l'ancienne valeur si vide, sinon valide
        /// </summary>
        private void TimeBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Si le champ est vide, restaurer l'ancienne valeur
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    if (originalValues.ContainsKey(textBox))
                    {
                        textBox.Text = originalValues[textBox];
                    }
                }
                else
                {
                    // Valider l'heure saisie
                    TimeLostFocus(sender, e);
                }
            }
        }

        /// <summary>
        /// Initialise les mappings entre les contrôles et les jours
        /// </summary>
        private void InitializeControlMappings()
        {
            morningCheckBoxes = new Dictionary<string, CheckBox>
            {
                { "Monday", chkMondayMorning },
                { "Tuesday", chkTuesdayMorning },
                { "Wednesday", chkWednesdayMorning },
                { "Thursday", chkThursdayMorning },
                { "Friday", chkFridayMorning },
                { "Saturday", chkSaturdayMorning },
                { "Sunday", chkSundayMorning }
            };

            afternoonCheckBoxes = new Dictionary<string, CheckBox>
            {
                { "Monday", chkMondayAfternoon },
                { "Tuesday", chkTuesdayAfternoon },
                { "Wednesday", chkWednesdayAfternoon },
                { "Thursday", chkThursdayAfternoon },
                { "Friday", chkFridayAfternoon },
                { "Saturday", chkSaturdayAfternoon },
                { "Sunday", chkSundayAfternoon }
            };

            morningStartBoxes = new Dictionary<string, TextBox>
            {
                { "Monday", txtMondayMorningStart },
                { "Tuesday", txtTuesdayMorningStart },
                { "Wednesday", txtWednesdayMorningStart },
                { "Thursday", txtThursdayMorningStart },
                { "Friday", txtFridayMorningStart },
                { "Saturday", txtSaturdayMorningStart },
                { "Sunday", txtSundayMorningStart }
            };

            morningEndBoxes = new Dictionary<string, TextBox>
            {
                { "Monday", txtMondayMorningEnd },
                { "Tuesday", txtTuesdayMorningEnd },
                { "Wednesday", txtWednesdayMorningEnd },
                { "Thursday", txtThursdayMorningEnd },
                { "Friday", txtFridayMorningEnd },
                { "Saturday", txtSaturdayMorningEnd },
                { "Sunday", txtSundayMorningEnd }
            };

            afternoonStartBoxes = new Dictionary<string, TextBox>
            {
                { "Monday", txtMondayAfternoonStart },
                { "Tuesday", txtTuesdayAfternoonStart },
                { "Wednesday", txtWednesdayAfternoonStart },
                { "Thursday", txtThursdayAfternoonStart },
                { "Friday", txtFridayAfternoonStart },
                { "Saturday", txtSaturdayAfternoonStart },
                { "Sunday", txtSundayAfternoonStart }
            };

            afternoonEndBoxes = new Dictionary<string, TextBox>
            {
                { "Monday", txtMondayAfternoonEnd },
                { "Tuesday", txtTuesdayAfternoonEnd },
                { "Wednesday", txtWednesdayAfternoonEnd },
                { "Thursday", txtThursdayAfternoonEnd },
                { "Friday", txtFridayAfternoonEnd },
                { "Saturday", txtSaturdayAfternoonEnd },
                { "Sunday", txtSundayAfternoonEnd }
            };
        }

        /// <summary>
        /// Charge la configuration depuis le service
        /// </summary>
        private void LoadSchedule()
        {
            schedule = SilentModeScheduleService.LoadSchedule();
            UpdateControlsFromSchedule();
        }

        /// <summary>
        /// Met à jour les contrôles selon la configuration chargée
        /// </summary>
        private void UpdateControlsFromSchedule()
        {
            var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            var dayOfWeeks = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                                   DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };

            for (int i = 0; i < days.Length; i++)
            {
                string day = days[i];
                DaySchedule daySchedule = schedule.GetDaySchedule(dayOfWeeks[i]);

                // Mettre à jour les cases à cocher
                morningCheckBoxes[day].IsChecked = daySchedule.Morning.Enabled;
                afternoonCheckBoxes[day].IsChecked = daySchedule.Afternoon.Enabled;

                // Mettre à jour les heures
                morningStartBoxes[day].Text = daySchedule.Morning.Start;
                morningEndBoxes[day].Text = daySchedule.Morning.End;
                afternoonStartBoxes[day].Text = daySchedule.Afternoon.Start;
                afternoonEndBoxes[day].Text = daySchedule.Afternoon.End;
            }
        }

        /// <summary>
        /// Sauvegarde la configuration
        /// </summary>
        private void SaveSchedule()
        {
            if (isLoading) return;

            bool success = SilentModeScheduleService.SaveSchedule(schedule);

            if (!success)
            {
                CustomMessageBox.Show(Window.GetWindow(this),
                    "Une erreur est survenue lors de la sauvegarde du planning silencieux.",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Gestionnaire pour les changements de cases à cocher
        /// </summary>
        private void TimeSlot_Changed(object sender, RoutedEventArgs e)
        {
            if (isLoading) return;

            if (sender is CheckBox checkBox)
            {
                // Mettre à jour la configuration
                UpdateScheduleFromControls();
                SaveSchedule();
            }
        }

        /// <summary>
        /// Gestionnaire pour les changements d'heures (validation uniquement à la perte de focus)
        /// </summary>
        private void TimeChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (isLoading) return;

            // Seulement sauvegarder, pas de validation en temps réel
            UpdateScheduleFromControls();
            SaveSchedule();
        }

        /// <summary>
        /// Gestionnaire pour la validation des heures quand on quitte le champ
        /// </summary>
        private void TimeLostFocus(object sender, RoutedEventArgs e)
        {
            if (isLoading) return;

            if (sender is TextBox textBox && textBox.Tag is string tag)
            {
                string[] parts = tag.Split('|');
                if (parts.Length == 2)
                {
                    string dayPeriod = parts[0]; // Ex: "MondayMorning"
                    string timeType = parts[1];  // "Start" ou "End"

                    // Déterminer si c'est matin ou après-midi
                    bool isMorning = dayPeriod.Contains("Morning");

                    // Obtenir l'autre TextBox de la même plage
                    TextBox startBox = null, endBox = null;
                    string day = dayPeriod.Replace("Morning", "").Replace("Afternoon", "");

                    if (isMorning)
                    {
                        morningStartBoxes.TryGetValue(day, out startBox);
                        morningEndBoxes.TryGetValue(day, out endBox);
                    }
                    else
                    {
                        afternoonStartBoxes.TryGetValue(day, out startBox);
                        afternoonEndBoxes.TryGetValue(day, out endBox);
                    }

                    if (startBox != null && endBox != null)
                    {
                        // Valider la plage complète
                        var (validStart, validEnd) = SilentModeScheduleService.ValidateTimeRange(
                            startBox.Text, endBox.Text, isMorning);

                        // Mettre à jour les deux champs si nécessaire
                        if (startBox.Text != validStart)
                            startBox.Text = validStart;
                        if (endBox.Text != validEnd)
                            endBox.Text = validEnd;

                        // Mettre à jour la configuration après correction
                        UpdateScheduleFromControls();
                        SaveSchedule();
                    }
                }
            }
        }

        /// <summary>
        /// Valide et corrige une heure selon les contraintes
        /// </summary>
        private string ValidateTime(string time, string dayPeriod, string timeType)
        {
            bool isMorning = dayPeriod.Contains("Morning");
            bool isEndTime = timeType == "End";

            if (isMorning)
            {
                return SilentModeScheduleService.ValidateMorningTime(time, isEndTime);
            }
            else
            {
                return SilentModeScheduleService.ValidateAfternoonTime(time, isEndTime);
            }
        }

        /// <summary>
        /// Met à jour la configuration depuis les contrôles
        /// </summary>
        private void UpdateScheduleFromControls()
        {
            var days = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            var dayOfWeeks = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                                   DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };

            for (int i = 0; i < days.Length; i++)
            {
                string day = days[i];
                DaySchedule daySchedule = schedule.GetDaySchedule(dayOfWeeks[i]);

                // Mettre à jour depuis les contrôles
                daySchedule.Morning.Enabled = morningCheckBoxes[day].IsChecked ?? false;
                daySchedule.Afternoon.Enabled = afternoonCheckBoxes[day].IsChecked ?? false;

                daySchedule.Morning.Start = morningStartBoxes[day].Text;
                daySchedule.Morning.End = morningEndBoxes[day].Text;
                daySchedule.Afternoon.Start = afternoonStartBoxes[day].Text;
                daySchedule.Afternoon.End = afternoonEndBoxes[day].Text;
            }
        }
    }
}