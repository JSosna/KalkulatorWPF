using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kalkulator
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private bool lastButtonWasOperator;
        private bool lastButtonWasOperator;
        private Button lastOperator;
        private List<double> savedNumbers = new List<double>();
        private List<char> savedOperators = new List<char>();
        private double finalResult = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Number_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button.Content.ToString() == "," && !Wynik.Text.ToString().Contains(','))
                Wynik.Text += ',';

            else if (button.Content.ToString() != ",")
            {
                double tmp = 0;

                if (lastButtonWasOperator)
                {
                    Double.TryParse(Wynik.Text, out tmp);
                    savedNumbers.Add(tmp);

                    Wynik.Text = "0";

                    lastOperator.BorderThickness = new Thickness(0);
                }

                Double.TryParse(Wynik.Text + button.Content, out tmp);
                Wynik.Text = $"{tmp}";
            }

            lastButtonWasOperator = false;
        }

        private void Operator_Click(object sender, RoutedEventArgs e)
        {
            if (lastButtonWasOperator)
            {
                lastOperator.BorderThickness = new Thickness(0);
                savedOperators.RemoveAt(savedOperators.Count - 1);
            }
                

            if (lastOperator != sender || !lastButtonWasOperator)
            {
                lastButtonWasOperator = true;
                lastOperator = sender as Button;
                lastOperator.BorderThickness = new Thickness(2);
                savedOperators.Add(lastOperator.Content.ToString()[0]);
            }
            else
            {
                // Dzięki temu można odznaczyć operator i dalej wprowadzać liczbę
                lastOperator = null;
                lastButtonWasOperator = false;
                savedOperators.RemoveAt(savedOperators.Count - 1);
            }
                
        }

        private void AC_Click(object sender, RoutedEventArgs e)
        {
            savedNumbers.Clear();
            finalResult = 0;
            Wynik.Text = "0";
            lastButtonWasOperator = false;
            lastOperator.BorderThickness = new Thickness(0);
            lastOperator = null;
        }

        private void CE_Click(object sender, RoutedEventArgs e)
        {
            Wynik.Text = "0";
            lastButtonWasOperator = false;
            lastOperator.BorderThickness = new Thickness(0);
            lastOperator = null;
        }

        private void Neg_Click(object sender, RoutedEventArgs e)
        {
            if (Wynik.Text.Contains("-"))
                Wynik.Text = Wynik.Text.Replace("-", "");
            else
                Wynik.Text = "-" + Wynik.Text;

        }
    }
}
