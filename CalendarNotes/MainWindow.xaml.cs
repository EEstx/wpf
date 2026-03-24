using System.Windows;
using CalendarNotes.ViewModels;
using CalendarNotes.Views;

namespace CalendarNotes;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        if (DataContext is MainViewModel vm)
        {
            vm.RequestOpenNotes = (date) =>
            {
                var noteWin = new NoteWindow(date, vm.AllNotes);
                noteWin.Owner = this;
                noteWin.ShowDialog();
                
                vm.UpdateNotesIndicator();
            };
        }
    }
}