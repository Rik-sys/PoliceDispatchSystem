﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class GeoUtils
    {
        private const double EARTH_RADIUS_METERS = 6371000;

        /// <summary>
        /// מחשב מרחק אווירי בין שתי נקודות גיאוגרפיות באמצעות נוסחת הברסין
        /// </summary>
        /// <param name="lat1">קו רוחב נקודה 1</param>
        /// <param name="lon1">קו אורך נקודה 1</param>
        /// <param name="lat2">קו רוחב נקודה 2</param>
        /// <param name="lon2">קו אורך נקודה 2</param>  
        /// <returns>מרחק במטרים</returns>
        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            double lat1Rad = ToRadians(lat1);
            double lat2Rad = ToRadians(lat2);
            double deltaLat = ToRadians(lat2 - lat1);
            double deltaLon = ToRadians(lon2 - lon1);

            double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                       Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                       Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EARTH_RADIUS_METERS * c;
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180;
    }
}
