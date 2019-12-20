using System;
using System.Collections.Generic;
using System.Text;
using LilaApp.Models;
using LilaApp.Models.Railways;

namespace LilaApp.Algorithm.Genetic
{
    public enum BotCommand : byte
    {
        Idle = 0,

        Prev,
        Exit,
        Enter,
        Leave,

        //ScaleN,
        //ScaleS,
        //ScaleW,
        //ScaleE,

        ScaleL1N,
        //ScaleL2N,
        //ScaleL3N,
        //ScaleL4N,

        ScaleL1S,
        //ScaleL2S,
        //ScaleL3S,
        //ScaleL4S,

        ScaleL1W,
        //ScaleL2W,
        //ScaleL3W,
        //ScaleL4W,

        ScaleL1E,
        //ScaleL2E,
        //ScaleL3E,
        //ScaleL4E,

        Mutate,

        Goto1,
        Goto2,
        Goto3,
        Goto4,
        //Goto5,
        //Goto6,
        //Goto7,
        //Goto8,
    }

    public static class BotCommandExtension
    {
        private static T RandomEnumValue<T>()
        {
            var v = Enum.GetValues(typeof(T));
            return (T)v.GetValue(new Random().Next(v.Length));
        }

        public static BotCommand GetRandomCommand()
        {
            return RandomEnumValue<BotCommand>();
        }

        public static bool IsScale(this BotCommand command, Direction? direction = null, RailwayType? type = null)
        { 
            var cmd = command.ToString();

            var result = cmd.StartsWith("Scale");

            if (direction is Direction d)
            {
                result = result && cmd.EndsWith(d.ToString());
            }

            if (type is RailwayType t)
            {
                result = result && cmd.Contains(t.ToString());
            }

            return result;
        }

        public static string ExtractRailwayType(this BotCommand command)
        {
            if (!command.IsScale()) return null;

            var cmd = command.ToString();

            foreach (var type in new []{"L1", "L2", "L3", "L4"})
            {
                if (cmd.Contains(type)) return type;
            }

            return null;
        }

        public static Direction? ExtractDirection(this BotCommand command)
        {
            if (!command.IsScale()) return null;

            var cmd = command.ToString();

            if (cmd.Length <= 7) return null;

            var dir = cmd.Substring(7);

            if (Enum.TryParse(dir, out Direction direction))
            {
                return direction;
            }

            return null;
        }

        public static bool IsMutate(this BotCommand command)
        {
            var cmd = command.ToString();

            return cmd.StartsWith("Mutate");
        }

        public static bool IsGoto(this BotCommand command)
        {
            var cmd = command.ToString();

            return cmd.StartsWith("Goto");
        }

    }
}
