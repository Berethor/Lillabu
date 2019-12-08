using System;
using System.IO;
using LilaApp;
using LilaApp.Generator;
using Microsoft.Win32;

namespace Lilabu.ViewModels
{
    public class FileLoaderViewModel : ABaseViewModel
    {
        /// <summary>
        /// Путь к файлу
        /// </summary>
        public string FilePath { get => Get<string>(); set => Set(value); }

        /// <summary>
        /// Содержимое файла
        /// </summary>
        public string InputText { get => Get<string>(); set => Set(value); }

        /// <summary>
        /// Команда выбора файла
        /// </summary>
        public BaseCommand BrowseCommand { get; }

        /// <summary>
        /// Команда сохранения
        /// </summary>
        public BaseCommand SaveCommand { get; }

        /// <summary>
        /// Параметры генератора
        /// </summary>
        public GeneratorViewModel GeneratorVM { get; }

        /// <summary>
        /// Событие изменения входных данных
        /// </summary>
        public event EventHandler<string> InputChanged;

        /// <summary>
        /// Конфигурация
        /// </summary>
        private FileLoaderConfiguration Configuration { get; }

        /// <summary>
        /// Открыть последний файл
        /// </summary>
        public void OpenLastFile()
        {
            var lastFileName = Configuration?.LastFile;

            // Открываем последний файл
            if (!string.IsNullOrEmpty(lastFileName))
            {
                FilePath = lastFileName;
                InputText = File.ReadAllText(FilePath);

                InputChanged?.Invoke(this, InputText);
            }
        }

        /// <summary>
        /// Конструктор по-умолчанию
        /// </summary>
        public FileLoaderViewModel()
        {
            Configuration = new FileLoaderConfiguration().Load();

            var generatorConfiguration = GeneratorConfiguration.Default.Load();
            GeneratorVM = GeneratorViewModel.FromConfiguration(generatorConfiguration);

            GeneratorVM.OnGeneration += (_, configuration) =>
            {
                var model = new Generator(configuration).CreateInputData();
                
                InputText = model.Serialize();

                InputChanged?.Invoke(this, InputText);
            };

            BrowseCommand = new BaseCommand(() =>
            {
                var openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    FilePath = openFileDialog.FileName;
                    InputText = File.ReadAllText(FilePath);

                    // Запоминаем последний открытый файл
                    Configuration.LastFile = FilePath;
                    Configuration.Save();

                    InputChanged?.Invoke(this, InputText);
                }
            });
            
            SaveCommand = new BaseCommand(() =>
            {
                if (!string.IsNullOrEmpty(FilePath))
                {
                    File.WriteAllText(FilePath, InputText);
                }

                InputChanged?.Invoke(this, InputText);
            });
        }
    }

    [Serializable]
    public class FileLoaderConfiguration : IConfiguration
    {
        #region Properties

        /// <summary>
        /// Путь к файлу для сохранения последнего файла
        /// </summary>
        public string LastFile { get; set; }

        #endregion

        #region Implementation of IConfiguration

        /// <summary>
        /// Название файла конфигурации
        /// </summary>
        public string FileName => "last_file.xml";

        #endregion
    }
}
