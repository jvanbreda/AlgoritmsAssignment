using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntryPoint {
#if WINDOWS || LINUX
    public static class Program {
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
            int length = 0;
            foreach (Vector2 vector in specialBuildings) {
                length++;
            }
            MergeSort(specialBuildings, house, 0, length);
            //return specialBuildings.OrderBy(v => Vector2.Distance(v, house));
            return specialBuildings;
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

        private static void MergeSort(IEnumerable<Vector2> specialBuildings, Vector2 house, int beginIndex, int endIndex) {
            int middleIndex;
            if (beginIndex < endIndex) {
                middleIndex = (beginIndex + endIndex) / 2;
                MergeSort(specialBuildings, house, beginIndex, middleIndex);
                MergeSort(specialBuildings, house, middleIndex + 1, endIndex);
                Merge(specialBuildings, house, beginIndex, middleIndex, endIndex);
            } 
        }

        private static void Merge(IEnumerable<Vector2> specialBuildings, Vector2 house, int beginIndex, int middleIndex, int endIndex) {
            List<Vector2> tempArray = new List<Vector2>();

            int pointerLeft = beginIndex;
            int pointerRight = middleIndex + 1;

            while (pointerLeft <= middleIndex && pointerRight <= endIndex) {
                double distanceLeft = Math.Sqrt(Math.Pow((house.X) - (specialBuildings.ElementAt(pointerLeft).X), 2) + Math.Pow((house.Y) - (specialBuildings.ElementAt(pointerLeft).Y), 2));
                double distanceRight = Math.Sqrt(Math.Pow((house.X) - (specialBuildings.ElementAt(pointerLeft).X), 2) + Math.Pow((house.Y) - (specialBuildings.ElementAt(pointerLeft).Y), 2));
                if (distanceLeft <= distanceRight) {
                    tempArray.Add(specialBuildings.ElementAt(pointerLeft++));
                }
                else {
                    tempArray.Add(specialBuildings.ElementAt(pointerRight++));
                }
            }

            while (pointerLeft < middleIndex) {
                tempArray.Add(specialBuildings.ElementAt(pointerLeft++));
            }

            while (pointerRight < endIndex) {
                tempArray.Add(specialBuildings.ElementAt(pointerRight++));
            }

            specialBuildings = tempArray;
        }
    }
#endif
}
