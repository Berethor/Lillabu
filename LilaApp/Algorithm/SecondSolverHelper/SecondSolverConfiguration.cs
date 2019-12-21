namespace LilaApp.Algorithm.SecondSolverHelper
{
    public class SecondSolverConfiguration : IConfiguration
    {
        /// <summary>
        /// Количество кластеров
        /// </summary>
        public int ClusterCount { get; set; }

        /// <summary>
        /// Максимальное количество итераций
        /// </summary>
        public int IterationCount { get; set; }

        /// <summary>
        /// Включить / выключить разбиение на кластеры
        /// </summary>
        public bool EnableClusters { get; set; }

        #region Implementation of IConfiguration

        /// <summary>
        /// Название файла конфигурации
        /// </summary>
        public string FileName => "second_solver_config.txt";

        #endregion

        public static SecondSolverConfiguration Default = new SecondSolverConfiguration()
        {
            ClusterCount = 16,
            IterationCount = 10000,
            EnableClusters = true,
        };
    }
}
