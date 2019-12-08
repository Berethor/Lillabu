using System;
using System.IO;
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
        /// Событие изменения входных данных
        /// </summary>
        public event EventHandler<string> InputChanged;

        /// <summary>
        /// Путь к файлу для сохранения последнего файла
        /// </summary>
        private const string LastFileName = "last_file.txt";

        /// <summary>
        /// Открыть последний файл
        /// </summary>
        public void OpenLastFile()
        {
            // Открываем последний файл
            if (File.Exists(LastFileName))
            {
                FilePath = File.ReadAllText(LastFileName);
                InputText = File.ReadAllText(FilePath);

                InputChanged?.Invoke(this, InputText);
            }
        }

        /// <summary>
        /// Конструктор по-умолчанию
        /// </summary>
        public FileLoaderViewModel()
        {
            BrowseCommand = new BaseCommand(() =>
            {
                var openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    FilePath = openFileDialog.FileName;
                    InputText = File.ReadAllText(FilePath);

                    // Запоминаем последний открытый файл
                    File.WriteAllText(LastFileName, FilePath);

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
}
