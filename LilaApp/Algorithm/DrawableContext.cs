using System;
using System.Collections.Generic;
using System.Text;
using LilaApp.Models;

namespace LilaApp.Algorithm
{
    /// <summary>
    /// Интерфейс поставщика <seealso cref="DrawableContext">контекста</seealso> дополнительных данных для визуализации.
    ///
    /// </summary>
    public interface IDrawableContextProvider
    {
        DrawableContext Context { get; }
    }

    /// <summary>
    /// Контекст для любых дополнительных данных от алгоритма.
    /// <para>
    /// Сюда можно добавлять любые поля и отображать их в интерфейсе
    /// </para>
    /// </summary>
    public class DrawableContext
    {
        public string BotsRating { get; set; }

        public Point Cursor1Point { get; set; }

        public Point Cursor2Point { get; set; }

        public string ErrorMessage { get; set; }
    }
}
