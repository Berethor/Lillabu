using System;
using System.Collections.Generic;
using System.Text;

namespace LilaApp.Algorithm
{
    public interface IDrawableContextProvider
    {
        DrawableContext Context { get; }
    }

    public class DrawableContext
    {
        public string BotsRating { get; set; }
    }
}
