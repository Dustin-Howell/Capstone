﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Resources
{
    static class AnimationNames
    {
        public static String PegMove { get { return "move"; } }
    }

    static class Scenes
    {
        public static String MainPlayScene { get { return "Scene1"; } }
    }

    static class ElementNames
    {
        public static String BoardGroup { get { return "GameBoard"; } }
        public static String BoardSurface { get { return "boardSurface"; } }
        public static String RootScene { get { return "Scene1"; } }
    }

    static class Models
    {
        //Model Scales
        private static readonly Vector3 _pegScale = new Vector3(.25f, .25f, .25f);
        public static Vector3 PegScale { get { return _pegScale; } }
        private static readonly Vector3 _fireScale = new Vector3(.25f, .25f, .25f);
        public static Vector3 FireScale { get { return _fireScale; } }
        private static readonly Vector3 _iceScale = new Vector3(1f, 1f, 1f);
        public static Vector3 IceScale { get { return _iceScale; } }
        private static readonly Vector3 _possibleScale = new Vector3(.25f, .25f, .25f);
        public static Vector3 PossibleScale { get { return _possibleScale; } }

        //Model Names
        public static String FirePeg { get { return "Model/FirePeon/Peon"; } }
        public static String IcePeg { get { return "Model/IcePeon/Peon"; } }
        public static String PossiblePeg { get { return "Model/PossiblePeon/Peon"; } }
    }

    static class Textures
    {
        //Tiles
        public static String UncapturedTile { get { return "square"; } }
        public static String WhiteTile { get { return "Assets/Fire Tile Cropped"; } }
        public static String BlackTile { get { return "Assets/Ice Tile Cropped"; } }

        //The Board
        public static String GameBoard { get { return "Assets/MainBoard"; } }
        public static String FireCorner { get { return "Assets/Fire Corner"; } }
        public static String IceCorner { get { return "Assets/Ice Corner"; } }

        //Pegs
        public static String Default { get { return "Textures/Grid"; } }
        public static String FirePeg { get { return "Assets/Fire Peg"; } }
        public static String IcePeg { get { return "Assets/Ice Peg"; } }

        //SkyBox
        public static String SkyBox { get { return "Textures/SkyCubeMap"; } }
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
