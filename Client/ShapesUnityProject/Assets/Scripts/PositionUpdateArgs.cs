using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class PositionUpdateArgs : EventArgs
    {
        public Vector3 Position { get; set; }
    }
}