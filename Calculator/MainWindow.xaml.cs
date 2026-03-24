using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Calculator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private double _lastNumber, _result;
    private string _selectedOperator = "";

    public MainWindow()
    {
        InitializeComponent();
    }

    // Обработка нажатия на цифровую кнопку
    private void Digit_Click(object sender, RoutedEventArgs e)
    {
        Button button = (Button)sender;
        
        if (Display.Text == "0" || _selectedOperator == "=")
        {
            Display.Text = button.Content.ToString();
            
            if (_selectedOperator == "=") _selectedOperator = "";
        }
        else
        {
            Display.Text += button.Content.ToString();
        }
    }

    // Выбор математической операции (+, -, *, /)
    private void Operation_Click(object sender, RoutedEventArgs e)
    {
        Button button = (Button)sender;
        
        if (double.TryParse(Display.Text, out _lastNumber))
        {
            Display.Text = "0";
            _selectedOperator = button.Content.ToString();
        }
    }

    // Кнопка вычисления результата (равно)
    private void Equals_Click(object sender, RoutedEventArgs e)
    {
        double newNumber;
        
        if (double.TryParse(Display.Text, out newNumber))
        {
            switch (_selectedOperator)
            {
                case "+":
                    _result = _lastNumber + newNumber;
                    break;
                case "-":
                    _result = _lastNumber - newNumber;
                    break;
                case "*":
                    _result = _lastNumber * newNumber;
                    break;
                case "/":
                    _result = newNumber == 0 ? 0 : _lastNumber / newNumber;
                    break;
                default:
                    _result = newNumber;
                    break;
            }
            Display.Text = _result.ToString();
            
            _selectedOperator = "=";
        }
    }

    // Сброс калькулятора (кнопка С)
    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _lastNumber = 0;
        _result = 0;
        _selectedOperator = "";
        
        Display.Text = "0";
    }
}