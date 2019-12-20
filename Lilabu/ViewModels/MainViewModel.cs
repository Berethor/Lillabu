using LilaApp;
using LilaApp.Algorithm;
using LilaApp.Models;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LilaApp.Algorithm.Genetic;
using LilaApp.Models.Railways;

namespace Lilabu.ViewModels
{
    public class MainViewModel : ABaseViewModel
    {
        #region Properties

        public string Version { get => Get<string>(); set => Set(value); }

        /// <summary>
        /// Заголовок
        /// </summary>
        public string Title { get => Get<string>(); set => Set(value); }

        /// <summary>
        /// Текст вывода
        /// </summary>
        public string Output { get => Get<string>(); set => Set(value); }

        /// <summary>
        /// View-Model загрузки файла исходных данных
        /// </summary>
        public FileLoaderViewModel FileLoaderVm { get; } = new FileLoaderViewModel();

        /// <summary>
        /// View-Model карты пути
        /// </summary>
        public TraceMapViewModel TraceMapVm { get; } = new TraceMapViewModel();

        /// <summary>
        /// Исходная модель
        /// </summary>
        public Model Model { get; private set; }

        /// <summary>
        /// Алгоритмы решения задачи
        /// </summary>
        private readonly IFinalTaskSolver[] _solvers;

        private string _selectedSolver;

        /// <summary>
        /// Названия алгоритмов решения задачи
        /// </summary>
        public string[] Solvers => _solvers.Select(solver => solver.GetType().Name).ToArray();

        /// <summary>
        /// Выбранный алгоритм решения задачи
        /// </summary>
        public string SelectedSolver
        {
            get => _selectedSolver;
            set {

                // Останавливаем старый алгоритм
                var wasRunning = IsRunning;
                if (wasRunning) RunCommand?.Execute(null);

                _selectedSolver = value;
                Configuration.LastSolver = value;
                Configuration.Save();

                // Запускаем новый алгоритм
                if (wasRunning) RunCommand?.Execute(null);
            }
        }

        /// <summary>
        /// Команда запуска решения обратной задачи
        /// </summary>
        public BaseCommand RunCommand { get; }

        /// <summary>
        /// Запущен ли какой-либо алгоритм
        /// </summary>
        public bool IsRunning => _cts != null;

        /// <summary>
        /// Текст кнопки запуска / остановки
        /// </summary>
        public string RunText { get => Get<string>(); set => Set(value); }

        /// <summary>
        /// Отображать процесс решения
        /// </summary>
        public bool ShouldDraw
        {
            get => Get<bool>();
            set
            {
                Set(value);

                if (value && Model != null)
                {
                    DisplayModelStep(this, new FinalAnswer(Model, new DirectTaskSolver().Solve(Model)));
                }

                Configuration.DisableGui = !value;
                Configuration.Save();
            }
        }

        /// <summary>
        /// Конфигурация
        /// </summary>
        private MainConfiguration Configuration { get; }

        public JoystickViewModel Joystick { get; }

        public string InfoText { get => Get<string>(); set => Set(value); }

        #endregion

        /// <summary>
        /// Записать текст в консоль вывода
        /// </summary>
        /// <param name="text">Строка вывода</param>
        /// <param name="ignoreEmpty">Игнорировать пустые строки</param>
        public void WriteLine(string text, bool ignoreEmpty = true)
        {
            Output += text;

            if (!ignoreEmpty || !string.IsNullOrEmpty(text))
            {
                Output += "\r\n";
            }
        }

        /// <summary>
        /// Отобразить ответ
        /// </summary>
        /// <param name="answer"></param>
        private void DisplayAnswer(FinalAnswer answer)
        {
            Model = answer.Model;

            var trace = TraceBuilder.CalculateTrace(Model);
            WriteLine(string.Join("\r\n", trace.Exceptions.Select(error => error.Message)));

            // Проверяем на пересечения:
            var crosses = RailwayChain.FromModel(Model).FindCrosses();
            foreach (var cross in crosses)
            {
                trace.Exceptions.Add(new Exception($"Пересечение блоков в точке {cross}"));
                WriteLine($"Ошибка: Пересечение блоков в точке {cross}");
            }

            if (ShouldDraw)
            {
                TraceMapVm.Points = trace.Points;
            }

            WriteLine($"Прибыль с точек маршрута: {answer.Price.Income}");
            WriteLine($"Стоимость блоков: {answer.Price.Price}");
            WriteLine($"Итого: {answer.Price.Result}");

            if (trace.Exceptions.Any())
            {
                WriteLine($"Алгоритм {SelectedSolver} выдал некорректное решение. Рекомендуется провести отладку алгоритма");
            }
        }

