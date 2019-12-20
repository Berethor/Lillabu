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
        public IRailwayTemplate BuildTemplate(string blueprint, Model model)
        {
            if (TryBuildTemplate(out var chain, out var error, blueprint, model))
            {
                return chain;
            }

            return null;
        }

        public bool TryBuildTemplate(out IRailwayTemplate template, out string error, string blueprint, Model model)
        {
            // Копируем список блоков
            var blocks = model?.Blocks.Select(block => (Block)block.Clone()).ToList();

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
                                var lineBlocks = blocks?.FirstOrDefault(_ => _.Name == $"L{k}");
                                while (lineBlocks?.Count > 0 && length >= k)
                                {
                                    length -= k;
                                    lineBlocks.Count--;
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
                                    template = null;
                                    return false;
                            }

                            var t4Blocks = blocks?.FirstOrDefault(_ => _.Name == "T4");
                            var t8Blocks = blocks?.FirstOrDefault(_ => _.Name == "T8");

                            if (t8Blocks?.Price < t4Blocks?.Price)
                            {
                                while (t8Blocks?.Count > 0 && amount >= 1)
                                {
                                    if (t8Blocks?.Count == 1 && amount == 2) break;

                                    amount -= 1;
                                    t8Blocks.Count--;
                                    railways.Add(type == "T" ? Railway.T8R : Railway.T8L);
                                }
                            }
                            while (t4Blocks?.Count > 0 && amount >= 2)
                            {
                                amount -= 2;
                                t4Blocks.Count--;
                                railways.Add(type == "T" ? Railway.T4R : Railway.T4L);
                            }
                            while (t8Blocks?.Count > 0 && amount > 0)
                            {
                                amount -= 1;
                                t8Blocks.Count--;
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

                            var bridgeBlocks = blocks?.FirstOrDefault(_ => _.Name == "B1");
                            if (bridgeBlocks?.Count > 0)
                            {
                                length--;
                                bridgeBlocks.Count--;
                                railways.Add(Railway.B1);
                            }

                            break;
                        }
                }

                if (length != 0)
                {
                    error = $"Недостаточно блоков для производства {match.Value} в шаблоне {blueprint}";
                    template = null;
                    return false;
                }
            }

            var chain = new RailwayChain(railways.Cast<IRailwayTemplate>().ToArray());

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

            // Если произвести блок удалось
            // применяем изменение количества блоков
            model?.Blocks.Clear();
            model?.Blocks.AddRange(blocks);

            if (chain.GetRailways().Count == 1)
            {
                template = chain[0];
                return true;
            }

            template = chain;
            return true;
        }
    }
}
