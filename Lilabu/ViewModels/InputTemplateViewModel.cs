namespace Lilabu.ViewModels
{
    public class InputTemplateViewModel : ABaseViewModel
    {
        /// <summary>
        /// Текст шаблона
        /// </summary>
        public string InputText { get => Get<string>(); set => Set(value); }

        public BaseCommand Ok { get; set; }

        public BaseCommand Canсel { get; set; }

        public JoystickViewModel Joystick { get; set; }

        public InputTemplateViewModel()
        {
            InputText = "";
        }
    }
}
