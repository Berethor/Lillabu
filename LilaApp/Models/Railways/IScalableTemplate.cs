namespace LilaApp.Models.Railways
{
    /// <summary>
    /// Интерфейс масштабируемого шаблона.
    ///
    /// Под масштабированием подразумевается изменение размеров шаблона без явного порождения новых шаблонов
    /// с сохранением возможности дальнейшего масштабирования.
    /// При масштабировании рекомендовано использовать симметричную вставку.
    /// </summary>
    public interface IScalableTemplate
    {
        /// <summary>
        /// Есть ли возможность изменить размер шаблона
        /// в указанном или любом направлении
        /// </summary>
        /// <param name="angle">Направление, если null - то любое</param>
        bool CanScale(double? angle);

        /// <summary>
        /// Вызвать изменение размера
        /// в указанном или любом направлении
        /// </summary>
        /// <param name="angle">Направление, если null - то любое</param>
        /// <returns>True - если получилось, иначе - false</returns>
        bool TryScale(double? angle);
    }

    /// <summary>
    /// Интерфейс мутируемого шаблона.
    /// 
    /// Под мутацией подразумевается изменение шаблона путём частичной или полной замены шаблона на другой шаблон.
    /// При мутации возможно частичное нарушение симметричности и порождение нового шаблона
    /// </summary>
    public interface IMutableTemplate
    {
        /// <summary>
        /// Есть ли возможность применить мутацию к шаблону
        /// </summary>
        /// <param name="dimensions">Размеры шаблона, который хотим вставить. Если null - то любой</param>
        /// <returns></returns>
        bool CanMutate(TemplateDimensions dimensions = null);

        /// <summary>
        /// Применить мутацию
        /// </summary>
        /// <param name="template">Шаблон для вставки. Если null - то применить любую мутацию на своё усмотрение</param>
        /// <returns>True - если получилось, иначе - false</returns>
        bool TryMutate(IRailwayTemplate template = null);
    }
}
