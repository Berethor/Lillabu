using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LilaApp.Models;
using LilaApp.Models.Railways;

namespace LilaApp.Algorithm.Genetic
{
    public class GeneticFinalSolver : IFinalTaskSolver, IDrawableContextProvider
    {
        #region Implementation of IFinalTaskSolver

        /// <summary>
        /// Решить обратную задачу
        /// </summary>
        /// <param name="model">Неполная модель исходных данных (только блоки DATA и ROUTE)</param> расшириться
        /// <param name="directTaskSolver">Алгоритм решения прямой задачи - для вычисления стоимости</param>
        /// <returns>Полная модель</returns> 
        public FinalAnswer Solve(Model model, IDirectTaskSolver directTaskSolver)
        {
            _model = Model.Copy(model);
            _checker = directTaskSolver ?? new DirectTaskSolver();

            Init();

            Start();

            return new FinalAnswer()
            {
                Model = model,
                Price = _checker.Solve(model),
            };
        }

        /// <summary>
        /// Токен отмены задачи
        /// </summary>
        public CancellationToken Token { get; set; }

        /// <summary>
        /// Событие для отображения каждого шага в процессе решения
        /// </summary>
        public event EventHandler<FinalAnswer> OnStepEvent;

        #endregion

        void Init()
        {
            Best = new FinalAnswer(_model, _checker.Solve(_model));

            Bots = new List<Bot>();

            for (var i = 0; i < TotalCount; i++)
            {
                var bot = new Bot(_model, Best.Price);

                for (var j = 0; j < bot.Dna.Length; j++)
                {
                    bot.Dna[j] = BotCommandExtension.GetRandomCommand();
                }

                Bots.Add(bot);
            }
        }

        public List<Bot> Bots { get; private set; } = new List<Bot>();

        private Model _model;

        private IDirectTaskSolver _checker;

        private readonly Random _random = new Random();

        private readonly RailwayFactory _factory = new RailwayFactory();

        private int TotalCount { get; } = 24;

        private int BestCount { get; } = 8;

        private int Generation { get; set; } = 0;

        private int Iteration { get; set; } = 0;

        public FinalAnswer Best { get; private set; }

        public void NextGeneration()
        {
            Bots.Sort((a, b) => a.Best.CompareTo(b.Best));

            var count = Math.Max(1, Math.Min(BestCount, Bots.Count));

            var bestBots = Bots.Take(count).ToArray();
            Bots.Clear();

            var i = 0;
            while (Bots.Count < TotalCount)
            {
                var child = new Bot(_model, _checker.Solve(_model))
                {
                    Dna = bestBots[i].Dna,
                };

                // Вероятностная мутация
                if (_random.Next(100) <= 35)
                {
                    // Количество ячеек мутации 
                    var mutationsCount = _random.Next(1, child.Dna.Length / 8);

                    for (var j = 0; j < mutationsCount; j++)
                    {
                        var k = _random.Next(child.Dna.Length);
                        child.Dna[k] = BotCommandExtension.GetRandomCommand();
                    }

                    child.IsMutant = true;
                }

                Bots.Add(child);

                if (++i >= bestBots.Length)
                {
                    i = 0;
                }
            }

            Generation++;
        }

