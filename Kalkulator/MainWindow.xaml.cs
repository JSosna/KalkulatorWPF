﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kalkulator
{
    public partial class MainWindow : Window
    {
        private bool lastButtonWasOperator;
        private Button lastOperator;
        private double finalResult = 0;
        private bool equalsPressed = false;
        private string expression = "";

        public MainWindow()
        {
            InitializeComponent();
            Load_Saved_Numbers();
        }

        private void Load_Saved_Numbers()
        {
            string[] numbers = SqliteDataAccess.Load_Numbers();
            
            for(int i=0; i<numbers.Length; i++)
            {
                Save_Number(numbers[i]);
            }
        }

        private void Number_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (equalsPressed)
            {
                AC_Click(null, null);
            }

            if (button.Content.ToString() == "," && lastButtonWasOperator)
                ResultTextBox.Text = "0,";
            else if (button.Content.ToString() == "," && !ResultTextBox.Text.ToString().Contains(','))
                ResultTextBox.Text += ',';

            if (button.Content.ToString() != ",")
            {
                if (lastButtonWasOperator)
                {
                    ResultTextBox.Text = "0";
                    lastOperator.BorderThickness = new Thickness(0);
                }

                if(button.Content.ToString() == "0" && ResultTextBox.Text.ToString().Contains(','))
                    ResultTextBox.Text += "0";
                else
                {
                    Double.TryParse(ResultTextBox.Text + button.Content, out double tmp);
                    if(tmp.ToString().Length<19)
                        ResultTextBox.Text = $"{tmp}";
                }
            }

            equalsPressed = false;
            lastButtonWasOperator = false;
        }

        async private void Operator_Click(object sender, RoutedEventArgs e)
        {
            if (!lastButtonWasOperator && !equalsPressed)
            {
                Calculate_result();
                if (Double.IsNaN(finalResult) || Double.IsInfinity(finalResult))
                    ResultTextBox.Text = "BŁĄD";
                else
                    ResultTextBox.Text = finalResult.ToString();
            }

            // Operator change
            if (lastOperator != sender && lastButtonWasOperator)
            {
                lastOperator.BorderThickness = new Thickness(0);

                DoubleAnimation da = new DoubleAnimation(((Label)StackHelpers.Children[StackHelpers.Children.Count - 1]).ActualWidth, 0, new Duration(TimeSpan.FromSeconds(0.2)));
                StackHelpers.Children[StackHelpers.Children.Count - 1].BeginAnimation(WidthProperty, da);

                await Task.Delay(200);
                StackHelpers.Children.RemoveAt(StackHelpers.Children.Count - 1);
            }

            if (lastOperator != sender || !lastButtonWasOperator)
            {
                lastOperator = sender as Button;
                lastOperator.BorderThickness = new Thickness(2);

                Add_Helper_Operator(lastOperator.Content.ToString()[0]);
            }

            equalsPressed = false;
            lastButtonWasOperator = true;
        }

        private void AC_Click(object sender, RoutedEventArgs e)
        {
            finalResult = 0;
            ResultTextBox.Text = "0";
            lastButtonWasOperator = false;
            equalsPressed = false;
            expression = "";
            if (lastOperator != null)
            {
                lastOperator.BorderThickness = new Thickness(0);
                lastOperator = null;
            }

            Remove_Helpers();
        }

        private void CE_Click(object sender, RoutedEventArgs e)
        {
            if (equalsPressed)
            {
                AC_Click(null, null);
                equalsPressed = false;
            }
                
            ResultTextBox.Text = "0";
        }

        private void Neg_Click(object sender, RoutedEventArgs e)
        {
            if(equalsPressed || lastButtonWasOperator)
            {
                var tmp = finalResult;
                AC_Click(null, null);
                ResultTextBox.Text = tmp.ToString();
                equalsPressed = false;
                lastButtonWasOperator = false;
            }

            if (ResultTextBox.Text.Contains("-"))
                ResultTextBox.Text = ResultTextBox.Text.Replace("-", "");
            else
                ResultTextBox.Text = "-" + ResultTextBox.Text;
        }

         async private void Add_Helper_Number(double value)
        {
            Border border = new Border() { Style = FindResource("BorderForHelpLabel") as Style, Opacity = 0 };
            Label label = new Label() { Content = value.ToString().Replace('.',','), Style = FindResource("HelpBottomLabel") as Style };
            label.MouseDoubleClick += Number_MouseDoubleClick;

            border.Child = label;

            SavedNumbersPanel.Children.Add(border);
            await Task.Delay(20);
            var actualWidth = border.ActualWidth;
            SavedNumbersPanel.Children.Remove(border);

            StackHelpers.Children.Add(border);

            DoubleAnimation da = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.3)));
            border.BeginAnimation(OpacityProperty, da);
            DoubleAnimation da1 = new DoubleAnimation(0, actualWidth, new Duration(TimeSpan.FromSeconds(0.25)));
            border.BeginAnimation(WidthProperty, da1);
        }

        async private void Add_Helper_Operator(char o)
        {
            Label label = new Label() { Content = o, Style = FindResource("BasicOperatorLabel") as Style, Opacity = 0 };

            await Task.Delay(45);
            StackHelpers.Children.Add(label);

            DoubleAnimation da = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.2)));
            label.BeginAnimation(OpacityProperty, da);
            DoubleAnimation da1 = new DoubleAnimation(0, 16, new Duration(TimeSpan.FromSeconds(0.2)));
            label.BeginAnimation(WidthProperty, da1);
        }

        async private void Equals_Click(object sender, RoutedEventArgs e)
        {
            if (!equalsPressed)
            {
                Calculate_result();

                if (Double.IsNaN(finalResult) || Double.IsInfinity(finalResult))
                    ResultTextBox.Text = "BŁĄD";
                else
                    ResultTextBox.Text = finalResult.ToString();
            }
            else
            {
                Remove_Helpers();
                await Task.Delay(300);
                expression = finalResult.ToString();
                Add_Helper_Number(finalResult);
            }

            if (lastOperator != null)
                lastOperator.BorderThickness = new Thickness(0);
            equalsPressed = true;
            lastButtonWasOperator = false;
        }

        async private void Remove_Helpers()
        {
            for(int i=0; i<StackHelpers.Children.Count; i++)
            {
                DoubleAnimation daOpacity = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.3)));
                if (StackHelpers.Children[i].GetType() == typeof(Label))
                {
                    DoubleAnimation da = new DoubleAnimation(((Label)StackHelpers.Children[i]).ActualWidth, 0, new Duration(TimeSpan.FromSeconds(0.3)));
                    StackHelpers.Children[i].BeginAnimation(WidthProperty, da);
                    StackHelpers.Children[i].BeginAnimation(OpacityProperty, daOpacity);
                }
                    
                else if(StackHelpers.Children[i].GetType() == typeof(Border))
                {
                    DoubleAnimation da = new DoubleAnimation(((Border)StackHelpers.Children[i]).ActualWidth, 0, new Duration(TimeSpan.FromSeconds(0.3)));
                    StackHelpers.Children[i].BeginAnimation(WidthProperty, da);
                    StackHelpers.Children[i].BeginAnimation(OpacityProperty, daOpacity);
                }
            }

            await Task.Delay(300);
            StackHelpers.Children.Clear();
        }


        private void Calculate_result()
        {
            Double.TryParse(ResultTextBox.Text, out double tmp);
            Add_Helper_Number(tmp);
            if(lastOperator != null)
                expression += lastOperator.Tag.ToString();
            expression += $"({tmp})";
            //expression += tmp;

            Console.WriteLine(expression);

            DataTable dt = new DataTable();
            try
            {
                double value = Double.Parse(dt.Compute(expression.Replace(',', '.'), "").ToString());
                value = Double.Parse(value.ToString(), NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint);
                finalResult = value;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                ResultTextBox.Text = "Błąd!";
            }
        }

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ScrollViewer sv = sender as ScrollViewer;
            sv.ScrollToRightEnd();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D0 || e.Key == Key.NumPad0)
                Set_Pressed(Button0);
            if (e.Key == Key.D1 || e.Key == Key.NumPad1)
                Set_Pressed(Button1);
            if (e.Key == Key.D2 || e.Key == Key.NumPad2)
                Set_Pressed(Button2);
            if (e.Key == Key.D3 || e.Key == Key.NumPad3)
                Set_Pressed(Button3);
            if (e.Key == Key.D4 || e.Key == Key.NumPad4)
                Set_Pressed(Button4);
            if (e.Key == Key.D5 || e.Key == Key.NumPad5)
                Set_Pressed(Button5);
            if (e.Key == Key.D6 || e.Key == Key.NumPad6)
                Set_Pressed(Button6);
            if (e.Key == Key.D7 || e.Key == Key.NumPad7)
                Set_Pressed(Button7);
            if (e.Key == Key.D8 || e.Key == Key.NumPad8)
                Set_Pressed(Button8);
            if (e.Key == Key.D9 || e.Key == Key.NumPad9)
                Set_Pressed(Button9);

            if (e.Key == Key.Multiply)
                Set_Pressed(ButtonMult);
            if (e.Key == Key.OemMinus || e.Key == Key.Subtract)
                Set_Pressed(ButtonMinus);
            if (e.Key == Key.Divide || e.Key == Key.OemQuestion)
                Set_Pressed(ButtonDiv);
            if (e.Key == Key.Add)
                Set_Pressed(ButtonPlus);
            if (e.Key == Key.Decimal || e.Key == Key.OemComma || e.Key == Key.OemPeriod)
                Set_Pressed(ButtonComma);
            if (e.Key == Key.OemPlus || e.Key == Key.Return || e.Key == Key.Enter)
                Set_Pressed(ButtonEquals);
            if (e.Key == Key.Delete)
                Set_Pressed(ButtonAC);
            if (e.Key == Key.Back)
                Set_Pressed(ButtonCE);
            if (e.Key == Key.N)
                Set_Pressed(ButtonNeg);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D0 || e.Key == Key.NumPad0)
                Set_Unpressed(Button0);
            if (e.Key == Key.D1 || e.Key == Key.NumPad1)
                Set_Unpressed(Button1);
            if (e.Key == Key.D2 || e.Key == Key.NumPad2)
                Set_Unpressed(Button2);
            if (e.Key == Key.D3 || e.Key == Key.NumPad3)
                Set_Unpressed(Button3);
            if (e.Key == Key.D4 || e.Key == Key.NumPad4)
                Set_Unpressed(Button4);
            if (e.Key == Key.D5 || e.Key == Key.NumPad5)
                Set_Unpressed(Button5);
            if (e.Key == Key.D6 || e.Key == Key.NumPad6)
                Set_Unpressed(Button6);
            if (e.Key == Key.D7 || e.Key == Key.NumPad7)
                Set_Unpressed(Button7);
            if (e.Key == Key.D8 || e.Key == Key.NumPad8)
                Set_Unpressed(Button8);
            if (e.Key == Key.D9 || e.Key == Key.NumPad9)
                Set_Unpressed(Button9);

            if (e.Key == Key.Multiply)
                Set_Unpressed(ButtonMult);
            if (e.Key == Key.OemMinus || e.Key == Key.Subtract)
                Set_Unpressed(ButtonMinus);
            if (e.Key == Key.Divide || e.Key == Key.OemQuestion)
                Set_Unpressed(ButtonDiv);
            if (e.Key == Key.Add)
                Set_Unpressed(ButtonPlus);
            if (e.Key == Key.Decimal || e.Key == Key.OemComma || e.Key == Key.OemPeriod)
                Set_Unpressed(ButtonComma);
            if (e.Key == Key.OemPlus || e.Key == Key.Return)
                Set_Unpressed(ButtonEquals);
            if (e.Key == Key.Delete)
                Set_Unpressed(ButtonAC);
            if (e.Key == Key.Back)
                Set_Unpressed(ButtonCE);
            if (e.Key == Key.N)
                Set_Unpressed(ButtonNeg);
        }

        private void Set_Pressed(Button button)
        {
            button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(button, new object[] { true });
        }

        private void Set_Unpressed(Button button)
        {
            typeof(Button).GetMethod("set_IsPressed", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(button, new object[] { false });
        }

        private void ResultTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Save_Number(ResultTextBox.Text);
            SqliteDataAccess.Save_Number(ResultTextBox.Text);
        }

        private void Number_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Label label = sender as Label;
            Save_Number(label.Content.ToString());
            SqliteDataAccess.Save_Number(label.Content.ToString());
        }

        async private void Save_Number(String number)
        {
            Border border = new Border() { Style = FindResource("SavedNumberBorder") as Style, Opacity = 0 };
            StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal };

            Label labelNumber = new Label() { Style = FindResource("SavedNumberLabel") as Style, Content = number };
            Label labelX = new Label() { Style = FindResource("RemoveSavedNumberLabel") as Style };
            labelNumber.MouseDown += SavedNumber_Click;
            labelX.MouseDown += Remove_SavedNumber_Click;

            sp.Children.Add(labelNumber);
            sp.Children.Add(labelX);

            border.Child = sp;

            SavedNumbersPanel.Children.Add(border);
            await Task.Delay(30);
            var actualWidth = border.ActualWidth;

            DoubleAnimation da = new DoubleAnimation(0, actualWidth, new Duration(TimeSpan.FromSeconds(0.3)));
            DoubleAnimation da1 = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.3)));

            border.BeginAnimation(WidthProperty, da);
            border.BeginAnimation(OpacityProperty, da1);
        }

        private void SavedNumber_Click(object sender, MouseButtonEventArgs e)
        {
            Label label = sender as Label;

            if (equalsPressed)
            {
                AC_Click(null, null);
                equalsPressed = false;
            }
            if (lastButtonWasOperator)
            {
                lastButtonWasOperator = false;
                lastOperator.BorderThickness = new Thickness(0);
            }

            ResultTextBox.Text = label.Content.ToString();
        }

        async private void Remove_SavedNumber_Click(object sender, MouseButtonEventArgs e)
        {
            Label label = sender as Label;
            StackPanel sp = label.Parent as StackPanel;
            Border border = sp.Parent as Border;

            SqliteDataAccess.Delete_Number(((Label)sp.Children[0]).Content.ToString());

            Console.WriteLine(border.ActualWidth);

            DoubleAnimation doubleAnimation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.3)));
            DoubleAnimation doubleAnimation1 = new DoubleAnimation(border.ActualWidth, 0, new Duration(TimeSpan.FromSeconds(0.3)));
            border.BeginAnimation(OpacityProperty, doubleAnimation);
            border.BeginAnimation(WidthProperty, doubleAnimation1);

            await Task.Delay(300);
            SavedNumbersPanel.Children.Remove(border);
        }
    }
}
