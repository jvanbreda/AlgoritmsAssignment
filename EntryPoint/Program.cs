using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntryPoint {
#if WINDOWS || LINUX
    public static class Program {
        static List<Vector2> specialBuildingsList;
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
          IEnumerable<Vector2> specialBuildings,
          IEnumerable<Tuple<Vector2, float>> housesAndDistances) {
            return
                from h in housesAndDistances
                select
                  from s in specialBuildings
                  where Vector2.Distance(h.Item1, s) <= h.Item2
                  select s;
        }

        private static IEnumerable<Tuple<Vector2, Vector2>> FindRoute(Vector2 startingBuilding,
          Vector2 destinationBuilding, IEnumerable<Tuple<Vector2, Vector2>> roads) {
            var startingRoad = roads.Where(x => x.Item1.Equals(startingBuilding)).First();
            List<Tuple<Vector2, Vector2>> fakeBestPath = new List<Tuple<Vector2, Vector2>>() { startingRoad };
            var prevRoad = startingRoad;
            for (int i = 0; i < 30; i++) {
                prevRoad = (roads.Where(x => x.Item1.Equals(prevRoad.Item2)).OrderBy(x => Vector2.Distance(x.Item2, destinationBuilding)).First());
                fakeBestPath.Add(prevRoad);
            }
            return fakeBestPath;
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
    }
#endif
}
