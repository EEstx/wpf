using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CalendarNotes.Helpers;
using CalendarNotes.Models;

namespace CalendarNotes.ViewModels
{
    public class NoteViewModel : ObservableObject
    {
        private readonly DateTime _date;
        private readonly ObservableCollection<Note> _allNotes;

        public ObservableCollection<Note> DayNotes { get; } = new();

        public string Title => $"Заметки: {_date:dd.MM.yyyy}";

        private string _newNoteText = string.Empty;
        public string NewNoteText
        {
            get => _newNoteText;
            set => SetProperty(ref _newNoteText, value);
        }

        public ICommand AddNoteCommand { get; }
        public ICommand DeleteNoteCommand { get; }

        public NoteViewModel(DateTime date, ObservableCollection<Note> allNotes)
        {
            _date = date;
            _allNotes = allNotes;
            
            LoadNotes();

            AddNoteCommand = new RelayCommand(AddNote);
            DeleteNoteCommand = new RelayCommand(DeleteNote);
        }

        private void LoadNotes()
        {
            DayNotes.Clear();
            var notesForDay = _allNotes.Where(x => x.Date.Date == _date.Date).ToList();
            foreach (var n in notesForDay)
            {
                DayNotes.Add(n);
            }
        }

        private void AddNote(object? obj)
        {
            if (string.IsNullOrWhiteSpace(NewNoteText)) return;

            var note = new Note { Date = _date.Date, Text = NewNoteText.Trim() };
            _allNotes.Add(note);
            DayNotes.Add(note);

            NewNoteText = string.Empty;
        }

        private void DeleteNote(object? obj)
        {
            if (obj is Note note)
            {
                _allNotes.Remove(note);
                DayNotes.Remove(note);
            }
        }
    }
}