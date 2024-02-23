namespace Turnbind
{
    using System.Reactive.Disposables;
    using System.Windows;
    using Turnbind.Action;
    using Turnbind.Model;

    public partial class MainWindow : Window
    {

        readonly TurnSettings _settings = new();

        readonly TurnAction _turnAction = new();

        readonly InputAction _inputAction = new();

        readonly ProcessWindowAction _windowAction = new();

        public string? ProcessName { get => _windowAction.ProcessName; set => _windowAction.ProcessName = value; }


        public MainWindow()
        {
            InitializeComponent();

            CompositeDisposable disposables = [];
        }
    }
}