        public void Start()
        {
            Iteration = 0;

            while (true)
            {
                if (Token.IsCancellationRequested)
                {
                    var model = Model.Copy(Best.Model);
                    model.Blocks.Clear();
                    model.Blocks.AddRange(_model.Blocks);

                    OnStepEvent?.Invoke(this, new FinalAnswer(model, _checker.Solve(model)));

                    throw new OperationCanceledException("Работа алгоритма остановлена в связи с отменой задачи");
                }

                if (Bots.Count(_ => !_.IsDead) == 0)
                {
                    NextGeneration();

                    continue;
                }

                for(var i = 0; i < Bots.Count; i++)
                {
                    var bot = Bots[i];

                    if (bot.IsDead)
                    {
                        continue;
                    }

                    ExecuteCommand(bot);

                    bot.Cur++;

                    var answer = bot.Root.ConvertToModel(bot.Current.Model);
                    var price = _checker.Solve(answer);
                    var trace = TraceBuilder.CalculateTrace(answer);

                    if (trace.Exceptions.Any() || 
                        price.Result < -1000) // TODO: const MIN_PRICE
                    {
                        bot.IsDead = true;
                    }

                    if (Math.Abs(price.Result - bot.Current.Price.Result) < Constants.Precision)
                    {
                        bot.DeadLoops++;

                        if (bot.DeadLoops > 100)
                        {
                            bot.IsDead = true;
                        }
                    }
                    else
                    {
                        bot.DeadLoops = 0;
                    }

                    bot.Current = new FinalAnswer(answer, price);

                    //OnStepEvent?.Invoke(this, bot.Current);

                    if (price > bot.Best.Price)
                    {
                        bot.Best = bot.Current;

                        if (price > Best.Price)
                        {
                            Best = bot.Best;
                            OnStepEvent?.Invoke(this, Best);
                        }

                    }
                }

                OnStepEvent?.Invoke(this, Best);

                Iteration++;
            }
        }

        private readonly string[] _lineBlocks = new[] { "L1", "L2", "L3", "L4" };

        void ExecuteCommand(Bot bot)
        {
            for (var k = 0; k < 10; k++) // 10 попыток
            {
                var command = bot.Dna[bot.Cur];

                if (command.IsScale()) //  Расширение
                {
                    var blueprint = command.ExtractRailwayType();

                    if (blueprint == null)
                    {
                        blueprint = _lineBlocks[_random.Next(_lineBlocks.Length)];
                    }

                    if (!_factory.TryBuildTemplate(out var template, out var error, blueprint, bot.Current.Model))
                    {
                        continue;
                    }

                    var direction = command.ExtractDirection() ?? Direction.S;

                    if (!bot.TryScale(direction, template))
                    {
                        template.ReturnBlocksToModel(bot.Current.Model);
                    }
                    else
                    {
                        return;
                    }
                }
                else if (command.IsMutate()) // Мутация
                {
                    var blueprint = RailwayTemplates.Library[_random.Next(RailwayTemplates.Library.Count)];

                    if (!_factory.TryBuildTemplate(out var template, out var error, blueprint, bot.Current.Model))
                    {
                        continue;
                    }

                    if (!bot.TryMutate(template))
                    {
                        template.ReturnBlocksToModel(bot.Current.Model);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    bot.Cur += (byte) command;

                    break;
                }
            }

            // TODO: возможно стоит иногда оставлять бота в живых
            bot.IsDead = true;
        }

        #region Implementation of IDrawableContextProvider

        public DrawableContext Context {
            get {
                var text = new StringBuilder($"Generation {Generation} (Iteration: {Iteration})\n");
                text.Append($"THE BEST ANSWER: {Best.Price.Result:F2}\n");

                var sumCur = 0.0;
                var sumBest = 0.0;
                for (var i = 0; i < Bots.Count; i++)
                {
                    var bot = Bots[i];
                    var dead = bot.IsDead ? 'D' : '-';
                    var mutant = bot.IsMutant ? 'M' : '-';
                    text.Append($"{i}.{dead}{mutant} {bot.Current.Price.Result:F2} Best: {bot.Best.Price.Result:F2} \n");

                    sumCur += bot.Current.Price.Result;
                    sumBest += bot.Best.Price.Result;
                }

                sumCur /= Bots.Count;
                sumBest /= Bots.Count;

                text.Append($"Average  current: {sumCur:F2}\n");
                text.Append($"Average the best: {sumBest:F2}\n");


                return new DrawableContext()
                {
                    BotsRating = text.ToString(),
                };
            }
        }
    }

    #endregion
}

