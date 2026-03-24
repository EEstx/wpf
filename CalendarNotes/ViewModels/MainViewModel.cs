using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CalendarNotes.Helpers;
using CalendarNotes.Models;

namespace CalendarNotes.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        public ObservableCollection<DateModel> CalendarDays { get; } = new();
        public ObservableCollection<Note> AllNotes { get; } = new();

        private DateTime _currentMonth;
        public string MonthYear => _currentMonth.ToString("MMMM yyyy");

        public ICommand NextMonthCommand { get; }
        public ICommand PrevMonthCommand { get; }
        public ICommand DayClickCommand { get; }

        public Action<DateTime>? RequestOpenNotes { get; set; }

        public MainViewModel()
        {
            _currentMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            
            NextMonthCommand = new RelayCommand(o => { _currentMonth = _currentMonth.AddMonths(1); GenerateCalendar(); });
            PrevMonthCommand = new RelayCommand(o => { _currentMonth = _currentMonth.AddMonths(-1); GenerateCalendar(); });
            DayClickCommand = new RelayCommand(OnDayClick);

            GenerateCalendar();
        }

        private void GenerateCalendar()
        {
            CalendarDays.Clear();
            OnPropertyChanged(nameof(MonthYear));

            var firstDay = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
            
            // Определяем день недели для первого числа месяца
            // DayOfWeek.Sunday = 0, поэтому сдвигаем так, чтобы ПН(1) был первым
            int startOffset = (int)firstDay.DayOfWeek - 1;
            if (startOffset < 0) startOffset = 6; // Если воскресенье, сдвиг 6 дней назад

            var startDate = firstDay.AddDays(-startOffset);
            
            // Календарь обычно имеет 6 строк по 7 дней
            for (int i = 0; i < 42; i++)
            {
                var date = startDate.AddDays(i);
                CalendarDays.Add(new DateModel
                {
                    Date = date,
                    IsCurrentMonth = date.Month == _currentMonth.Month,
                    HasNotes = AllNotes.Any(n => n.Date.Date == date.Date)
                });
            }
        }

        private void OnDayClick(object? obj)
        {
            if (obj is DateModel day)
            {
                RequestOpenNotes?.Invoke(day.Date);
            }
        }

        public void UpdateNotesIndicator()
        {
            foreach (var day in CalendarDays)
            {
                day.HasNotes = AllNotes.Any(n => n.Date.Date == day.Date.Date);
            }
        }
    }
}