using System;
using CalendarNotes.Helpers;

namespace CalendarNotes.Models
{
    public class Note : ObservableObject
    {
        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        private string _text = string.Empty;
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }
    }
}