using System;
using System.Collections.ObjectModel;
using System.Windows;
using CalendarNotes.Models;
using CalendarNotes.ViewModels;

namespace CalendarNotes.Views
{
    public partial class NoteWindow : Window
    {
        public NoteWindow(DateTime date, ObservableCollection<Note> allNotes)
        {
            InitializeComponent();
            DataContext = new NoteViewModel(date, allNotes);
        }
    }
}