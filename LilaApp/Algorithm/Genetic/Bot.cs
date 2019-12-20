using System;
using System.Collections.Generic;
using LilaApp.Models;
using LilaApp.Models.Railways;

namespace LilaApp.Algorithm.Genetic
{
    public class Bot
    {
        private int _cur;

        public int Cur
        {
            get => _cur;
            set
            {
                _cur = value;
                while (_cur < Dna.Length) _cur += Dna.Length;
                if (_cur >= Dna.Length) _cur %= Dna.Length;
            }
        }

        public int DeadLoops { get; set; }

        public bool IsDead { get; set; }

        public bool IsMutant { get; set; }

        public BotCommand[] Dna { get; set; } = new BotCommand[16];

        public RailwayChain Root { get; set; }

        public IRailwayTemplate Pointer { get; set; }

        public Stack<IRailwayTemplate> ParentStack { get; } = new Stack<IRailwayTemplate>();

        public FinalAnswer Current { get; set; }

        public FinalAnswer Best { get; set; }

        public Bot(Model initial, TracePrice tracePrice)
        {
            Current = new FinalAnswer(Model.Copy(initial), tracePrice);
            Best = new FinalAnswer(Model.Copy(initial), tracePrice);

            var direction = new Random().Next(2) == 0 ? RailwayType.T4R: RailwayType.T4L;

            Root = RailwayTemplates.CreateCircle(direction, Current.Model);
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

}
