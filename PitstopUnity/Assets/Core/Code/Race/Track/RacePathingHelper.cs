using System;
using System.Collections.Generic;
using System.Linq;
using Dreamteck.Splines;
using Sirenix.OdinInspector;
using UnityEngine;

using Object = UnityEngine.Object;

namespace SadPumpkin.Game.Pitstop
{
    public enum RacePathingType : int
    {
        MainTrack = 0,
        AlternateTrack = 1,
        Pit = 99,
    }

    public class RacePathingHelper
    {
        [ReadOnly] public IReadOnlyDictionary<RacePathingType, RacePathingComponent[]> PathingComponents;

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
            (float distance, float percent, SplineComputer path) GetPathData(
                RacePathingComponent pathingComponent,
                Vector3 position)
            {
                if (Vector3.Distance(position, pathingComponent.transform.position) < pathingComponent.TestRange)
                {
                    var splineSample = pathingComponent.Spline.Project(position);

                    return (Vector3.Distance(position, splineSample.position), (float)splineSample.percent, pathingComponent.Spline);
                }

                return default;
            }

            (float distance, float percent, SplineComputer path) pathPoint = default;

            // If we're aiming for the pitstop, see if we're within range to start following that path
            if (carGoal == CarGoal.GoToPit &&
                PathingComponents.TryGetValue(RacePathingType.Pit, out var pathList))
            {
                pathPoint = pathList
                    .Select(x => GetPathData(x, carPosition))
                    .Where(x => x != default)
                    .OrderBy(x => x.distance)
                    .FirstOrDefault();
            }

            // If we're still racing then use whatever we can
            if (pathPoint == default)
            {
                pathPoint = PathingComponents
                    .SelectMany(x => x.Value)
                    .Select(x => GetPathData(x, carPosition))
                    .Where(x => x != default)
                    .OrderBy(x => x.distance)
                    .FirstOrDefault();
            }

            if (pathPoint != default)
            {
                return pathPoint.path.EvaluatePosition((pathPoint.percent + forwardStepPercent) % 1f);
            }
            else
            {
                return Vector3.zero;
            }
        }
    }
}