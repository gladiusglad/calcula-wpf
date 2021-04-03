using CalculaCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;

namespace CalculaWPF
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private const string InvalidMessage = "Please enter a valid expression",
            HistoryPath = "history.json";
        private readonly Calculator calculator = new();

        private bool lockResult, noChangeSinceLock;
        private string expression, result, previousExpression, lastValidResult, calculateTime;
        private MessageColor color = MessageColor.Unfocused;
        private DateTimeOffset lastForceTime;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ICommand CalculateCommand { get; set; }
        public ICommand SetExpressionCommand { get; set; }
        public ObservableCollection<HistoryEntry> History { get; } = new();

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

        public MainWindowViewModel()
        {
            CalculateCommand = new RelayCommand(ForceCalculate, o => true);
            SetExpressionCommand = new RelayCommand(SetExpression, o => true);
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

            if (result == null)
            {
                CalculateTime = "";
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
                CalculateTime = $"{DateTimeOffset.Now.Subtract(startCalculate).TotalMilliseconds}ms";
                Color = MessageColor.Normal;
                Result = result.Value.ToString("G");
                lastValidResult = Result;

                if (force)
                {
                    lockResult = true;
                    lastForceTime = DateTimeOffset.Now;
                    noChangeSinceLock = true;

                    if (exp != Result)
                    {
                        HistoryEntry entry = new(exp, Result);
                        if (History.Count == 0 || History[^1] != entry)
                        {
                            History.Add(entry);
                        }
                    }
                }
            }
        }

        public void ForceCalculate(object expression)
        {
            Calculate(expression, true);
        }

        public void SetExpression(object expression)
        {
            Expression = expression == null ? "" : (string)expression;
        }

        public void LoadHistory()
        {
            if (File.Exists(HistoryPath))
            {
                History.Clear();
                JsonSerializer.Deserialize<List<HistoryEntry>>(File.ReadAllText(HistoryPath)).ForEach(e => History.Add(e));
            }
        }

        public void SaveHistory()
        {
            File.WriteAllText(HistoryPath, JsonSerializer.Serialize(new List<HistoryEntry>(History)));
        }
    }
}
