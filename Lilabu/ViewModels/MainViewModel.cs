namespace Lilabu.ViewModels
{
    public class MainViewModel: ABaseViewModel
    {
        #region Properties

        /// <summary>
        /// Заголовок
        /// </summary>
        public string Title { get => Get<string>(); set => Set(value); }

        #endregion

        /// <summary>
        ///  Конструктор по-умолчанию
        /// </summary>
        public MainViewModel()
        {
            Title = "Lilabu Application";
        }
    }
}
