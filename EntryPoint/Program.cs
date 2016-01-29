using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntryPoint {
#if WINDOWS || LINUX
    public static class Program {
        static List<Vector2> specialBuildingsList;
        static List<List<Vector2>> result;
        static Dictionary<Vector2, int> idDictionary;
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

        /// <summary>
        /// Calls the method which is needed to initiate the merge sort
        /// </summary>
        /// <param name="house"></param>
        /// <param name="specialBuildings"></param>
        /// <returns></returns>
        private static IEnumerable<Vector2> SortSpecialBuildingsByDistance(Vector2 house, IEnumerable<Vector2> specialBuildings) {
            specialBuildingsList = specialBuildings.ToList();
            MergeSort(specialBuildingsList, house, 0, specialBuildingsList.Count -1);
            return specialBuildingsList;
        }

        /// <summary>
        /// Creates a tree, stores every node in the tree accordingly
        /// Then traverses the tree pre-order and finds all the buildings within a given distance
        /// </summary>
        /// <param name="specialBuildings"></param>
        /// <param name="housesAndDistances"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Calculates the shortest path from a starting building to a given destination building
        /// </summary>
        /// <param name="startingBuilding"></param>
        /// <param name="destinationBuilding"></param>
        /// <param name="roads"></param>
        /// <returns></returns>
        private static IEnumerable<Tuple<Vector2, Vector2>> FindRoute(Vector2 startingBuilding,
          Vector2 destinationBuilding, IEnumerable<Tuple<Vector2, Vector2>> roads) {
            List<Tuple<Vector2, Vector2>> bestPath = new List<Tuple<Vector2, Vector2>>();
            double[][] adjecancyMatrix = GetAdjencancyMatrix(roads);
            
            Dictionary<Vector2, DijkstraMatrix> infoMatrix = new Dictionary<Vector2, DijkstraMatrix>();

            // For each vector2, we keep some information: if the node is visited, the distance from the source node, and the previous node.
            foreach (Vector2 key in idDictionary.Keys)
                infoMatrix.Add(key, new DijkstraMatrix(false, Double.PositiveInfinity, new Vector2(float.NegativeInfinity, float.NegativeInfinity)));

            infoMatrix[startingBuilding].cost = 0;
            Vector2 current = startingBuilding;

            //We keep searching for a new vector2 to add to the path until we reach our destination
            while (!current.Equals(destinationBuilding)) { 
                infoMatrix[current].visited = true;
                List<Vector2> neighbors = GetNeighbors(current, adjecancyMatrix);
                foreach(Vector2 neighbor in neighbors) {
                    double altDistance = infoMatrix[current].cost + adjecancyMatrix[idDictionary[current]][idDictionary[neighbor]];
                    if (altDistance < infoMatrix[neighbor].cost) {
                        infoMatrix[neighbor].cost = altDistance;
                        infoMatrix[neighbor].previous = current;
                    }
                }
                // We get all the unvisited nodes and get the one with the lowest distance 
                List<Vector2> unvisitedNodes = infoMatrix.Where(x => x.Value.visited == false).Select(x => x.Key).ToList();
                current = infoMatrix.Where(x => x.Key == unvisitedNodes[0]).Select(x => x.Key).First();
                foreach(Vector2 node in unvisitedNodes) {
                    if (infoMatrix[node].visited == false && infoMatrix[node].cost < infoMatrix[current].cost)
                        current = node;
                }
            }

            // Here, the current node is our destination. From here, we use the previous of each node to find the
            // complete path. While a given node has a previous node, we add the previous node to the path.
            List<Vector2> path = new List<Vector2>();
            path.Add(current);
            Vector2 previous = infoMatrix[current].previous;
            while (previous.X != float.NegativeInfinity && previous.Y != float.NegativeInfinity) {
                path.Add(previous);
                previous = infoMatrix[previous].previous;
            }
            path.Reverse();

            // Using the list of vector2's, we now create a list of tuples to contruct the path
            for (int i = 0; i < path.Count() - 1; i++)
                bestPath.Add(new Tuple<Vector2, Vector2>(path[i], path[i + 1]));

            return bestPath;


        }
        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="startingBuilding"></param>
        /// <param name="destinationBuildings"></param>
        /// <param name="roads"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Keeps splitting the array in half until each element of the original array is in a seperate array
        /// </summary>
        /// <param name="specialBuildingsList"></param>
        /// <param name="house"></param>
        /// <param name="beginIndex"></param>
        /// <param name="endIndex"></param>
        private static void MergeSort(List<Vector2> specialBuildingsList, Vector2 house, int beginIndex, int endIndex) {
            int middleIndex;
            if (beginIndex < endIndex) {
                middleIndex = (beginIndex + endIndex) / 2;
                MergeSort(specialBuildingsList, house, beginIndex, middleIndex);
                MergeSort(specialBuildingsList, house, middleIndex + 1, endIndex);
                Merge(specialBuildingsList, house, beginIndex, middleIndex, endIndex);
            }
        }

        /// <summary>
        /// Merges the given 'two' arrays (actually one array with two pointers) to one sorted array
        /// </summary>
        /// <param name="specialBuildingsList"></param>
        /// <param name="house"></param>
        /// <param name="beginIndex"></param>
        /// <param name="middleIndex"></param>
        /// <param name="endIndex"></param>
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

        /// <summary>
        /// Constructs the adjecancy matrix by checking every tuple in the roads List.
        /// Also, a dictionary is made to keep track of which vector2 belongs to which row and column
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static double[][] GetAdjencancyMatrix(IEnumerable<Tuple<Vector2, Vector2>> list) {
            List<Tuple<Vector2, Vector2>> listCopy = list.ToList();
            idDictionary = new Dictionary<Vector2, int>();
            int counter = 0;
            foreach(Tuple<Vector2, Vector2> t in listCopy) {
                if (!idDictionary.ContainsKey(t.Item1))
                    idDictionary.Add(t.Item1, counter++);
                if (!idDictionary.ContainsKey(t.Item2))
                    idDictionary.Add(t.Item2, counter++);
            }
            
             
            double[][] adjecancyMatrix = new double[listCopy.Count()][];
            for (int i = 0; i < list.Count(); i++) {
                adjecancyMatrix[i] = new double[listCopy.Count()];
            }


            for(int i = 0; i < listCopy.Count(); i++) {
                for(int j = 0; j < listCopy.Count(); j++) {
                    adjecancyMatrix[i][j] = 0.0;
                }
            }
            foreach(Tuple<Vector2, Vector2> t in listCopy) {
                int column = idDictionary[t.Item1];
                int row = idDictionary[t.Item2];
                adjecancyMatrix[column][row] = Vector2.Distance(t.Item1, t.Item2);
            }
            return adjecancyMatrix;
        }

        /// <summary>
        /// Gets the neighbors of the given vector2 by checking the adjecancy matrix
        /// If the number on the checked row/column combination is not equal to 0, there is a connection between 
        /// the two vector2's
        /// </summary>
        /// <param name="v"></param>
        /// <param name="adjecancyMatrix"></param>
        /// <returns></returns>
        private static List<Vector2> GetNeighbors(Vector2 v, double[][] adjecancyMatrix) {
            List<Vector2> neighbors = new List<Vector2>();
            double[] row = adjecancyMatrix[idDictionary[v]];
            for (int i = 0; i < row.Length; i++) {
                if (row[i] != 0.0)
                    neighbors.Add(idDictionary.Where(x => x.Value == i).Select(x => x.Key).First());
            }
            return neighbors;
        }
    }
#endif
}
