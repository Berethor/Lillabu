using System;
using System.Threading;
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
        /// <param name="directTaskSolver">Алгоритм решения прямой задачи - для вычисления стоимости</param>
        /// <returns>Полная модель (включая блоки ORDER и TOP)</returns>
        FinalAnswer Solve(Model model, IDirectTaskSolver directTaskSolver);

        /// <summary>
        /// Токен отмены задачи
        /// </summary>
        CancellationToken Token { get; set; }

        /// <summary>
        /// Событие для визуализации каждого шага в процессе решения
        /// </summary>
        event EventHandler<FinalAnswer> OnStepEvent;
    }

}
