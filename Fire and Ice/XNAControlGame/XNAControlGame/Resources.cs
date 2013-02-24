using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Resources
{
    static class Scenes
    {
        public static String MainPlayScene { get { return "Scene1"; } }
    }

    static class Models
    {
        //Model Scales
        private static readonly Vector3 _pegScale = new Vector3( 5, 5, 5 );
        public static Vector3 PegScale { get { return _pegScale; } }

        //Model Names
        public static String PegModel { get { return "Model/sphere"; } }
    }

    static class Textures
    {
        //Tiles
        public static String UncapturedTile { get { return "square"; } }
        public static String WhiteTile { get { return "Assets/Fire Tile"; } }
        public static String BlackTile { get { return "Assets/Ice Tile"; } }

        //The Board
        public static String GameBoard { get { return "Assets/MainBoard"; } }

        //Pegs
        public static String Default { get { return "Textures/Grid"; } }
    }

    static class Cameras
    {
        public static String MainView { get { return "MainCamera"; } }
    }

    static class Board
    {
        public static String Name { get { return "TheBoard"; } }
    }
}
