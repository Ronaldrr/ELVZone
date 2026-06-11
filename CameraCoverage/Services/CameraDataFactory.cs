using System;
using Autodesk.Revit.DB;
using ELVZone.Geometry;
using ELVZone.Models;

namespace ELVZone.Services
{
    public class CameraDataFactory
    {
        private const double FeetPerMeter = 3.280839895013123;
        private readonly ElementParameterReader _parameterReader = new ElementParameterReader();

        public CameraViewZoneData Create(Element camera, ViewZoneSettings settings)
        {
            var origin = GetOrigin(camera);
            var direction = GetDirection(camera);

            return new CameraViewZoneData
            {
                Origin = origin,
                Direction = direction,
                HorizontalAngleRadians = _parameterReader.ReadAngleRadians(
                    camera,
                    settings.HorizontalAngleParameter,
                    settings.DefaultHorizontalAngleDegrees),
                VerticalAngleRadians = _parameterReader.ReadAngleRadians(
                    camera,
                    settings.VerticalAngleParameter,
                    settings.DefaultVerticalAngleDegrees),
                MountingHeightFeet = _parameterReader.ReadLengthFeet(
                    camera,
                    settings.MountingHeightParameter,
                    settings.DefaultMountingHeightMeters),
                DeadZoneLengthFeet = _parameterReader.ReadLengthFeet(
                    camera,
                    settings.DeadZoneLengthParameter,
                    settings.DefaultDeadZoneLengthMeters),
                ZoneLengthsFeet = new[]
                {
                    _parameterReader.ReadLengthFeet(camera, settings.Zone1LengthParameter, settings.DefaultZone1LengthMeters),
                    _parameterReader.ReadLengthFeet(camera, settings.Zone2LengthParameter, settings.DefaultZone2LengthMeters),
                    _parameterReader.ReadLengthFeet(camera, settings.Zone3LengthParameter, settings.DefaultZone3LengthMeters),
                    _parameterReader.ReadLengthFeet(camera, settings.Zone4LengthParameter, settings.DefaultZone4LengthMeters)
                },
                TotalLengthFeet = _parameterReader.ReadLengthFeet(
                    camera,
                    settings.TotalLengthParameter,
                    settings.DefaultTotalLengthMeters),
                AnalysisBottomHeightFeet = settings.AnalysisBottomHeightMeters * FeetPerMeter,
                AnalysisTopHeightFeet = settings.AnalysisTopHeightMeters * FeetPerMeter
            };
        }

        private static XYZ GetOrigin(Element camera)
        {
            if (camera.Location is LocationPoint locationPoint)
            {
                return locationPoint.Point;
            }

            var boundingBox = camera.get_BoundingBox(null);
            if (boundingBox != null)
            {
                return (boundingBox.Min + boundingBox.Max).Multiply(0.5);
            }

            throw new InvalidOperationException("Не удалось определить точку установки камеры.");
        }

        private static XYZ GetDirection(Element camera)
        {
            if (camera is FamilyInstance familyInstance)
            {
                var facing = familyInstance.FacingOrientation;
                if (facing != null && facing.GetLength() > 0.001)
                {
                    return facing;
                }
            }

            if (camera.Location is LocationPoint locationPoint)
            {
                var rotation = locationPoint.Rotation;
                return new XYZ(Math.Sin(rotation), Math.Cos(rotation), 0);
            }

            return XYZ.BasisY;
        }
    }
}
