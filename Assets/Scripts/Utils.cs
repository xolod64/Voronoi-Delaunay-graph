using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utils
{
    public static Vector2 GetCircumcenter(Vector2 a, Vector2 b, Vector2 c)
    {
        float d = 2f * (a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y));

        if (Mathf.Abs(d) < 0.001f)
        {
            return (a + b + c) / 3f;
        }

        float aSq = a.x * a.x + a.y * a.y;
        float bSq = b.x * b.x + b.y * b.y;
        float cSq = c.x * c.x + c.y * c.y;

        float ux = (aSq * (b.y - c.y) + bSq * (c.y - a.y) + cSq * (a.y - b.y)) / d;
        float uy = (aSq * (c.x - b.x) + bSq * (a.x - c.x) + cSq * (b.x - a.x)) / d;

        return new Vector2(ux, uy);
    }

    public static void GetPointsOnLine(int x0, int y0, int x1, int y1, List<Vector2Int> points)
    {
        points.Clear();
        
        // Обчислюємо абсолютну відстань по осях
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);

        // Визначаємо напрямок руху (+1 або -1)
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        // Початкова помилка зсуву
        int err = dx - dy;

        while (true)
        {
            // Додаємо поточну точку в список
            points.Add(new Vector2Int(x0, y0));

            // Якщо дійшли до кінцевої точки — зупиняємось
            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;

            // Крок по осі X
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            // Крок по осі Y
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
}