using System.Collections.Generic;
using System.Linq;
using Dreamteck.Splines;
using SadPumpkin.Game.Pitstop.Core.Code.Race.Cars;
using Sirenix.OdinInspector;
using UnityEngine;

using Object = UnityEngine.Object;

namespace SadPumpkin.Game.Pitstop.Core.Code.Race.Track
{
    public enum RacePathingType : int
    {
        MainTrack = 0,
        AlternateTrack = 1,
        Pit = 99,
    }

    public class RacePathingHelper
    {
        private readonly struct PathPoint
        {
            public readonly float Distance;
            public readonly float Percent;
            public readonly SplineComputer Path;

            public PathPoint(float distance, float percent, SplineComputer path)
            {
                Distance = distance;
                Percent = percent;
                Path = path;
            }
        }

        [ReadOnly] public IReadOnlyDictionary<RacePathingType, RacePathingComponent[]> PathingComponents;

        private float _maxDistFromPath;
        private readonly List<PathPoint> _allPathPointsCache = new List<PathPoint>(10);
        
        private static readonly IReadOnlyCollection<RacePathingType> _pitStopPriorityOrder = new[]
        {
            RacePathingType.Pit,
            RacePathingType.MainTrack,
            RacePathingType.AlternateTrack,
        };

        private static readonly IReadOnlyCollection<RacePathingType> _racePriorityOrder = new[]
        {
            RacePathingType.MainTrack,
            RacePathingType.AlternateTrack,
        };
        
        public RacePathingHelper(float maxDistanceFromValidPath = 10f)
        {
            _maxDistFromPath = maxDistanceFromValidPath;
        }

        public void InitializeRacePaths()
        {
            PathingComponents = Object.FindObjectsOfType<RacePathingComponent>()
                .GroupBy(x => x.Type)
                .ToDictionary(
                    x => x.Key,
                    x => x.ToArray());
        }

        public float GetCarPositionInRace(Vector3 carPosition)
        {
            if (PathingComponents.TryGetValue(RacePathingType.MainTrack, out var pathList))
            {
                return (float)pathList
                    .Select(x => x.Spline.Project(carPosition).percent)
                    .OrderByDescending(x => x)
                    .FirstOrDefault();
            }

            return 0f;
        }

        public Vector3 GetCarForwardPoint(Vector3 carPosition, CarGoal carGoal, float forwardStepPercent = 0.025f)
        {
            IReadOnlyCollection<RacePathingType> pathingTypeOrder = carGoal == CarGoal.GoToPit
                ? _pitStopPriorityOrder
                : _racePriorityOrder;

            _allPathPointsCache.Clear();
            foreach (RacePathingType racePathingType in pathingTypeOrder)
            {
                if (PathingComponents.TryGetValue(racePathingType, out var pathList))
                {
                    foreach (RacePathingComponent pathingComponent in pathList)
                    {
                        PathPoint pathPoint = GetPathData(pathingComponent, carPosition);
                        if (pathPoint.Path != null)
                        {
                            if (pathPoint.Distance <= _maxDistFromPath)
                            {
                                return pathPoint.Path.EvaluatePosition((pathPoint.Percent + forwardStepPercent) % 1f);
                            }

                            _allPathPointsCache.Add(pathPoint);
                        }
                    }
                }
            }

            if (_allPathPointsCache.Count > 0)
            {
                PathPoint bestBadPoint = _allPathPointsCache.OrderBy(x => x.Distance).First();
                return bestBadPoint.Path.EvaluatePosition((bestBadPoint.Percent + forwardStepPercent) % 1f);
            }
            else
            {
                return Vector3.zero;
            }
        }

        private PathPoint GetPathData(RacePathingComponent pathingComponent, Vector3 position)
        {
            if (Vector3.Distance(position, pathingComponent.transform.position) < pathingComponent.TestRange)
            {
                var splineSample = pathingComponent.Spline.Project(position);

                return new PathPoint(Vector3.Distance(position, splineSample.position), (float)splineSample.percent, pathingComponent.Spline);
            }

            return default;
        }
    }
}