        /// <summary>
        /// Отобразить ответ в UI-потоке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="answer"></param>
        public void DisplayModelStep(object sender, FinalAnswer answer)
        {
            Application.Current.Dispatcher?.Invoke((Action)(() =>
            {
                Output = string.Empty;

                if (ShouldDraw)
                {
                    FileLoaderVm.InputText = answer.Model.Serialize();

                    if (sender is IDrawableContextProvider contextProvider)
                    {
                        var context = contextProvider.Context;

                        InfoText = context.BotsRating;

                        TraceMapVm.Cursor1Point = context.Cursor1Point;
                        TraceMapVm.Cursor2Point = context.Cursor2Point;

                        WriteLine(context.ErrorMessage);
                    }
                }
                else
                {
                    FileLoaderVm.InputText = "Визуализация отключена";
                    WriteLine("Визуализация отключена");
                }

                DisplayAnswer(answer);

            }));

            if (ShouldDraw && sender != null && SelectedSolver != typeof(WasdFinalSolver).Name)
            {
                // Задержка отрисовки для анимации
                Thread.Sleep(10);
            }
        }

        private CancellationTokenSource _cts;

        /// <summary>
        ///  Конструктор по-умолчанию
        /// </summary>
        public MainViewModel(Window mainWindow = null)
        {
            Title = "Lilabu Application";
            Version = Assembly.GetEntryAssembly()?.GetName().Version.ToString();

            RunText = "Запустить";

            Joystick = new JoystickViewModel();
            Joystick.InsertTemplate = new BaseCommand(() =>
            {
                var window = new InputTemplateWindow
                {
                    Owner = mainWindow, 
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                window.OnResult += (_, text) =>
                {
                    var args = new JoystickEventArg(JoystickKey.InsertTemplate)
                    {
                        Template = text,
                    };

                    Joystick.InvokeKeyPress(Joystick, args);
                };

                window.Show();
                window.TextBoxTemplate.Focus();
            });

            Configuration = new MainConfiguration().Load();

            _solvers = new IFinalTaskSolver[]
            {
                new FirstFinalSolver(),
                new SecondFinalSolver(), 
                new ThirdFinalSolver(),
                new WasdFinalSolver(Joystick), 
                new GeneticFinalSolver(), 
                new AmplifierFinalSolver(), 
            };

            SelectedSolver = Configuration?.LastSolver ?? _solvers[1].GetType().Name;

            ShouldDraw = (!Configuration?.DisableGui) ?? true;

            var checker = new DirectTaskSolver();

            RunCommand = new BaseCommand(() =>
            {
                if (!IsRunning)
                {
                    var solver = _solvers.First(_ => _.GetType().Name == SelectedSolver);
                    solver.OnStepEvent += DisplayModelStep;

                    _cts = new CancellationTokenSource();
                    solver.Token = _cts.Token;

                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            var answer = solver.Solve(Model, checker);

                            DisplayModelStep(null, answer);

                            if (IsRunning) RunCommand?.Execute(null);
                        }
                        catch (OperationCanceledException)
                        {
                            solver.OnStepEvent -= DisplayModelStep;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }

                    }, _cts.Token);
                }
                else
                {
                    _cts.Cancel();
                    _cts.Dispose();
                    _cts = null;
                }

                RunText = IsRunning ? "Остановить" : "Запустить";
            });

            FileLoaderVm.InputChanged += (sender, inputContent) =>
            {
                try
                {
                    Output = string.Empty;

                    var (model, errors) = new Loader().CheckAndParse(inputContent);

                    Model = model;
                    WriteLine(string.Join("\r\n", errors.Select(error => error.Message)));

                    model.Distances = MathFunctions.GetDistanсeBetweenAllPoints(model.Points);

                    DisplayAnswer(new FinalAnswer(Model, new DirectTaskSolver().Solve(Model)));

                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.StackTrace, exception.Message);
                }
            };

            FileLoaderVm.OpenLastFile();
        }
    }

    [Serializable]
    public class MainConfiguration : IConfiguration
    {
        #region Properties

        /// <summary>
        /// Последний используемый алгоритм
        /// </summary>
        public string LastSolver { get; set; }

        /// <summary>
        /// Отображать ли маршрут
        /// </summary>
        public bool DisableGui { get; set; }

        #endregion

        #region Implementation of IConfiguration

        /// <summary>
        /// Название файла конфигурации
        /// </summary>
        public string FileName => "last_solver.xml";

        #endregion
    }
}
