using System;

namespace LilaApp.Algorithm
{
    using Models;
    using Models.Railways;

    /// <summary>
    /// Третий вариант решения обратной задачи.
    /// Основан на вставке элементов в двусвязный список, а также
    /// замене нескольких элементов, объединённых в шаблон, на другой шаблон и т.д.
    /// </summary>
    public class ThirdFinalSolver : IFinalTaskSolver
    {
        #region Implementation of IFinalTaskSolver

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException" />
        public FinalAnswer Solve(Model model, IDirectTaskSolver directTaskSolver)
        {
            _model = model; // ?? throw new ArgumentNullException(nameof(model));

            // Создать стартовую трассу - кольцо
            var head = RailwayTemplates.CreateCircle(RailwayType.T4R);

            _answer = head.ConvertToModel(_model);
            OnStepEvent?.Invoke(this, _answer);

            var compass = RailwayTemplates.Compass(head);

            // Первый этап - расширяем кольцо вверх, вниз и в сторону
            const int count = 16;
            for (var i = 0; i < count; i++)
            {
                // Блок, к которому добавляем L1
                var dest = compass.W;

                // Расширение по сторонам
                if (i < count / 4) dest = compass.W;
                else if (i < 2 * count / 4) dest = compass.NW;
                else if (i < 3 * count / 4) dest = compass.N;
                else if (i < 4 * count / 4) dest = compass.NE;

                // Чередование расширений по сторонам
                //if (i % 4 == 0) dest = compass.W;
                //else if (i % 4 == 1) dest = compass.NW;
                //else if (i % 4 == 2) dest = compass.N;
                //else if (i % 4 == 3) dest = compass.NE;

                var chain = new RailwayChain(new[]
                {
                    new Railway(RailwayType.T8L),
                    new Railway(RailwayType.T8R),
                });
                //dest.AppendSymmetric(chain);

                dest.AppendSymmetric(new Railway(RailwayType.L1));

                _answer = head.ConvertToModel(_model);
                OnStepEvent?.Invoke(this, _answer);
            }

            return new FinalAnswer
            {
                Model = _answer,
                Price = directTaskSolver.Solve(_answer),
            };
        }

        /// <inheritdoc />
        public event EventHandler<Model> OnStepEvent;

        #endregion

        #region Fields 

        private Model _model;
        private Model _answer;

        #endregion
        
    }
}
