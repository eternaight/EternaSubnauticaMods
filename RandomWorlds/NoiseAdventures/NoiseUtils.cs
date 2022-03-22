using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseUtils {
    public delegate float DomainMapV3(Vector3 point);
    public delegate float DomainMapV2(Vector2 point);
    public static Vector3 DomainWarp(Vector3 originalPoint, DomainMapV3 angleFunction, DomainMapV3 distanceFunction, float maxDistance) {
        var angle = angleFunction(originalPoint) * Mathf.PI * 2;
        var distance = distanceFunction(originalPoint) * maxDistance;

        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distance;

        return originalPoint + offset;
    }
    public static Vector2 DomainWarp(Vector2 originalPoint, DomainMapV2 angleFunction, DomainMapV2 distanceFunction, float maxDistance) {
        var angle = angleFunction(originalPoint) * Mathf.PI * 2;
        var distance = distanceFunction(originalPoint) * maxDistance;

        Vector2 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;

        return originalPoint + offset;
    }

    public static float BooleanIntersect(params float[] values) => Mathf.Min(values);
    public static float BooleanUnion(params float[] values) => Mathf.Max(values);
    public static float BooleanSubtract(float minuend, float subtrahend) => Mathf.Min(minuend, -subtrahend);

    // from http://viniciusgraciano.com/blog/smin/
    public static float smin(float a, float b, float k) {
        float h = Mathf.Clamp01(0.5f + 0.5f * (a - b) / k);
        return Mathf.Lerp(a, b, h) - k * h * (1 - h);
    }

    public static float SmoothStart(float v) {
        return v * v;
    }
    public static float SmoothStep(float v) {
        return Mathf.SmoothStep(0, 1, v);
    }
    public static float SmoothStop(float v) {
        v -= 1;
        return 1 - v * v;
    }
}
