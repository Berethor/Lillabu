using System;
using LilaApp.Models;

namespace LilaApp.Algorithm
{
    /// <summary>
    /// Интерфейс решения обратной задачи
    /// </summary>
    public interface IFinalTaskSolver
    {
        /// <summary>
        /// Решить обратную задачу
        /// </summary>
        /// <param name="model">Неполная модель исходных данных (только блоки DATA и ROUTE)</param>
        /// <param name="directTaskSolver">Решатель прямой задачи - для вычислеия стоимости</param>
        /// <returns>Полная модель (включая блоки ORDER и TOP)</returns>
        FinalAnswer Solve(Model model, IDirectTaskSolver directTaskSolver);

        /// <summary>
        /// Событие для отрисовки каждого шага в процессе решения
        /// </summary>
        event EventHandler<Model> OnStepEvent;
    }

}
