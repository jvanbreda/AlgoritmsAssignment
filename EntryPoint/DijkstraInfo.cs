﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntryPoint {
    class DijkstraInfo {
        public bool visited;
        public double cost;
        public Vector2 previous;

        public DijkstraInfo(bool visited, double cost, Vector2 previous) {
            this.visited = visited;
            this.cost = cost;
            this.previous = previous;
        }
    }
}
