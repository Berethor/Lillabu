using System;
using System.IO;
using System.Xml.Serialization;

namespace LilaApp
{
    /// <summary>
    /// Интерфейс конфигурации
    /// </summary>
    public interface IConfiguration
    {
        /// <summary>
        /// Название файла конфигурации
        /// </summary>
        string FileName { get; }
    }

    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Сохранение конфигурации в файл
        /// </summary>
        /// <param name="configuration"></param>
        public static void Save(this IConfiguration configuration)
        {
            var serializer = new XmlSerializer(configuration.GetType());

            using (var fileStream = new FileStream(configuration.FileName, FileMode.Create))
            {
                serializer.Serialize(fileStream, configuration);
            }
        }

        /// <summary>
        /// Загрузка конфигурации из файла
        /// </summary>
        /// <param name="configuration">Конфигурация по-умолчанию</param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static T Load<T>(this T configuration, string fileName = null) where T : IConfiguration
        {
            var serializer = new XmlSerializer(typeof(T));

            if (fileName == null)
            {
                fileName = configuration.FileName;
            }

            if (!File.Exists(fileName))
            {
                return configuration;
            }

            using (var fileStream = new FileStream(fileName, FileMode.Open))
            {
                var output = (T)serializer.Deserialize(fileStream);

                if (output == null)
                {
                    return configuration;
                }

                return output;
            }
        }
    }
}
