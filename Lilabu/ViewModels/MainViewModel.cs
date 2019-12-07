using LilaApp;
using LilaApp.Algorithm;
using LilaApp.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Lilabu.ViewModels
{
    public class MainViewModel : ABaseViewModel
    {
        #region Properties

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
        /// Команда запуска решения обратной задачи
        /// </summary>
        public BaseCommand RunCommand { get; }

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
        /// Отрисовать модель и вывести её стоимость
        /// </summary>
        /// <param name="model"></param>
        private void DisplayModel(Model model)
        {
            Model = model;

            var trace = TraceBuilder.CalculateTrace(model);
            WriteLine(string.Join("\r\n", trace.Exceptions.Select(error => error.Message)));
            TraceMapVm.Points = trace.Points;

            var income = DirectTaskSolver.GetRoutePrice(model, trace.Points);

            WriteLine($"Прибыль с точек маршрута: {income}");
            WriteLine($"Стоимость блоков: {trace.Price}");
            WriteLine($"Итого: {income - trace.Price}");
        }

        /// <summary>
        ///  Конструктор по-умолчанию
        /// </summary>
        public MainViewModel()
        {
            Title = "Lilabu Application";

            RunCommand = new BaseCommand(() =>
            {
                var checker = new DirectTaskSolver();
                var solver = new ThirdFinalSolver();

                solver.OnStepEvent += (_, m) =>
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        Output = string.Empty;
                        DisplayModel(m);
                    }));

                    Thread.Sleep(100);
                };

                Task.Factory.StartNew(() =>
                {
                    var answer = solver.Solve(Model, checker);

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        Output = string.Empty;
                        DisplayModel(answer.Model);
                    }));
                });
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

                    DisplayModel(Model);

                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.StackTrace, exception.Message);
                }
            };

        }

    }
}
