using Microsoft.Win32;
using System;
using System.IO;

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

                    InputChanged?.Invoke(this, InputText);
                }
            });

            SaveCommand = new BaseCommand(() =>
            {
                File.WriteAllText(FilePath, InputText);

                InputChanged?.Invoke(this, InputText);
            });
        }
    }
}
