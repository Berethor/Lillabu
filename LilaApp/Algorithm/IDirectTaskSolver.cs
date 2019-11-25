using LilaApp.Models;

namespace LilaApp.Algorithm
{
    public interface IDirectTaskSolver
    {
        /// <summary>
        /// Решить прямую задачу
        /// </summary>
        /// <param name="model">Полная модель</param>
        /// <returns>Цена маршрута</returns>
        double Solve(Model model);
    }
}
