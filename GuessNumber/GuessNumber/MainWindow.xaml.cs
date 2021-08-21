using BiliLogin;
using GuessNumber.Game;
using GuessNumber.Game.Core;
using GuessNumber.Game.DanmakuGame;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace GuessNumber
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Private fields

        private CookieCollection userCookieCollection = null;
        private Thread workingThread = null;
        private bool responseSyncing = false;
        private StringBuilder logTextBuilder;
        private Queue<string> logQueue = new Queue<string>();
        private Task pendingAppendLogTask = null;

        #endregion

        #region Dependency property

        #region Registration

        private static readonly DependencyProperty CurrentPuzzleProperty = DependencyProperty.Register("CurrentPuzzle", typeof(PuzzleBase), typeof(MainWindow), new PropertyMetadata(new PropertyChangedCallback((d, e) => { ((MainWindow)d).OnCanSetResponseChanged(e); })));
        private static readonly DependencyProperty CurrentSolverProperty = DependencyProperty.Register("CurrentSolver", typeof(PuzzleSolverBase), typeof(MainWindow), new PropertyMetadata(null));

        private static readonly DependencyProperty AProperty = DependencyProperty.Register("A", typeof(int), typeof(MainWindow), new PropertyMetadata(0, new PropertyChangedCallback((d, e) => { ((MainWindow)d).OnAChanged(e); })));
        private static readonly DependencyProperty BProperty = DependencyProperty.Register("B", typeof(int), typeof(MainWindow), new PropertyMetadata(0, new PropertyChangedCallback((d, e) => { ((MainWindow)d).OnBChanged(e); })));
        private static readonly DependencyProperty NumCorrectProperty = DependencyProperty.Register("NumCorrect", typeof(int), typeof(MainWindow), new PropertyMetadata(0, new PropertyChangedCallback((d, e) => { ((MainWindow)d).OnNumCorrectChanged(e); })));
        private static readonly DependencyProperty PosCorrectProperty = DependencyProperty.Register("PosCorrect", typeof(int), typeof(MainWindow), new PropertyMetadata(0, new PropertyChangedCallback((d, e) => { ((MainWindow)d).OnPosCorrectChanged(e); })));
        private static readonly DependencyProperty IsValueValidProperty = DependencyProperty.Register("IsValueValid", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));
        private static readonly DependencyProperty GuessInputProperty = DependencyProperty.Register("GuessInput", typeof(Guess), typeof(MainWindow), new PropertyMetadata(null));
        private static readonly DependencyProperty CanSetResponseProperty = DependencyProperty.Register("CanSetResponse", typeof(bool), typeof(MainWindow));

        private static readonly DependencyProperty IsNotLoggedinProperty = DependencyProperty.Register("IsNotLoggedin", typeof(bool), typeof(MainWindow), new PropertyMetadata(true));
        private static readonly DependencyProperty SelectedModeProperty = DependencyProperty.Register("SelectedMode", typeof(Type), typeof(MainWindow), new PropertyMetadata(typeof(MyPuzzle)));

        private static readonly DependencyProperty StatusTextProperty = DependencyProperty.Register("StatusText", typeof(string), typeof(MainWindow), new PropertyMetadata("就绪..."));
        private static readonly DependencyProperty RemainCountProperty = DependencyProperty.Register("RemainCount", typeof(int?), typeof(MainWindow), new PropertyMetadata(null));

        private static readonly DependencyProperty CandidateCollectionProperty = DependencyProperty.Register("CandidateCollection", typeof(ObservableCollection<Guess>), typeof(MainWindow), new PropertyMetadata(null));
        private static readonly DependencyProperty GuessesCollectionProperty = DependencyProperty.Register("GuessesCollection", typeof(ObservableCollection<Guess>), typeof(MainWindow), new PropertyMetadata(null));
        private static readonly DependencyProperty ResponsesCollectionProperty = DependencyProperty.Register("ResponsesCollection", typeof(ObservableCollection<Response>), typeof(MainWindow), new PropertyMetadata(null));
        private static readonly DependencyProperty WeightMatrixViewProperty = DependencyProperty.Register("WeightMatrixView", typeof(DataView), typeof(MainWindow), new PropertyMetadata(null));

        private static readonly DependencyProperty LogTextProperty = DependencyProperty.Register("LogText", typeof(string), typeof(MainWindow), new PropertyMetadata(null));

        #endregion

        #region Properties

        private PuzzleBase CurrentPuzzle
        {
            get
            {
                return (PuzzleBase)GetValue(CurrentPuzzleProperty);
            }
            set
            {
                SetValue(CurrentPuzzleProperty, value);
            }
        }
        private PuzzleSolverBase CurrentSolver
        {
            get
            {
                return (PuzzleSolverBase)GetValue(CurrentSolverProperty);
            }
            set
            {
                SetValue(CurrentSolverProperty, value);
            }
        }

        private int A
        {
            get
            {
                return (int)GetValue(AProperty);
            }
            set
            {
                SetValue(AProperty, value);
            }
        }
        private int B
        {
            get
            {
                return (int)GetValue(BProperty);
            }
            set
            {
                SetValue(BProperty, value);
            }
        }
        private int NumCorrect
        {
            get
            {
                return (int)GetValue(NumCorrectProperty);
            }
            set
            {
                SetValue(NumCorrectProperty, value);
            }
        }
        private int PosCorrect
        {
            get
            {
                return (int)GetValue(PosCorrectProperty);
            }
            set
            {
                SetValue(PosCorrectProperty, value);
            }
        }
        private bool IsValueValid
        {
            get
            {
                return (bool)GetValue(IsValueValidProperty);
            }
            set
            {
                SetValue(IsValueValidProperty, value);
            }
        }
        private Guess GuessInput
        {
            get
            {
                return (Guess)GetValue(GuessInputProperty);
            }
            set
            {
                SetValue(GuessInputProperty, value);
            }
        }
        private bool CanSetResponse
        {
            get
            {
                return (bool)GetValue(CanSetResponseProperty);
            }
            set
            {
                SetValue(CanSetResponseProperty, value);
            }
        }

        private bool IsNotLoggedin
        {
            get
            {
                return (bool)GetValue(IsNotLoggedinProperty);
            }
            set
            {
                SetValue(IsNotLoggedinProperty, value);
            }
        }
        private Type SelectedMode
        {
            get
            {
                return (Type)GetValue(SelectedModeProperty);
            }
            set
            {
                SetValue(SelectedModeProperty, value);
            }
        }
        
        private string StatusText
        {
            get
            {
                return (string)GetValue(StatusTextProperty);
            }
            set
            {
                SetValue(StatusTextProperty, value);
            }
        }
        private int? RemainCount
        {
            get
            {
                return (int?)GetValue(RemainCountProperty);
            }
            set
            {
                SetValue(RemainCountProperty, value);
            }
        }

        private ObservableCollection<Guess> CandidateCollection
        {
            get
            {
                return (ObservableCollection<Guess>)GetValue(CandidateCollectionProperty);
            }
            set
            {
                SetValue(CandidateCollectionProperty, value);
            }
        }
        private ObservableCollection<Guess> GuessesCollection
        {
            get
            {
                return (ObservableCollection<Guess>)GetValue(GuessesCollectionProperty);
            }
            set
            {
                SetValue(GuessesCollectionProperty, value);
            }
        }
        private ObservableCollection<Response> ResponsesCollection
        {
            get
            {
                return (ObservableCollection<Response>)GetValue(ResponsesCollectionProperty);
            }
            set
            {
                SetValue(ResponsesCollectionProperty, value);
            }
        }
        private DataView WeightMatrixView
        {
            get
            {
                return (DataView)GetValue(WeightMatrixViewProperty);
            }
            set
            {
                SetValue(WeightMatrixViewProperty, value);
            }
        }

        private string LogText
        {
            get
            {
                return logTextBuilder.ToString();
            }
        }

        #endregion

        #region OnChanged handlers

        private void OnAChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdateNumPos();
        }

        private void OnBChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdateNumPos();
        }
        
        private void OnNumCorrectChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdateAB();
        }
        
        private void OnPosCorrectChanged(DependencyPropertyChangedEventArgs e)
        {
            
            UpdateAB();
        }

        private void UpdateAB()
        {
            if (responseSyncing)
            {
                return;
            }
            responseSyncing = true;
            A = PosCorrect;
            B = NumCorrect - PosCorrect;
            responseSyncing = false;

            ValueVerification();
        }

        private void UpdateNumPos()
        {
            if (responseSyncing)
            {
                return;
            }
            responseSyncing = true;
            NumCorrect = A + B;
            PosCorrect = A;
            responseSyncing = false;

            ValueVerification();
        }

        private void ValueVerification()
        {
            IsValueValid = A + B <= 4 && PosCorrect <= NumCorrect;
        }

        private void OnCanSetResponseChanged(DependencyPropertyChangedEventArgs e)
        {
            CanSetResponse = (CurrentPuzzle as ISetExternalResponse) != null;
        }

        #endregion

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
        }

        #endregion

        #region Private methods

        private void LoadConfig()
        {
            if (File.Exists("cookies.dat"))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                using (FileStream fileStream = new FileStream("cookies.dat", FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    object obj = binaryFormatter.Deserialize(fileStream);
                    CookieCollection cookieCollection = (CookieCollection)obj;
                    userCookieCollection = cookieCollection;
                }
                IsNotLoggedin = false;
            }
        }

        private bool Login()
        {
            MoblieLoginWindow moblieLoginWindow = new MoblieLoginWindow(this);
            moblieLoginWindow.LoggedIn += MoblieLoginWindow_LoggedIn;
            moblieLoginWindow.Canceled += MoblieLoginWindow_Canceled;
            moblieLoginWindow.Owner = this;
            moblieLoginWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            return moblieLoginWindow.ShowDialog() == true;
        }

        private int Play(PuzzleBase puzzle, PuzzleSolverBase solver)
        {
            AppendLogAsync("-------- Game start --------\n");
            Dispatcher.Invoke(() =>
            {
                logTextBuilder = new StringBuilder();
                StatusText = "正在初始化...";
                GuessesCollection = new ObservableCollection<Guess>();
                ResponsesCollection = new ObservableCollection<Response>();
            }, DispatcherPriority.DataBind);

            int epoch = 0;
            while (!puzzle.IsSolved)
            {
                AppendLogAsync($"-------- Epoch {epoch} - Status --------\n");
                epoch++;

                AppendLogAsync(solver.FormatMatrix());
                AppendLogAsync(solver.FormatGuessCandidates());

                DataTable dataTable = new DataTable();
                for (int i = 0; i < solver.WeightMatrix.GetLength(1); i++)
                {
                    dataTable.Columns.Add(i.ToString(), typeof(int));
                }
                for (int i = 0; i < solver.WeightMatrix.GetLength(0); i++)
                {
                    DataRow dataRow = dataTable.NewRow();
                    for (int j = 0; j < solver.WeightMatrix.GetLength(1); j++)
                    {
                        dataRow[j] = solver.WeightMatrix[i, j];
                    }
                    dataTable.Rows.Add(dataRow);
                }

                Dispatcher.Invoke(() =>
                {
                    CandidateCollection = new ObservableCollection<Guess>(solver.GuessCandidates);
                    RemainCount = solver.GuessCandidates.Count;
                    WeightMatrixView = dataTable.DefaultView;
                }, DispatcherPriority.DataBind);

                Guess myguess = solver.NextGuess();

                AppendLogAsync($"-------- Epoch {epoch} - Guess --------\n");
                AppendLogAsync(myguess.ToString());
                AppendLogAsync("\n");
                Dispatcher.Invoke(() =>
                {
                    GuessesCollection.Add(myguess);
                    StatusText = "等待回复...";
                }, DispatcherPriority.DataBind);

                Response response = puzzle.Guess(myguess);

                Dispatcher.Invoke(() =>
                {
                    StatusText = "正在解析...";
                }, DispatcherPriority.DataBind);

                solver.SetResponse(response);

                AppendLogAsync($"-------- Epoch {epoch} - Response --------\n");
                AppendLogAsync(response.ToString());
                AppendLogAsync("\n");
                Dispatcher.Invoke(() =>
                {
                    ResponsesCollection.Add(response);
                }, DispatcherPriority.DataBind);
            }
            int result = solver.ReportGuessTimes();

            AppendLogAsync($"-------- Finished --------\n");
            Dispatcher.Invoke(() =>
            {
                StatusText = "完成";
                RemainCount = null;
                CandidateCollection = null;
            }, DispatcherPriority.DataBind);

            return result;
        }

        private Task AppendLogAsync(string msg)
        {
            logQueue.Enqueue(msg);

            if (pendingAppendLogTask == null)
            {
                pendingAppendLogTask = Task.Factory.StartNew(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        while (true)
                        {
                            string text = logQueue.Dequeue();
                            logTextBuilder.Append(text);
                            if (logQueue.Count == 0)
                            {
                                pendingAppendLogTask = null;
                                break;
                            }
                        }
                        SetValue(LogTextProperty, LogText);
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LogText"));
                    }, DispatcherPriority.Loaded);
                });
            }

            return pendingAppendLogTask;
        }

        private void StartGame()
        {
            // Abort on going game (if exists)
            if (workingThread != null)
            {
                workingThread.Abort();
                while(workingThread.ThreadState == ThreadState.Running) { Thread.Sleep(1); }
            }

            // Create a game instance
            Type gameType = SelectedMode;
            PuzzleBase puzzle = (PuzzleBase)Activator.CreateInstance(gameType);

            // For login required
            ILoginRequired loginRequired = puzzle as ILoginRequired;
            if (loginRequired != null)
            {
                if (userCookieCollection == null)
                {
                    if (!Login())
                    {
                        // Login canceled
                        return;
                    }
                }
                loginRequired.SetPlayerCookies(userCookieCollection);
            }

            PuzzleSolverBase solver = new MyPuzzleSolver();

            // Start the game
            Thread thread = new Thread(() =>
            {
                try
                {
                    Play(puzzle, solver);
                }
                catch (Exception ex)
                {
                    if(ex.GetType() != typeof(ThreadAbortException))
                    {
                        if (ex.GetType() == typeof(MyPuzzleSolver.NoSolutionException))
                        {
                            MessageBox.Show($"游戏解析发生异常，游戏终止。\n\n无法找到合适的解。", "游戏解析发生异常", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.None);
                        }
                        else
                        {
                            MessageBox.Show($"游戏解析发生异常，游戏终止。\n\n{ex.Message}\n\n{ex.StackTrace}", "游戏解析发生异常", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.None);
                        }
                    }

                    Dispatcher.Invoke(() =>
                    {
                        StatusText = "终止";
                        RemainCount = null;
                        CandidateCollection = null;

                        AppendLogAsync($"-------- Terminated --------");
                    }, DispatcherPriority.DataBind);
                }

                Dispatcher.Invoke(() =>
                {
                    CurrentPuzzle = null;
                });
            })
            {
                IsBackground = true
            };
            thread.Start();
            workingThread = thread;

            CurrentPuzzle = puzzle;
            CurrentSolver = solver;
        }

        private void AppendRule()
        {
            if (!IsValueValid)
            {
                MessageBox.Show($"数值无效。", "数值无效", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.None);
                return;
            }
            Guess guess = GuessInput;
            if (guess == null)
            {
                MessageBox.Show($"输入无效。", "输入无效", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.None);
                return;
            }
            Response result = new Response(A, B);

            PuzzleSolverBase solver = CurrentSolver;
            try
            {
                solver.ApplyRule(guess, result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"输入无效。", "输入无效", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.None);
            }

            AppendLogAsync($"-------- Manually Apply - Guess --------\n");
            AppendLogAsync(guess.ToString());
            AppendLogAsync("\n");

            GuessesCollection.Insert(GuessesCollection.Count - 1, guess);

            AppendLogAsync($"-------- Manually Apply - Response --------\n");
            AppendLogAsync(result.ToString());
            AppendLogAsync("\n");

            ResponsesCollection.Add(result);

            AppendLogAsync($"-------- Manually Apply - Status --------\n");

            AppendLogAsync(solver.FormatMatrix());
            AppendLogAsync(solver.FormatGuessCandidates());

            DataTable dataTable = new DataTable();
            for (int i = 0; i < solver.WeightMatrix.GetLength(1); i++)
            {
                dataTable.Columns.Add(i.ToString(), typeof(int));
            }
            for (int i = 0; i < solver.WeightMatrix.GetLength(0); i++)
            {
                DataRow dataRow = dataTable.NewRow();
                for (int j = 0; j < solver.WeightMatrix.GetLength(1); j++)
                {
                    dataRow[j] = solver.WeightMatrix[i, j];
                }
                dataTable.Rows.Add(dataRow);
            }

            CandidateCollection = new ObservableCollection<Guess>(solver.GuessCandidates);
            RemainCount = solver.GuessCandidates.Count;
            WeightMatrixView = dataTable.DefaultView;
        }

        #endregion

        #region Login event handlers

        private void MoblieLoginWindow_Canceled(MoblieLoginWindow sender)
        {
            
        }

        private void MoblieLoginWindow_LoggedIn(MoblieLoginWindow sender, CookieCollection cookies, uint uid)
        {
            userCookieCollection = cookies;
            IsNotLoggedin = false;

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (FileStream fileStream = new FileStream("cookies.dat", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                binaryFormatter.Serialize(fileStream, cookies);
            }
        }

        #endregion

        #region UI event handlers

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadConfig();
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            Login();
        }
       
        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        private void ComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            int num = (int)e.Key - (int)Key.D0;
            if (e.Key == Key.Escape)
            {
                num = 0;
            }
            if (num < 0 || num > 10)
            {
                return;
            }
            ((ComboBox)sender).SelectedIndex = num;
        }

        private void ResponseBtn_Click(object sender, RoutedEventArgs e)
        {
            ISetExternalResponse setResponse = CurrentPuzzle as ISetExternalResponse;
            if (setResponse != null)
            {
                if (!IsValueValid)
                {
                    MessageBox.Show($"数值无效。", "数值无效", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.None);
                    return;
                }
                setResponse.SetExternalResponse(A, B);
            }
        }

        private void AppendRuleBtn_Click(object sender, RoutedEventArgs e)
        {
            AppendRule();
        }

        private void LogBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).ScrollToEnd();
        }

        #endregion

    }
}
