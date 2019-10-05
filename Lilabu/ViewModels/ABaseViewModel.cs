using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Lilabu.ViewModels
{
    /// <summary>
    /// Базовый класс ViewModel с реализацией INotifyPropertyChanged и словарём приватных свойств
    /// </summary>
    /// <example>
    /// public class MyViewModel: ABaseViewModel
    /// {
    ///     public int MyProperty { get => Get⟨int⟩(); set => Set(value); }
    /// }
    /// </example>  
    public abstract class ABaseViewModel : INotifyPropertyChanged
    {
        // Эта строчка из шаблона Xamarin'а, может пригодится когда-нибудь:
        // public IDataStore<Item> DataStore => DependencyService.Get<IDataStore<Item>>() ?? new MockDataStore();

        /// <summary>
        /// Словарь значений приватных свойств
        /// </summary>
        private Dictionary<string, object> _values = new Dictionary<string, object>();

        /// <summary>
        /// Установить значение свойства с вызовом PropertyChanged
        /// </summary>
        /// <typeparam name="T">Тип свойства</typeparam>
        /// <param name="value">Новое значение</param>
        /// <param name="propertyName">Название свойства (извлекается автоматически)</param>
        protected void Set<T>(T value, [CallerMemberName]string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName)) return;

            // Если словарь уже содержит данное поле
            if (_values.ContainsKey(propertyName))
                // Обновляем значение поля
                _values[propertyName] = value;
            else
                // Добавляем зачение поля
                _values.Add(propertyName, value);

            // Уведомляем подписчиков
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // Уведомляем об изменении всех свойств, которые зависят от этого поля
            foreach (var member in GetType().GetProperties())
            {
                var attributes = member.GetCustomAttributes(typeof(DependsOnAttribute), true);
                if (attributes.Length == 0) continue;
                var dependsOn = attributes.First() as DependsOnAttribute;
                if (dependsOn?.DependsOn.Contains(propertyName) == true)
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(member.Name));
            }

        }

        /// <summary>
        /// Получить значение свойства
        /// </summary>
        /// <typeparam name="T">Тип свойства</typeparam>
        /// <param name="propertyName">Название свойства (извлекается автоматически)</param>
        /// <returns>Значение свойства</returns>
        protected T Get<T>(T defaultValue = default, [CallerMemberName]string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName) && _values.ContainsKey(propertyName))
                return (T)_values[propertyName];
            else
                return defaultValue;
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }

    /// <summary>
    /// Атрибут свойства, указывающий на зависимость от других свойств
    /// </summary>
    public class DependsOnAttribute : Attribute
    {
        /// <summary>
        /// Список свойств, от которых зависит данное поле
        /// </summary>
        public List<string> DependsOn { get; set; } = new List<string>();

        /// <summary>
        /// Атрибут зависимости от других свойств
        /// При обновлении любого из свойств, которые указаны в <paramref name="properties"/> у данного поля тоже будет вызван PropertyChanged
        /// </summary>
        /// <param name="properties">Свойства от которых зависит данное поле. Используйте nameof(...)</param>
        public DependsOnAttribute(params string[] properties)
        {
            DependsOn.AddRange(properties);
        }
    }
}
