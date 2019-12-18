using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        /// <param name="model">Неполная модель исходных данных (только блоки DATA и ROUTE)</param>
        /// <param name="directTaskSolver">Решатель прямой задачи - для вычисления стоимости</param>
        /// <returns>Полная модель (включая блоки ORDER и TOP)</returns>
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
        /// Событие для отрисовки каждого шага в процессе решения
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
                    bot.Dna[j] = _random.Next(bot.Dna.Length);
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
                    var k = _random.Next(child.Dna.Length);
                    child.Dna[k] = _random.Next(child.Dna.Length);
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

                for (var i = 0; i < Bots.Count; i++)
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

        void ExecuteCommand(Bot bot)
        {

            if (!_factory.TryBuildTemplate(out var l1, out var error, "L1", bot.Current.Model))
            {
                bot.IsDead = true;

                return;
            }

            for (var k = 0; k < 10; k++)
            {
                switch (bot.Dna[bot.Cur])
                {
                    //case 1: bot.Prev(); return;
                    //case 2: bot.Next(); return;
                    //case 3: bot.Enter(); return;
                    //case 4: bot.Exit(); return;
                    case 5: bot.TryScale(Direction.N, l1); return;
                    case 6: bot.TryScale(Direction.S, l1); return;
                    case 7: bot.TryScale(Direction.W, l1); return;
                    case 8: bot.TryScale(Direction.E, l1); return;

                    case 9:
                        var blueprint = RailwayTemplates.Library[_random.Next(RailwayTemplates.Library.Count)];
                        if (_factory.TryBuildTemplate(out var template, out error, blueprint, bot.Current.Model))
                        {
                            bot.TryMutate(template);
                        }
                        break;

                    default: bot.Cur = _random.Next(bot.Dna.Length); break;
                }
            }
        }

        #region Implementation of IDrawableContextProvider

        public DrawableContext Context {
            get {
                var text = new StringBuilder($"Generation {Generation} (Iter: {Iteration})\n");
                text.Append($"THE BEST ANSWER: {Best.Price.Result:F2}\n");

                var sumCur = 0.0;
                var sumBest = 0.0;
                for (var i = 0; i < Bots.Count; i++)
                {
                    var bot = Bots[i];
                    var dead = bot.IsDead ? 'D' : '-';
                    var mutant = bot.IsMutant ? 'M' : '-';
                    text.Append($"{i}.{mutant} {bot.Current.Price.Result:F2} Best: {bot.Best.Price.Result:F2} \n");

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

public class Bot
{
    private int _cur;

    public int Cur { get => _cur; set => _cur = Math.Max(0, Math.Min(Dna.Length - 1, value)); }

    public int DeadLoops { get; set; }

    public bool IsDead { get; set; }

    public bool IsMutant { get; set; }

    public int[] Dna { get; set; } = new int[16];

    public RailwayChain Root { get; set; }

    public IRailwayTemplate Pointer { get; set; }

    public Stack<IRailwayTemplate> ParentStack { get; } = new Stack<IRailwayTemplate>();

    public FinalAnswer Current { get; set; }

    public FinalAnswer Best { get; set; }

    public Bot(Model initial, TracePrice tracePrice)
    {
        Current = new FinalAnswer(Model.Copy(initial), tracePrice);
        Best = new FinalAnswer(Model.Copy(initial), tracePrice);

        Root = RailwayTemplates.CreateCircle(RailwayType.T4R, Current.Model);
        Pointer = Root;
    }

    public bool Prev()
    {
        if (Pointer.Prev != null)
        {
            Pointer = Pointer.Prev;
            
            return true;
        }

        return false;
    }

    public bool Next()
    {
        if (Pointer.Next != null)
        {
            Pointer = Pointer.Next;
            
            return true;
        }

        return false;
    }

    public bool Enter()
    {
        if (Pointer is RailwayChain chain)
        {
            ParentStack.Push(Pointer);
            Pointer = chain[0];
            
            return true;
        }

        return false;
    }

    public bool Exit()
    {
        if (ParentStack.Count > 0)
        {
            Pointer = ParentStack.Pop();
            
            return true;
        }

        return false;
    }

    public bool TryScale(Direction direction, IRailwayTemplate template = null)
    {
        if (Pointer is RailwayChain chain)
        {
            return chain.TryScale(direction, template);
        }

        return false;
    }

    public bool TryMutate(IRailwayTemplate template)
    {
        if (Pointer is RailwayChain chain)
        {
            return chain.TryMutate(template);
        }

        return false;
    }
}

