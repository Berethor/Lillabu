using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LilaApp.Models;
using LilaApp.Models.Railways;

namespace LilaApp.Algorithm
{
    /// <summary>
    /// Фабрика блоков железной дороги
    /// </summary>
    public class RailwayFactory
    {
        public static RailwayFactory Default { get; } = new RailwayFactory();

        /// <summary>
        /// Произвести шаблон блоков железной дороги по чертежу
        /// </summary>
        /// <param name="blueprint">Чертёж, в текстовом виде</param>
        /// <param name="model">Модель данных, для подсчета количества доступных блоков</param>
        /// <returns></returns>
        public RailwayChain BuildTemplate(string blueprint, Model model)
        {
            if (TryBuildTemplate(out var chain, out var error, blueprint, model))
            {
                return chain;
            }

            return null;
        }

        public bool TryBuildTemplate(out RailwayChain chain, out string error, string blueprint, Model model)
        {
            error = null;

            var regex = new Regex("((?'type'L|T|t|B)(?'lenght'\\d+))");

            var railways = new List<Railway>();

            foreach (Match match in regex.Matches(blueprint))
            {
                var type = match.Groups["type"].Value;
                var length = int.Parse(match.Groups["lenght"].Value);

                switch (type)
                {
                    case "L":
                        {
                            for (var k = 4; k > 0; k--)
                            {
                                var blocks = model?.Blocks.FirstOrDefault(_ => _.Name == $"L{k}");
                                while (blocks?.Count > 0 && length >= k)
                                {
                                    length -= k;
                                    blocks.Count--;
                                    var railway =
                                        k == 4 ? Railway.L4 :
                                        k == 3 ? Railway.L3 :
                                        k == 2 ? Railway.L2 :
                                        Railway.L1;
                                    railways.Add(railway);
                                }
                            }

                            break;
                        }
                    case "T":
                    case "t":
                        {
                            var amount = 0;
                            switch (length)
                            {
                                case 1:
                                    amount = 8;
                                    break; // T1 = 8 x T8
                                case 2:
                                    amount = 4;
                                    break; // T2 = 4 x T8
                                case 4:
                                    amount = 2;
                                    break; // T4 = 2 x T8
                                case 8:
                                    amount = 1;
                                    break; // T8 = 1 x T8
                                default:
                                    error = $"Некорректная длина блока {type}{length} в шаблоне {blueprint}";
                                    chain = null;
                                    return false;
                            }

                            var blocks = model?.Blocks.FirstOrDefault(_ => _.Name == "T4");
                            while (blocks?.Count > 0 && amount > 0)
                            {
                                amount -= 2;
                                blocks.Count--;
                                railways.Add(type == "T" ? Railway.T4R : Railway.T4L);
                            }

                            blocks = model?.Blocks.FirstOrDefault(_ => _.Name == "T8");
                            while (blocks?.Count > 0 && amount > 0)
                            {
                                amount -= 1;
                                blocks.Count--;
                                railways.Add(type == "T" ? Railway.T8R : Railway.T8L);
                            }

                            if (amount == 0)
                            {
                                length = 0;
                            }

                            break;
                        }
                    case "B":
                        {
                            if (length != 1)
                                throw new ArgumentException(
                                    $"Некорректная длина блока {type}{length} в шаблоне {blueprint}");

                            var blocks = model?.Blocks.FirstOrDefault(_ => _.Name == "B1");
                            if (blocks?.Count > 0)
                            {
                                length--;
                                blocks.Count--;
                                railways.Add(Railway.B1);
                            }

                            break;
                        }
                }

                if (length != 0)
                {
                    error = $"Недостаточно блоков для производства {match.Value} в шаблоне {blueprint}";
                    chain = null;
                    return false;
                }
            }

            chain = new RailwayChain(railways.Cast<IRailwayTemplate>().ToArray());

            // Создаём автоматические связи симметрии
            // TODO переделать
            var directions = chain.GetDirections();
            if (directions.ContainsKey(Direction.N) && directions.ContainsKey(Direction.S))
            {
                foreach (var block in directions[Direction.N])
                    block.Symmetric = directions[Direction.S].First();
            }
            if (directions.ContainsKey(Direction.W) && directions.ContainsKey(Direction.E))
            {
                foreach (var block in directions[Direction.W])
                    block.Symmetric = directions[Direction.E].First();
            }

            return true;
        }
    }
}
