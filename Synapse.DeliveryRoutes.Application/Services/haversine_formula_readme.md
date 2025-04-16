# Haversine Formula Documentation

## Overview

The Haversine formula is used in our medical equipment delivery scheduler to calculate the "as the crow flies" distance between two points on the Earth's surface. It provides a reasonably accurate measure of the great-circle distance between two points given their latitude and longitude coordinates.

## Mathematical Explanation

The Haversine formula determines the great-circle distance between two points on a sphere given their longitudes and latitudes. It is particularly important in navigation, giving distances between points on the Earth.

### The Formula

Given two points with coordinates (lat1, lon1) and (lat2, lon2), the Haversine formula calculates the distance as follows:

1. Convert latitude and longitude from degrees to radians:
   ```
   lat1_rad = lat1 * π/180
   lon1_rad = lon1 * π/180
   lat2_rad = lat2 * π/180
   lon2_rad = lon2 * π/180
   ```

2. Calculate the differences:
   ```
   Δlat = lat2_rad - lat1_rad
   Δlon = lon2_rad - lon1_rad
   ```

3. Apply the Haversine formula:
   ```
   a = sin²(Δlat/2) + cos(lat1_rad) * cos(lat2_rad) * sin²(Δlon/2)
   c = 2 * atan2(√a, √(1-a))
   distance = R * c
   ```

   Where:
   - `R` is the Earth's radius (approximately 6,371 kilometers or 3,959 miles)
   - `atan2` is the 2-argument arctangent function
   - The result is the distance in the same units as `R` (typically kilometers or miles)

### Implementation Details

In our codebase, the formula is implemented as:

```csharp
private double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
{
    // Earth's radius in kilometers
    const double R = 6371.0;
    
    // Convert degrees to radians
    var dLat = ToRadians(lat2 - lat1);
    var dLon = ToRadians(lon2 - lon1);
    
    // Haversine formula
    var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
    
    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    
    return R * c; // Distance in kilometers
}

private double ToRadians(double degrees)
{
    return degrees * Math.PI / 180.0;
}
```

## Accuracy and Limitations

The Haversine formula:

1. Assumes a spherical Earth, which introduces a small error (up to 0.5%) since the Earth is actually an ellipsoid.
2. Provides "as the crow flies" distances, not actual travel distances along roads.
3. Is suitable for most applications where precision within a few meters is acceptable.
4. Is computationally efficient, making it ideal for calculating many distances quickly.

## Usage in Our Project

In our medical equipment delivery scheduler:

1. The formula is used to build a distance matrix between all locations (the office and delivery points).
2. The distances are scaled to integers (multiplied by 1000) for compatibility with Google OR-Tools, which performs better with integer values.
3. These distances are used as the cost function for determining optimal routes.

## Alternative Methods

For future development, we could consider:

1. **Grid-Based (Manhattan) Distance**: Better for city environments with grid street patterns.
2. **Real-World Routing APIs**: Integration with Google Maps or similar services to get actual driving distances and times.

However, the Haversine formula provides an excellent balance of accuracy and computational efficiency for our initial implementation, making it suitable for our hackathon project.
