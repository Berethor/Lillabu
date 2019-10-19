using System;
using System.IO;
using System.Linq;
using System.Windows;
using LilaApp;
using LilaApp.Algorithm;
using LilaApp.Models;

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

        #endregion

        /// <summary>
        ///  Конструктор по-умолчанию
        /// </summary>
        public MainViewModel()
        {
            Title = "Lilabu Application";

            FileLoaderVm.InputChanged += (sender, inputContent) =>
            {
                try
                {
                    var model = new Loader().Parse(inputContent);
                    var trace = TraceBuilder.CalculateTrace(model);

                    Model = model;

                    TraceMapVm.Points = trace.Points;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.StackTrace, exception.Message);
                }
            };

        }

    }
}
