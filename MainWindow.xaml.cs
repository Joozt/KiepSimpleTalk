using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace KiepSimpleTalk
{
    public partial class Main : Window
    {
        private const int INTERVAL = 2000;
        private const bool AUTORESTARTAFTERSELECTION = true;
        private const int FONTSIZEDEFAULT = 60;
        private const int FONTSIZETEXTBOX = 80;


        private System.Timers.Timer _Timer = new System.Timers.Timer(INTERVAL);
        private int _SelectedRow;
        private int _SelectedColumn;
        private bool _DoCellSelection;

        public Main()
        {
            InitializeComponent();
            _Timer.Elapsed += new System.Timers.ElapsedEventHandler(_Timer_Elapsed);
            this.FontSize = FONTSIZEDEFAULT;
            txtOutput.FontSize = FONTSIZETEXTBOX;
        }

        void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_DoCellSelection)
            {
                _SelectedColumn++;

                if (_SelectedRow == 2)
                {
                    _SelectedColumn++;
                }

                HighlightCell(_SelectedRow, _SelectedColumn);

                //Stop if all columns have been highlighted
                if (_SelectedColumn >= _Grid.ColumnDefinitions.Count)
                {
                    Reset();
                }
            }
            else
            {
                _SelectedRow++;
                HighlightRow(_SelectedRow);

                //Stop if all rows have been highlighted
                if (_SelectedRow >= _Grid.RowDefinitions.Count)
                {
                    Reset();
                }
            }
        }

        delegate void HighlightRowDelegate(int rowNumber);

        private void HighlightRow(int rowNumber)
        {
            if (this.Dispatcher.Thread == Thread.CurrentThread)
            {
                foreach (object child in _Grid.Children)
                {
                    if (child is DockPanel)
                    {
                        DockPanel dockPanel = child as DockPanel;

                        if (Grid.GetRow(dockPanel) == rowNumber)
                        {
                            // Frozen colors cannot be animated
                            if (dockPanel.Background.IsFrozen)
                            {
                                dockPanel.Background = dockPanel.Background.CloneCurrentValue();
                            }

                            ColorAnimation animation = new ColorAnimation(Colors.Transparent, Colors.White, new Duration(new System.TimeSpan(0, 0, 0, 0, 500)));
                            dockPanel.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                        }
                        else
                        {
                            dockPanel.Background = Brushes.Transparent;
                        }
                    }
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new HighlightRowDelegate(this.HighlightRow), rowNumber);
            }
        }

        delegate void HighlightCellDelegate(int rowNumber, int columnNumber);

        private void HighlightCell(int rowNumber, int columnNumber)
        {
            if (this.Dispatcher.Thread == Thread.CurrentThread)
            {
                foreach (object child in _Grid.Children)
                {
                    if (child is DockPanel)
                    {
                        DockPanel dockPanel = child as DockPanel;

                        if (Grid.GetRow(dockPanel) == rowNumber)
                        {
                            if (Grid.GetColumn(dockPanel) == columnNumber)
                            {
                                // Frozen colors cannot be animated
                                if (dockPanel.Background.IsFrozen)
                                {
                                    dockPanel.Background = dockPanel.Background.CloneCurrentValue();
                                }

                                ColorAnimation animation = new ColorAnimation(Colors.Transparent, Colors.White, new Duration(new System.TimeSpan(0, 0, 0, 0, 500)));
                                dockPanel.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                            }
                            else
                            {
                                dockPanel.Background = Brushes.Transparent;
                            }
                        }
                        else
                        {
                            dockPanel.Background = Brushes.Transparent;
                        }
                    }
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new HighlightCellDelegate(this.HighlightCell), rowNumber, columnNumber);
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }

            if (e.Key == Key.Space || e.Key == Key.Add)
            {
                KeyPress();
            }
        }

        private string GetSelection(int rowNumber, int columnNumber)
        {
            string result = "";
            foreach (object child in _Grid.Children)
            {
                if (child is DockPanel)
                {
                    DockPanel dockPanel = child as DockPanel;

                    if (Grid.GetRow(dockPanel) == rowNumber && Grid.GetColumn(dockPanel) == columnNumber)
                    {
                        Button button = dockPanel.Children[0] as Button;
                        if (button != null)
                        {
                            result = button.Content.ToString().ToUpper();
                            break;
                        }
                    }
                }
            }

            if (result.Equals("SPACE"))
            {
                result = "_";
            }

            if (result.Equals("CLEAR"))
            {
                txtOutput.Clear();
                result = "";
            }

            if (result.Equals("BACK"))
            {
                if (txtOutput.Text.Length >= 1)
                {
                    txtOutput.Text = txtOutput.Text.Remove(txtOutput.Text.Length - 1);
                }
                result = "";
            }

            return result;
        }

        private void Reset()
        {
            _DoCellSelection = false;
            _Timer.Stop();
            _SelectedRow = 2;
            _SelectedColumn = 0;
            HighlightCell(_Grid.RowDefinitions.Count, _Grid.ColumnDefinitions.Count);
        }

        private void KeyPress()
        {
            if (_Timer.Enabled)
            {
                if (_DoCellSelection)
                {
                    // Completed selection
                    _Timer.Stop();
                    txtOutput.AppendText(GetSelection(_SelectedRow, _SelectedColumn));
                    Reset();

                    if (AUTORESTARTAFTERSELECTION)
                    {
                        // Restart automatically
                        HighlightRow(2);
                        _Timer.Start();
                    }
                }
                else
                {
                    // Row is selected -> switch to cell selection mode
                    _Timer.Stop();
                    _DoCellSelection = true;
                    HighlightCell(_SelectedRow, 0);
                    _SelectedColumn = 0;
                    _Timer.Start();
                }
            }
            else
            {
                // Initialize selection
                Reset();
                HighlightRow(2);
                _Timer.Start();
            }
        }

        private void Window_Deactivated(object sender, System.EventArgs e)
        {
            this.Topmost = true;
            this.Activate();
        }
    }
}
