using System;
using System.Collections.Generic;
using UnityEngine;

public class PoissonSampler
{
    private Action<Vector3> sampleAction;

    public PoissonSampler()
    {
        Action<Vector3> defaultSampleAction = (samplePosition) => {
            Debug.Log($"sample created at {samplePosition}");
        };

        SetSampleAction(defaultSampleAction);
    }

    public PoissonSampler(Action<Vector3> sampleAction)
    {
        this.sampleAction = sampleAction;
    }

    public void SetSampleAction(Action<Vector3> sampleAction)
    {
        this.sampleAction = sampleAction;
    }

   
    public void Sample(Vector3 originPosition, float areaWidth)
    {
        // alghorithm from the paper:
        // https://www.cs.ubc.ca/~rbridson/docs/bridson-siggraph07-poissondisk.pdf

        areaWidth += 0.5f;
        float r = 0.4f;
        int k = 40;
        float cellWidth = r / Mathf.Sqrt(2); // 2 dimensions // cellWidth == w

        int stride = Mathf.FloorToInt(areaWidth / cellWidth);

        Vector2[] grid = new Vector2[stride * stride];
        var active = new List<Vector2>();

        // STEP 0
        var emptyPoint = new Vector2(-1.0f, -1.0f);
        for (int i = 0; i < stride * stride; i++)
        {
            grid[i] = emptyPoint;
        }

        // STEP 1
        var xx = areaWidth / 2.0f;
        int ii = Mathf.FloorToInt(xx / cellWidth);
        var startingPos = new Vector2(xx, xx);
        grid[ii + ii * stride] = startingPos;
        active.Add(startingPos);

        while (active.Count > 0)
        {
            int randIndex = Mathf.FloorToInt(UnityEngine.Random.Range(0, active.Count));
            var pos = active[randIndex];
            bool found = false;

            // k tries
            for (int i = 0; i < k; i++)
            {
                var angle = UnityEngine.Random.Range(0.0f, 2.0f * Mathf.PI);
                Vector2 sample = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                sample.Normalize(); // TODO do i need to do this?
                sample *= UnityEngine.Random.Range(r, 2.0f * r);
                sample += pos;

                int x = Mathf.FloorToInt(sample.x / cellWidth);
                int y = Mathf.FloorToInt(sample.y / cellWidth);
                int sampleIndex = x + y * stride;

                if (x >= 1 && y >= 1 && x < stride - 1 && y < stride - 1)
                {

                    var ok = true;
                    for (int yOffset = -1; yOffset <= 1; yOffset++)
                    {
                        for (int xOffset = -1; xOffset <= 1; xOffset++)
                        {
                            var neighborIndex = sampleIndex + xOffset + yOffset * stride;
                            var neighbor = grid[neighborIndex];

                            if (neighbor != emptyPoint)
                            {
                                var distance = Vector2.Distance(sample, neighbor);
                                if (distance < r)
                                {
                                    ok = false;
                                }
                            }
                        }
                    }

                    if (ok)
                    {
                        found = true;
                        grid[sampleIndex] = sample;
                        active.Add(sample);
                    }
                }
            }

            if (!found)
            {
                active.RemoveAt(randIndex);
            }
        }

        foreach (var sample in grid)
        {
            if (sample != emptyPoint)
            {
                var samplePosition = new Vector3(sample.x, 0, sample.y) + originPosition;
                sampleAction(samplePosition);
            }
        }
    }
}