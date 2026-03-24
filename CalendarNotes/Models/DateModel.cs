using System;
using CalendarNotes.Helpers;

namespace CalendarNotes.Models
{
    public class DateModel : ObservableObject
    {
        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set
            {
                if (SetProperty(ref _date, value))
                {
                    OnPropertyChanged(nameof(DayNumber));
                }
            }
        }

        private bool _isCurrentMonth;
        public bool IsCurrentMonth
        {
            get => _isCurrentMonth;
            set => SetProperty(ref _isCurrentMonth, value);
        }

        public string DayNumber => Date.Day.ToString();

        private bool _hasNotes;
        public bool HasNotes
        {
            get => _hasNotes;
            set => SetProperty(ref _hasNotes, value);
        }
    }
}