using GuessNumber.Game.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace GuessNumber
{
    class GuessTextValidator : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                Guess guess = GetBoundValue<Guess>(value);
                if (guess == null)
                {
                    return new ValidationResult(false, "Invalid");
                }
                else
                {
                    return new ValidationResult(true, null);
                }
            }
            catch (Exception ex)
            {
                return new ValidationResult(true, "Designing mode");
            }
            
        }

        private T GetBoundValue<T>(object value)
        {
            if (value is BindingExpression)
            {
                // ValidationStep was UpdatedValue or CommittedValue (Validate after setting)
                // Need to pull the value out of the BindingExpression.
                BindingExpression binding = (BindingExpression)value;

                // Get the bound object and name of the property
                object dataItem = binding.DataItem;
                string propertyName = binding.ParentBinding.Path.Path;

                // Extract the value of the property.
                object propertyValue = GetPropertyValue(dataItem, propertyName);

                // This is what we want.
                return (T)propertyValue;
            }
            else
            {
                // ValidationStep was RawProposedValue or ConvertedProposedValue
                // The argument is already what we want!
                return (T)value;
            }
        }

        public static object GetPropertyValue(object obj, string propertyName)
        {
            foreach (string part in propertyName.Split('.'))
            {
                Type type = obj.GetType();
                PropertyInfo info = type.GetProperty(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                obj = info.GetValue(obj, null);
            }

            return obj;
        }
    }

    [ValueConversion(typeof(Guess), typeof(string))]
    class GuessTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Guess guess = (Guess)value;
            if (guess == null)
            {
                return string.Empty;
            }
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < Config.Slots; i++)
            {
                stringBuilder.Append(guess[i]);
            }
            return stringBuilder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = (string)value;
            if (text.Length != Config.Slots)
            {
                return null;
            }
            Guess guess = new Guess();
            for (int i = 0; i < text.Length; i++)
            {
                guess[i] = text[i] - '0';
            }
            if (guess.IsValid())
            {
                return guess;
            }
            else
            {
                return null;
            }
        }
    }

    [ValueConversion(typeof(bool), typeof(SolidColorBrush))]
    class BrushBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Brushes.Transparent;
            }
            else
            {
                return Brushes.Red;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush solidColorBrush = (SolidColorBrush)value;
            return solidColorBrush == Brushes.Transparent;
        }
    }
}
