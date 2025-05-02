using System;

namespace SERGamesLauncher_V31
{
    public class ProcessRestriction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ProcessName { get; set; } = string.Empty;
        public int MinimumAge { get; set; } = 0;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public ProcessRestriction() { }

        public ProcessRestriction(string processName, int minimumAge, string description = "", bool isActive = true)
        {
            ProcessName = processName;
            MinimumAge = minimumAge;
            Description = description;
            IsActive = isActive;
        }
    }
}