using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PropertyChanged;

namespace DemoCalc; 

[AddINotifyPropertyChangedInterface]
public partial class MainWindow : Window {
    private readonly List<string> _operations = new();

    public MainWindow() {
        InitializeComponent();
        DataContext = this;
    }

    public string Display { get; set; } = "0";

    private void Refresh() {
        if (_operations.Count == 0) {
            Display = "0";
            return;
        }

        Display = string.Join("", _operations);
        resultBox.ScrollToHorizontalOffset(resultBox.ExtentWidth);
    }

    private void PushNumber(string number) {
        if (_operations.Count > 0) {
            switch (_operations[^1][0]) {
                case '+':
                case '-':
                case '*':
                case '/':
                    if (number == ".") return;
                    _operations.Add(number);
                    break;
                default:
                    var last = _operations.Last();
                    if (last.Contains('.') && number == ".") return;
                    if (last.StartsWith('0') && number == "0") return;
                    if (last == "0" && number != ".")
                        _operations[^1] = number;
                    else
                        _operations[^1] += number;

                    break;
            }
        } else {
            if (number == ".") return;
            _operations.Add(number);
        }

        Refresh();
    }

    private void PushOperator(string content) {
        if (_operations.Count == 0) return;

        switch (_operations[^1][0]) {
            case '+':
            case '-':
            case '*':
            case '/':
                return;
        }

        _operations.Add(content);

        Refresh();
    }

    private void NumberButton_Click(object sender, RoutedEventArgs e) {
        var number = ((Button)sender).Content.ToString()!;
        PushNumber(number);
    }

    private void OperatorButton_Click(object sender, RoutedEventArgs e) {
        var content = ((Button)sender).Content.ToString()!;
        PushOperator(content);
    }

    private bool Calculate(ref int i) {
        var a = double.Parse(_operations[i - 1].Replace('.', ','));
        var b = double.Parse(_operations[i + 1].Replace('.', ','));
        switch (_operations[i][0]) {
            case '*':
                _operations[i - 1] = (a * b).ToString(CultureInfo.CurrentCulture);
                break;
            case '/':
                if (b == 0) {
                    MessageBox.Show("Attempt to division by zero!", "Calculation error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                _operations[i - 1] = (a / b).ToString(CultureInfo.CurrentCulture);
                break;
            case '+':
                _operations[i - 1] = (a + b).ToString(CultureInfo.CurrentCulture);
                break;
            case '-':
                _operations[i - 1] = (a - b).ToString(CultureInfo.CurrentCulture);
                break;
        }

        _operations.RemoveAt(i);
        _operations.RemoveAt(i);
        i--;
        return true;
    }

    private void EqualOperations() {
        if (_operations.Count == 0) return;

        switch (_operations[^1][0]) {
            case '+':
            case '-':
            case '*':
            case '/':
                _operations.RemoveAt(_operations.Count - 1);
                break;
        }

        for (var i = 0; i < _operations.Count; i++)
            switch (_operations[i][0]) {
                case '*':
                    Calculate(ref i);
                    break;
                case '/':
                    if (!Calculate(ref i)) {
                        Refresh();
                        return;
                    }

                    break;
            }

        for (var i = 0; i < _operations.Count; i++)
            switch (_operations[i][0]) {
                case '+':
                    Calculate(ref i);
                    break;
                case '-':
                    Calculate(ref i);
                    break;
            }

        Display = _operations[0].Replace(',', '.');
    }

    private void EqualButton_Click(object sender, RoutedEventArgs e) {
        EqualOperations();
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e) {
        Display = "0";
        _operations.Clear();
    }

    private void DelLast() {
        if (_operations.Count == 0) return;

        var last = _operations.Last();
        last = last[..^1];
        if (string.IsNullOrEmpty(last))
            _operations.RemoveAt(_operations.Count - 1);
        else
            _operations[^1] = last;

        Refresh();
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e) {
        DelLast();
    }

    private void MainWindow_OnKeyDown(object sender, KeyEventArgs e) {
        switch (e.Key) {
            case Key.D0:
            case Key.NumPad0:
                PushNumber("0");
                break;
            case Key.D1:
            case Key.NumPad1:
                PushNumber("1");
                break;
            case Key.D2:
            case Key.NumPad2:
                PushNumber("2");
                break;
            case Key.D3:
            case Key.NumPad3:
                PushNumber("3");
                break;
            case Key.D4:
            case Key.NumPad4:
                PushNumber("4");
                break;
            case Key.D5:
            case Key.NumPad5:
                PushNumber("5");
                break;
            case Key.D6:
            case Key.NumPad6:
                PushNumber("6");
                break;
            case Key.D7:
            case Key.NumPad7:
                PushNumber("7");
                break;
            case Key.D8:
            case Key.NumPad8:
                PushNumber("8");
                break;
            case Key.D9:
            case Key.NumPad9:
                PushNumber("9");
                break;

            case Key.Add:
                PushOperator("+");
                break;
            case Key.Subtract:
            case Key.OemMinus:
                PushOperator("-");
                break;
            case Key.Multiply:
                PushOperator("*");
                break;
            case Key.Divide:
                PushOperator("/");
                break;

            case Key.OemPlus:
                if (!Keyboard.IsKeyDown(Key.LeftShift))
                    EqualOperations();
                else
                    PushOperator("+");
                break;

            case Key.Back:
                DelLast();
                break;
            case Key.Enter:
                EqualOperations();
                break;
        }
    }
}