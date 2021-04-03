using CalculaCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CalculaWPF
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private const string InvalidMessage = "Please enter a valid expression";
        private readonly Calculator calculator = new();

        private bool lockResult, noChangeSinceLock;
        private string expression, result, previousExpression, lastValidResult, calculateTime;
        private MessageColor color = MessageColor.Unfocused;
        private DateTimeOffset lastForceTime;

        public MainWindowViewModel()
        {
            CalculateCommand = new RelayCommand(ForceCalculate, o => true);
            ClearCommand = new RelayCommand(ClearExpression, o => true);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ICommand CalculateCommand { get; set; }
        public ICommand ClearCommand { get; set; }

        public string Expression
        {
            get => expression;
            set
            {
                previousExpression = expression;
                expression = value;
                noChangeSinceLock = false;
                Calculate(value, false);
                OnPropertyChanged();
            }
        }

        public string Result
        {
            get => result;
            set
            {
                result = value;
                OnPropertyChanged();
            }
        }

        public string CalculateTime
        {
            get => calculateTime;
            set
            {
                calculateTime = value;
                OnPropertyChanged();
            }
        }

        public MessageColor Color
        {
            get => color;
            set
            {
                color = value;
                OnPropertyChanged();
            }
        }

        private void SendInvalid()
        {
            Result = InvalidMessage;
            Color = MessageColor.Warning;
        }

        public void Calculate(object expression, bool force)
        {
            if (force && noChangeSinceLock && lockResult && Result == lastValidResult &&
                DateTimeOffset.Now.Subtract(lastForceTime).TotalMilliseconds < 500)
            {
                Expression = Result;
                return;
            }

            string exp = (string)expression;

            if (exp.Length == 0 && !force)
            {
                lastValidResult = "";
            }

            if (lockResult && !force)
            {
                if (previousExpression != null && (exp.Length > previousExpression.Length || exp != previousExpression.Substring(0, exp.Length)))
                {
                    lockResult = false;
                }
                else
                {
                    if (exp.Length == 0)
                    {
                        Color = MessageColor.Unfocused;
                    }
                    return;
                }
            }

            if (exp.Length == 0)
            {
                CalculateTime = "";
                if (force)
                {
                    SendInvalid();
                }
                else
                {
                    Result = "";
                    Color = MessageColor.Unfocused;
                }
                return;
            }

            DateTimeOffset startCalculate = DateTimeOffset.Now;
            decimal? result;

            try
            {
                result = calculator.Calculate(exp, new CalculatorOptions(Debug: force));
            }
            catch
            {
                result = null;
            }

            CalculateTime = $"{DateTimeOffset.Now.Subtract(startCalculate).TotalMilliseconds}ms";

            if (result == null)
            {
                if (force)
                {
                    SendInvalid();
                }
                else
                {
                    Result = lastValidResult;
                    Color = MessageColor.Unfocused;
                }
            }
            else
            {
                Color = MessageColor.Normal;
                Result = result.Value.ToString("G");
                lastValidResult = Result;

                if (force)
                {
                    lockResult = true;
                    lastForceTime = DateTimeOffset.Now;
                    noChangeSinceLock = true;
                }
            }
        }

        public void ForceCalculate(object expression)
        {
            Calculate(expression, true);
        }

        public void ClearExpression(object _)
        {
            Expression = "";
        }
    }
}
