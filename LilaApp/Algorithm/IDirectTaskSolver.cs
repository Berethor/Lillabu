using LilaApp.Models;

namespace LilaApp.Algorithm
{
    public interface IDirectTaskSolver
    {
        /// <summary>
        /// Решить прямую задачу
        /// </summary>
        /// <param name="model">Полная модель</param>
        /// <returns>Прибыль с точек, цена блоков маршрута, результат = прибыль - цена блоков</returns>
        TracePrice Solve(Model model);
    }
}
