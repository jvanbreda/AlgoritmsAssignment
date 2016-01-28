using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntryPoint {
#if WINDOWS || LINUX
    public static class Program {
        static List<Vector2> specialBuildingsList;
        static List<List<Vector2>> result;
        [STAThread]
        static void Main() {
            var fullscreen = false;
            read_input:
            switch (Microsoft.VisualBasic.Interaction.InputBox("Which assignment shall run next? (1, 2, 3, 4, or q for quit)", "Choose assignment", VirtualCity.GetInitialValue())) {
                case "1":
                    using (var game = VirtualCity.RunAssignment1(SortSpecialBuildingsByDistance, fullscreen))
                        game.Run();
                    break;
                case "2":
                    using (var game = VirtualCity.RunAssignment2(FindSpecialBuildingsWithinDistanceFromHouse, fullscreen))
                        game.Run();
                    break;
                case "3":
                    using (var game = VirtualCity.RunAssignment3(FindRoute, fullscreen))
                        game.Run();
                    break;
                case "4":
                    using (var game = VirtualCity.RunAssignment4(FindRoutesToAll, fullscreen))
                        game.Run();
                    break;
                case "q":
                    return;
            }
            goto read_input;
        }

        private static IEnumerable<Vector2> SortSpecialBuildingsByDistance(Vector2 house, IEnumerable<Vector2> specialBuildings) {
            specialBuildingsList = specialBuildings.ToList();
            MergeSort(specialBuildingsList, house, 0, specialBuildingsList.Count -1);
            return specialBuildingsList;
        }

        private static IEnumerable<IEnumerable<Vector2>> FindSpecialBuildingsWithinDistanceFromHouse(
          IEnumerable<Vector2> specialBuildings,IEnumerable<Tuple<Vector2, float>> housesAndDistances) {
            result = new List<List<Vector2>>();
            KdTree tree = new KdTree();
            foreach(Vector2 v in specialBuildings) {
                tree.Insert(v);
            }
            List<Vector2> allNodes = tree.PreOrderTraversal(tree.root);
            foreach(Tuple<Vector2, float> house in housesAndDistances) {
                List<Vector2> buildingsInRange = new List<Vector2>();
                foreach (Vector2 specialBuilding in allNodes) {
                    if (Vector2.Distance(house.Item1, specialBuilding) <= house.Item2) {
                        buildingsInRange.Add(specialBuilding);
                    }
                }
                result.Add(buildingsInRange);
            }
            return result;
        }

        private static IEnumerable<Tuple<Vector2, Vector2>> FindRoute(Vector2 startingBuilding,
          Vector2 destinationBuilding, IEnumerable<Tuple<Vector2, Vector2>> roads) {
            List<Tuple<Vector2, Vector2>> bestPath = new List<Tuple<Vector2, Vector2>>(); 
            List<Vector2> allNodes = GetAllNodes(roads);
            
            Dictionary<Vector2, DijkstraMatrix> infoMatrix = new Dictionary<Vector2, DijkstraMatrix>();

            foreach (Vector2 node in allNodes)
                infoMatrix.Add(node, new DijkstraMatrix(false, Double.PositiveInfinity, new Vector2(float.NegativeInfinity, float.NegativeInfinity)));
            
            infoMatrix[startingBuilding].cost = 0;
            Vector2 current = startingBuilding;

            while (!current.Equals(destinationBuilding)) { 
                infoMatrix[current].visited = true;
                List<Vector2> neighbors = GetNeighbors(current, roads);
                foreach(Vector2 neighbor in neighbors) {
                    double altDistance = infoMatrix[current].cost + Vector2.Distance(current, neighbor);
                    if (altDistance < infoMatrix[neighbor].cost) {
                        infoMatrix[neighbor].cost = altDistance;
                        infoMatrix[neighbor].previous = current;
                    }
                }
                List<Vector2> unvisitedNodes = infoMatrix.Where(x => x.Value.visited == false).Select(x => x.Key).ToList();
                current = infoMatrix.Where(x => x.Key == unvisitedNodes[0]).Select(x => x.Key).First();
                foreach(Vector2 node in unvisitedNodes) {
                    if (infoMatrix[node].visited == false && infoMatrix[node].cost < infoMatrix[current].cost)
                        current = node;
                }
            }

            List<Vector2> path = new List<Vector2>();
            path.Add(current);
            Vector2 previous = infoMatrix[current].previous;
            while (previous.X != float.NegativeInfinity && previous.Y != float.NegativeInfinity) {
                path.Add(previous);
                previous = infoMatrix[previous].previous;
            }
            path.Reverse();

            for (int i = 0; i < path.Count() - 1; i++)
                bestPath.Add(new Tuple<Vector2, Vector2>(path[i], path[i + 1]));

            return bestPath;


        }

        private static IEnumerable<IEnumerable<Tuple<Vector2, Vector2>>> FindRoutesToAll(Vector2 startingBuilding,
          IEnumerable<Vector2> destinationBuildings, IEnumerable<Tuple<Vector2, Vector2>> roads) {
            List<List<Tuple<Vector2, Vector2>>> result = new List<List<Tuple<Vector2, Vector2>>>();
            foreach (var d in destinationBuildings) {
                var startingRoad = roads.Where(x => x.Item1.Equals(startingBuilding)).First();
                List<Tuple<Vector2, Vector2>> fakeBestPath = new List<Tuple<Vector2, Vector2>>() { startingRoad };
                var prevRoad = startingRoad;
                for (int i = 0; i < 30; i++) {
                    prevRoad = (roads.Where(x => x.Item1.Equals(prevRoad.Item2)).OrderBy(x => Vector2.Distance(x.Item2, d)).First());
                    fakeBestPath.Add(prevRoad);
                }
                result.Add(fakeBestPath);
            }
            return result;
        }

        private static void MergeSort(List<Vector2> specialBuildingsList, Vector2 house, int beginIndex, int endIndex) {
            int middleIndex;
            if (beginIndex < endIndex) {
                middleIndex = (beginIndex + endIndex) / 2;
                MergeSort(specialBuildingsList, house, beginIndex, middleIndex);
                MergeSort(specialBuildingsList, house, middleIndex + 1, endIndex);
                Merge(specialBuildingsList, house, beginIndex, middleIndex, endIndex);
            }
        }

        private static void Merge(List<Vector2> specialBuildingsList, Vector2 house, int beginIndex, int middleIndex, int endIndex) {
            Vector2[] tempArray = new Vector2[endIndex - beginIndex + 1];

            int pointerLeft = beginIndex;
            int tempPointer = 0;
            int pointerRight = middleIndex + 1;

            while (pointerLeft <= middleIndex && pointerRight <= endIndex) {
                double distanceLeft = Vector2.Distance(house, specialBuildingsList[pointerLeft]);
                double distanceRight = Vector2.Distance(house, specialBuildingsList[pointerRight]);
                
                if (distanceLeft <= distanceRight) {
                    tempArray[tempPointer++] = specialBuildingsList[pointerLeft++];
                }
                else {
                    tempArray[tempPointer++] = specialBuildingsList[pointerRight++];
                }
            }

            while (pointerLeft <= middleIndex) {
                tempArray[tempPointer++] = specialBuildingsList[pointerLeft++];
            }

            while (pointerRight <= endIndex) {
                tempArray[tempPointer++] = specialBuildingsList[pointerRight++];
            }

            tempPointer = 0;
            pointerLeft = beginIndex;
            while (tempPointer < tempArray.Length && pointerLeft <= endIndex) {
                specialBuildingsList[pointerLeft++] = tempArray[tempPointer++];
            }
        }

        public static List<Vector2> GetAllNodes(IEnumerable<Tuple<Vector2, Vector2>> list) {
            List<Vector2> allNodes = new List<Vector2>();
            foreach(Tuple<Vector2, Vector2> tuple in list) {
                if (!allNodes.Contains(tuple.Item1))
                    allNodes.Add(tuple.Item1);
                if (!allNodes.Contains(tuple.Item2))
                    allNodes.Add(tuple.Item2);
            }
            return allNodes;
        }

        private static List<Vector2> GetNeighbors(Vector2 v, IEnumerable<Tuple<Vector2, Vector2>> t) {
            List<Vector2> neighbors = new List<Vector2>();
            foreach (Tuple<Vector2, Vector2> tuple in t) {
                if (tuple.Item1.Equals(v)) {
                    neighbors.Add(tuple.Item2);
                }
            }
            return neighbors;
        }
    }
#endif
}
