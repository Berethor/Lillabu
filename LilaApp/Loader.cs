using LilaApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LilaApp
{
    public class Loader
    {
        /// <summary>
        /// Загрузка, разбор и проверка файла входных данных
        /// </summary>
        /// <param name="fileName">Путь к файлу</param>
        /// <returns>Преобразованная информация в объект данных</returns>
        public Model LoadAndParse(string fileName)
        {
            var text = File.ReadAllText(fileName);

            return Parse(text);
        }

        /// <summary>
        /// Разбор и проверка входных данных
        /// </summary>
        /// <param name="text">Содержимое входных данных</param>
        /// <returns>Преобразованная информация в объект данных</returns>
        /// <exception cref="FormatException">Исключение, в сообщении которого находится информация об ошибке</exception>
        public Model Parse(string text)
        {
            var (model, errors) = CheckAndParse(text);

            if (errors.Length > 0)
            {
                throw errors[0];
            }

            return model;
        }

        /// <summary>
        /// Разбор и проверка входных данных
        /// </summary>
        /// <param name="text">Содержимое входных данных</param>
        /// <returns>Преобразованная информация в объект данных и список ошибок</returns>
        /// <exception cref="FormatException">Исключение, в сообщении которого находится информация об ошибке</exception>
        public Tuple<Model, Exception[]> CheckAndParse(string text)
        {
            var exceptions = new List<Exception>();

            var model = new Model(MAX_ELEMENTS_RESTRICTION, MAX_POINTS_RESTRICTION);

            try
            {
                var lines = text.Replace("\r", "").Split('\n');

                var blockReached = new Dictionary<string, bool>();

                int elementsCount = 0;
                int pointsCount = 0;

                // Блок, который считывается в данный момент
                KeyValuePair<int, string>? currentBlock = null;
                int blockIndex = -1;

                // Был ли прочитанный блок пустым?
                bool isCurrentBlockNonEmpty = false;
                // Была ли встречена строка, закрывающая блок?
                bool isEndBlockReached = false;

                // Считываем файл построчно
                for (int lineNum = 1; lineNum <= lines.Length; lineNum++)
                {
                    var line = lines[lineNum - 1];

                    string exMessageTemplate = string.Format(EXCEPTION_TEMPLATE, lineNum, "{0}");

                    // Находим и удаляем комментарии в строке
                    var commentIndex = line.IndexOf(COMMENT_SIGN);
                    if (commentIndex != -1)
                    {
                        line = line.Remove(commentIndex);
                    }
                    line = line.Trim();

                    // Пропускаем строки-комментарии
                    if (string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    // Проверяем данную строку на объявление начала блока и их посследовательность
                    if ((blockIndex = _keyWords.IndexOf(line.ToUpper())) != -1)
                    {
                        // Первый вариант с последовательно расположенными блоками
                        // Может пригодится
                        //if ((currentBlock?.Key ?? -1) != (blockIndex - 1))
                        //{
                        //    throw new FormatException(string.Format(exMessageTemplate, $"{_keyWords[blockIndex]} block missing"));
                        //}

                        // Объявление нового блока данных без закрытия предыдущего
                        if ((currentBlock.HasValue) && (!isEndBlockReached))
                        {
                            exceptions.Add(new FormatException(string.Format(exMessageTemplate, $"{_keyWords[blockIndex - 1]} block should be closed with \"{END_BLOCK_SIGN}\" line")));
                        }

                        // Проверка на наличие данных
                        if (currentBlock.HasValue && !isCurrentBlockNonEmpty)
                        {
                            exceptions.Add(new FormatException(string.Format(exMessageTemplate, $"{_keyWords[blockIndex - 1]} block should not be empty")));
                        }

                        currentBlock = new KeyValuePair<int, string>(blockIndex, line.ToUpper());

                        isCurrentBlockNonEmpty = false;
                        isEndBlockReached = false;

                        if (!blockReached.ContainsKey(currentBlock.Value.Value))
                        {
                            blockReached.Add(currentBlock.Value.Value, true);
                        }

                        continue;
                    }

                    // Нет объявления блока
                    if (currentBlock == null)
                    {
                        exceptions.Add(new FormatException(string.Format(exMessageTemplate, $"Wrong block declaration. It is should be one of ({string.Join(",", _keyWords)})")));
                    }

                    if (line.Equals(END_BLOCK_SIGN))
                    {
                        // Добавляем координату 0-0, если она не является последней координатой
                        /*
                        if (model.Points.Count != 0)
                        {
                            var point = new Point(0, 0);
                            if (!model.Points.Last().Equals(point))
                            {
                                model.Points.Add(point);
                            }
                        }
                        */
                        isEndBlockReached = true;
                        continue;
                    }

                    // Разбираем сами данные
                    var data = line.Trim().Replace(".", ",").Split(DATA_SEPARATOR_BASIC, DATA_SEPARATOR_EXPAND);
                    var wrongArgcExTemplate = string.Format(exMessageTemplate, "Wrong number of input args for {0} block. Should be {1} but was {2}");
                    var wrongArgFormatExTemplate = string.Format(exMessageTemplate, "Argument has wrong format. {0}");

                    switch (currentBlock.Value.Value)
                    {
                        case DATA_KEY_WORD:
                            checkBlockLength(DATA_BLOCK_ARGC, data.Length, currentBlock.Value.Value, wrongArgcExTemplate);

                            var dataPointName = data[0];

                            checkPointName(dataPointName, wrongArgFormatExTemplate);

                            if (!int.TryParse(data[1], out int count))
                            {
                                exceptions.Add(new FormatException(string.Format(wrongArgFormatExTemplate, "The second argument should consist of integer numbers only")));
                            }

                            digitNotLessThan(count, 0, string.Format(wrongArgFormatExTemplate, "The second argument {0}"));

                            if (!double.TryParse(data[2], out double price))
                            {
                                exceptions.Add(new FormatException(string.Format(wrongArgFormatExTemplate, "The third argument should consist of numbers only")));
                            }

                            digitNotLessThan(price, 0, string.Format(wrongArgFormatExTemplate, "The third argument {0}"));

                            elementsCount += count;

                            if (elementsCount > MAX_ELEMENTS_RESTRICTION)
                            {
                                exceptions.Add(new FormatException(string.Format(wrongArgFormatExTemplate, $"Count of elements cannot be more than {MAX_ELEMENTS_RESTRICTION}")));
                            }

                            model.Blocks.Add(new Block(dataPointName, count, price));

                            if (_dataBlockCache.ContainsKey(dataPointName))
                            {
                                _dataBlockCache[dataPointName] += count;
                            }
                            else
                            {
                                _dataBlockCache.Add(dataPointName, count);
                            }

                            break;

                        case ROUTE_KEY_WORD:
                            checkBlockLength(ROUTE_BLOCK_ARGC, data.Length, currentBlock.Value.Value, wrongArgcExTemplate);

                            if (!double.TryParse(data[0], out double xCoor))
                            {
                                exceptions.Add(new FormatException(string.Format(wrongArgFormatExTemplate, "The \"X\" coordinate should be numeric")));
                            }

                            if (!double.TryParse(data[1], out double yCoor))
                            {
                                exceptions.Add(new FormatException(string.Format(wrongArgFormatExTemplate, "The \"Y\" coordinate should be numeric")));
                            }

                            if (!double.TryParse(data[2], out double pointPrice))
                            {
                                throw new FormatException(string.Format(wrongArgFormatExTemplate, "The price should be numeric"));
                            }

                            if ((model.Points.Count == 0) && (xCoor != 0) && (yCoor != 0))
                            {
                                exceptions.Add(new FormatException(string.Format(wrongArgFormatExTemplate, $"{ROUTE_KEY_WORD} block should starts with \"0 0\"")));
                            }

                            pointsCount++;

                            if (pointsCount > MAX_POINTS_RESTRICTION)
                            {
                                exceptions.Add(new FormatException(string.Format(wrongArgFormatExTemplate, $"Count of points cannot be more than {MAX_POINTS_RESTRICTION}")));
                            }

                            model.Points.Add(new Point(xCoor, yCoor, price: pointPrice));

                            break;

                        case ORDER_KEY_WORD:
                            checkBlockLength(ORDER_BLOCK_ARGC, data.Length, currentBlock.Value.Value, wrongArgcExTemplate);

                            var orderPointName = data[0];

                            checkPointName(orderPointName, wrongArgFormatExTemplate);

                            model.Order.Add(orderPointName);
                            _orderBlockCache.Add(lineNum, orderPointName);

                            break;

                        case TOP_KEY_WORD:
                            checkBlockLength(TOP_BLOCK_ARGC, data.Length, currentBlock.Value.Value, wrongArgcExTemplate);

                            if (!int.TryParse(data[0], out int firstBlock))
                            {
                                exceptions.Add(new FormatException(string.Format(wrongArgFormatExTemplate, "The first argument should consist of integer numbers only")));
                            }

                            digitNotLessThan(firstBlock, 0, string.Format(wrongArgFormatExTemplate, "The first argument {0}"));

                            if (!int.TryParse(data[1], out int secondBlock))
                            {
                                exceptions.Add(new FormatException(string.Format(wrongArgFormatExTemplate, $"The second argument should be numeric")));
                            }

                            digitNotLessThan(secondBlock, 0, string.Format(wrongArgFormatExTemplate, "The second argument {0}"));

                            if (!int.TryParse(data[2], out int direction))
                            {
                                exceptions.Add(new FormatException(string.Format(wrongArgFormatExTemplate, "The third argument should consist of integer numbers only")));
                            }

                            if (!_directions.Contains(direction))
                            {
                                exceptions.Add(new FormatException(string.Format(wrongArgFormatExTemplate, $"Direction should be one of ({string.Join(", ", _directions)})")));
                            }

                            var topology = new TopologyItem(firstBlock, secondBlock, direction);

                            model.Topology.Add(topology);
                            _topBlockCache.Add(lineNum, topology);

                            break;

                        // Этот код в теории должен быть недостижим
                        default:
                            exceptions.Add(new Exception(string.Format(exMessageTemplate, $"What a heck??? Line text: {line}")));
                            break;
                    }

                    isCurrentBlockNonEmpty = true;
                }

                string reachedBlockExMessage = string.Join(", ", blockReached.Where(block => !block.Value));

                if (!string.IsNullOrEmpty(reachedBlockExMessage))
                {
                    exceptions.Add(new FormatException($"Blocks missing: {reachedBlockExMessage}"));
                }

                // Создаём копию для работы
                var dataBlockCacheCopy = _dataBlockCache.ToDictionary(pair => pair.Key, pair => pair.Value);

                // Проверка блока ORDER на соответствие блока DATA
                foreach (var order in _orderBlockCache)
                {
                    var orderExTemplate = string.Format(EXCEPTION_TEMPLATE, order.Key, "{0}");

                    if (!dataBlockCacheCopy.ContainsKey(order.Value))
                    {
                        exceptions.Add(new FormatException(string.Format(orderExTemplate, $"{order.Value} element does not exists in {DATA_KEY_WORD} block")));
                    }

                    dataBlockCacheCopy[order.Value]--;

                    if (dataBlockCacheCopy[order.Value] < 0)
                    {
                        exceptions.Add(new FormatException(string.Format(orderExTemplate, $"Count of {order.Value} element cannot be more than specified in {DATA_KEY_WORD} block - {_dataBlockCache[order.Value]}")));
                    }
                }

                // Проверка блока TOP на соответствие блока DATA
                var topZeroCount = 0;
                foreach (var top in _topBlockCache)
                {
                    var routeExTemplate = string.Format(EXCEPTION_TEMPLATE, top.Key, "{0}");

                    if (top.Value.FirstBlock == 0 || top.Value.SecondBlock == 0)
                    {
                        topZeroCount++;

                        if (topZeroCount > REQUIRED_NUM_OF_ZEROS)
                        {
                            exceptions.Add(new FormatException(string.Format(routeExTemplate, $"Connection with zero element cannot be more than {REQUIRED_NUM_OF_ZEROS}")));
                        }
                    }
                    else
                    {
                        if (top.Value.FirstBlock > model.Order.Count)
                        {
                            exceptions.Add(new FormatException(string.Format(routeExTemplate, $"First arg element with {top.Value.FirstBlock} id does not exists in {ORDER_KEY_WORD} block")));
                        }

                        if (top.Value.SecondBlock > model.Order.Count)
                        {
                            exceptions.Add(new FormatException(string.Format(routeExTemplate, $"First arg element with {top.Value.SecondBlock} id does not exists in {ORDER_KEY_WORD} block")));
                        }

                        var blockName = model.Order[top.Value.SecondBlock - 1];

                        if (!_dataBlockCache.ContainsKey(blockName))
                        {
                            exceptions.Add(new FormatException(string.Format(routeExTemplate, $"{blockName} element does not exists in {DATA_KEY_WORD} block")));
                        }

                        _dataBlockCache[blockName]--;

                        if (_dataBlockCache[blockName] < 0)
                        {
                            exceptions.Add(new FormatException(string.Format(routeExTemplate, $"Count of {blockName} elements cannot be more than specified in {DATA_KEY_WORD} block - {_dataBlockCache[blockName]}")));
                        }
                    }
                }

                // Проверка, что все элементы из блока ORDER используются не больше двух (для Y - трёх) раз
                var orderElemetnsAvaliableConnections = model.Order.Select((item, index) => item.StartsWith("Y") ? 3 : 2).ToList();
                orderElemetnsAvaliableConnections.Insert(0, 2);
                foreach (var top in _topBlockCache)
                {
                    // Превышено допустимое количество использования блока
                    if (orderElemetnsAvaliableConnections[top.Value.FirstBlock]-- < 0)
                    {
                        exceptions.Add(new FormatException(string.Format(EXCEPTION_TEMPLATE, top.Key, $"Block №{top.Value.FirstBlock} usage exceeded")));
                    }
                    if (orderElemetnsAvaliableConnections[top.Value.SecondBlock]-- < 0)
                    {
                        exceptions.Add(new FormatException(string.Format(EXCEPTION_TEMPLATE, top.Key, $"Block №{top.Value.SecondBlock} usage exceeded")));
                    }
                }

                // Проверка, что все элементы из блока ORDER используются в блоке TOP
                var orderBlockKeys = _orderBlockCache.Keys.ToArray();
                for (int i = 0; i < model.Order.Count; i++)
                {
                    var orderLine = orderBlockKeys[i];
                    var element = _orderBlockCache[orderLine];
                    var usageCount = orderElemetnsAvaliableConnections[i + 1];

                    if (usageCount >= 2)
                    {
                        exceptions.Add(new FormatException(string.Format(EXCEPTION_TEMPLATE, orderLine, $"Element \"{element}\" (№{i + 1} in {ORDER_KEY_WORD} block) isn't used in {TOP_KEY_WORD} block")));
                    }
                    // У блока Y может быть 1 неиспользованное соединение, у остальных блоков - нет
                    else if (usageCount == 1 && !element.StartsWith("Y"))
                    {
                        exceptions.Add(new FormatException(string.Format(EXCEPTION_TEMPLATE, orderLine, $"Element \"{element}\" (№{i + 1} in {ORDER_KEY_WORD} block) has not enough connections. It used only {usageCount} time in {TOP_KEY_WORD} block")));
                    }
                }

                // Проверка количества соединений с нулевым элементом
                if (topZeroCount != REQUIRED_NUM_OF_ZEROS)
                {
                    exceptions.Add(new FormatException(string.Format(EXCEPTION_TEMPLATE, _topBlockCache.Keys.Last(), $"Connection with zero element should be equals to {REQUIRED_NUM_OF_ZEROS}")));
                }
            }
            catch (Exception exception)
            {
                exceptions.Add(exception);
            }
            finally
            {
                clearDicts();
            }

            return new Tuple<Model, Exception[]>(model, exceptions.ToArray());
        }

        private void digitNotLessThan(double digit, double num, string exTemplate)
        {
            if (digit < num)
            {
                throw new FormatException(string.Format(exTemplate, $"Arg should be more or equal than {num}"));
            }
        }

        private void checkPointName(string name, string exTemplate)
        {
            if (!_allowedElementsSymbols.IsMatch(name))
            {
                throw new FormatException(string.Format(exTemplate, $"The first arg should starts with one of {ALLOWED_CHARS} symbols and end with digits"));
            }
        }

        private void checkBlockLength(int requiredLength, int actualLength, string blockName, string exTemplate)
        {
            if (requiredLength != actualLength)
            {
                throw new FormatException(string.Format(exTemplate, blockName, requiredLength, actualLength));
            }
        }

        private void clearDicts()
        {
            _dataBlockCache.Clear();
            _orderBlockCache.Clear();
            _topBlockCache.Clear();

            GC.Collect();
        }

        #region Private Members

        private static readonly List<string> _keyWords = new List<string>()
        { DATA_KEY_WORD, ROUTE_KEY_WORD, ORDER_KEY_WORD, TOP_KEY_WORD };

        private static readonly List<int> _directions = new List<int>()
        { -1, 0, 1 };

        private static readonly Regex _allowedElementsSymbols = new Regex($"[{ALLOWED_CHARS}][0-9]+");

        // Ключ - имя типа элемента дороги
        // Значение - количество элементов типа
        private readonly Dictionary<string, int> _dataBlockCache = new Dictionary<string, int>(MAX_ELEMENTS_RESTRICTION);

        // Ключ - номер строки
        // Значение - элемент строки
        private readonly Dictionary<int, string> _orderBlockCache = new Dictionary<int, string>(MAX_ELEMENTS_RESTRICTION);
        private readonly Dictionary<int, TopologyItem> _topBlockCache = new Dictionary<int, TopologyItem>(MAX_ELEMENTS_RESTRICTION);

        #endregion Private Members

        #region Private Constants

        // Думаю, что это можно (но не обязательно) будет в конфиг файл завернуть
        private const string ALLOWED_CHARS = "LTYB";

        private const int REQUIRED_NUM_OF_ZEROS = 2;

        private const string DATA_KEY_WORD = "DATA";
        private const string ROUTE_KEY_WORD = "ROUTE";
        private const string ORDER_KEY_WORD = "ORDER";
        private const string TOP_KEY_WORD = "TOP";

        private const int DATA_BLOCK_ARGC = 3;
        private const int ROUTE_BLOCK_ARGC = 3;
        private const int ORDER_BLOCK_ARGC = 1;
        private const int TOP_BLOCK_ARGC = 3;

        private const string END_BLOCK_SIGN = "/";
        private const string COMMENT_SIGN = "--";
        private const char DATA_SEPARATOR_BASIC = ' ';
        private const char DATA_SEPARATOR_EXPAND = '\t';

        private const int MAX_ELEMENTS_RESTRICTION = 100000;
        private const int MAX_POINTS_RESTRICTION = 1000;

        private const string EXCEPTION_TEMPLATE = "Line {0}. Message: {1}";

        #endregion Private Constants
    }

}
