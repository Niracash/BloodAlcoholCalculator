﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BloodAlcoholCalculator
{
    public partial class MainWindow : Window
    {
        private string selectedGender = "Male";

        public MainWindow()
        {
            InitializeComponent();
            GenderLabel.Text = "Selected Gender: Male";
            StateChanged += MainWindow_StateChanged; // Handle window state changes
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                MaxHeight = SystemParameters.WorkArea.Height;
                MaxWidth = SystemParameters.WorkArea.Width;
            }
            else
            {
                MaxHeight = double.PositiveInfinity;
                MaxWidth = double.PositiveInfinity;
            }
        }

        private void SelectGender(object sender, RoutedEventArgs e)
        {
            selectedGender = sender == MaleButton ? "Male" : "Female";
            GenderLabel.Text = $"Selected Gender: {selectedGender}";
        }

        private void CalculateBAC(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!double.TryParse(WeightTextBox.Text, out double weight) ||
                    !double.TryParse(DrinkAmountTextBox.Text, out double drinkAmount))
                {
                    MessageBox.Show("Please enter valid numeric values.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                double timeHours = double.TryParse(TimeHoursTextBox.Text, out double parsedHours) ? parsedHours : 0;
                double timeMinutes = double.TryParse(TimeMinutesTextBox.Text, out double parsedMinutes) ? parsedMinutes : 0;

                if (WeightUnitComboBox.SelectedItem == null ||
                    DrinkUnitComboBox.SelectedItem == null ||
                    DrinkTypeComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Please select all dropdown values.", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string weightUnit = ((ComboBoxItem)WeightUnitComboBox.SelectedItem).Content.ToString();
                string drinkUnit = ((ComboBoxItem)DrinkUnitComboBox.SelectedItem).Content.ToString();
                string drinkType = ((ComboBoxItem)DrinkTypeComboBox.SelectedItem).Content.ToString();

                bool customAlcohol = double.TryParse(CustomAlcoholTextBox.Text, out double customAlcoholPercentage);

                if (weightUnit == "KG")
                    weight *= 2.20462;

                double r = selectedGender == "Male" ? 0.68 : 0.55;
                double totalTimeElapsed = timeHours + (timeMinutes / 60.0);
                double drinkInOunces = ConvertToOunces(drinkAmount, drinkUnit);
                double alcoholPercentage = customAlcohol ? customAlcoholPercentage : GetAlcoholContent(drinkType);

                if (drinkInOunces <= 0 || alcoholPercentage <= 0)
                {
                    MessageBox.Show("Invalid drink amount or alcohol percentage.", "Calculation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                double pureAlcoholOz = drinkInOunces * (alcoholPercentage / 100.0);
                double BAC = ((pureAlcoholOz * 5.14 / (weight * r)) - (0.017 * totalTimeElapsed));
                BAC = Math.Max(BAC, 0) * 10;
                BACResultLabel.Text = $"BAC: {BAC:F3}‰";

                if (BAC > 0.49)
                {
                    DrivingStatusLabel.Text = "Not Allowed to Drive";
                    DrivingStatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    DrivingStatusLabel.Text = "Allowed to Drive";
                    DrivingStatusLabel.Foreground = new SolidColorBrush(Colors.LightGreen);
                }

                double soberTime = BAC / 0.15;
                TimeSpan soberTimeSpan = TimeSpan.FromHours(soberTime);
                int hours = (int)soberTimeSpan.TotalHours;
                int minutes = soberTimeSpan.Minutes;
                SoberTimeLabel.Text = $"Estimated Sober Up Time: {hours}h {minutes}m";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private double ConvertToOunces(double amount, string unit)
        {
            return unit switch
            {
                "ML" => amount * 0.033814,
                "Glass" => amount * 12.7,
                "OZ" => amount,
                _ => 0
            };
        }

        private double GetAlcoholContent(string drinkType)
        {
            return drinkType switch
            {
                "Beer" => 5,
                "Wine" => 12,
                "Whiskey" => 40,
                "Vodka" => 40,
                "Cocktail" => 20,
                _ => 0
            };
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (WindowState == WindowState.Normal)
                    WindowState = WindowState.Maximized;
                else
                    WindowState = WindowState.Normal;
            }
            else
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
