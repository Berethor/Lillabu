using System.IO;
using System.Linq;
using LilaApp;
using LilaApp.Algorithm;

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

        #endregion

        /// <summary>
        ///  Конструктор по-умолчанию
        /// </summary>
        public MainViewModel()
        {
            Title = "Lilabu Application";

            FileLoaderVm.InputChanged += (sender, inputContent) =>
            {
                var model = new Loader().Parse(inputContent);
                var trace = TraceBuilder.CalculateTrace(model);

                TraceMapVm.Points = trace.Points;
            };

        }

    }
}
