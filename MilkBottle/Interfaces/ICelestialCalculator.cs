using System;
using MilkBottle.Models.Sunset;

namespace MilkBottle.Interfaces {
    interface ICelestialCalculator {
        CelestialData   CalculateData( double latitude, double longitude, DateTime forTime, double timeZoneDifferenceFromUtc );
        CelestialData   CalculateData( double latitude, double longitude );
    }
}
