using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Main : MonoBehaviour
{
    int[,] sudoku = new int[,]
    {
        {5, 3, 0, 0, 7, 0, 0, 0, 0},
        {6, 0, 0, 1, 9, 5, 0, 0, 0},
        {0, 9, 8, 0, 0, 0, 0, 6, 0},
        {8, 0, 0, 0, 6, 0, 0, 0, 3},
        {4, 0, 0, 8, 0, 3, 0, 0, 1},
        {7, 0, 0, 0, 2, 0, 0, 0, 6},
        {0, 6, 0, 0, 0, 0, 2, 8, 0},
        {0, 0, 0, 4, 1, 9, 0, 0, 5},
        {0, 0, 0, 0, 8, 0, 0, 7, 9}
    };
    int[,] solved = new int[9, 9];

    int[,] problemCpy = new int[9,9]; 
    int[,] weights = new int[9, 9];
    int[] elements = new int[9] {0,0,0,0,0,0,0,0,0};

    public List<GameObject> rows;
    public List<GameObject> Numbers;
    List<Transform> toDelete = new List<Transform>();

    private void Start()
    {
        int x, y;

        for (x = 0; x < 9; x++)
            for (y = 0; y < 9; y++)
            {
                problemCpy[x,y] = sudoku[x,y];
            }

        DisplayTiles(problemCpy);


    }
    
    void DisplayTiles(int[,] grid)
    {
        int x, y;

        if(toDelete.Count > 0)
        for (x = 0; x < toDelete.Count; x++)
        {
            Destroy(toDelete[x]);
        }

            for (x = 0; x < 9; x++)
        {
            for (y = 0; y < 9; y++)
            {
                if (grid[x,y] != 0)
                {
                    toDelete.Add(Instantiate(Numbers[grid[x, y] - 1], rows[x].transform.GetChild(y)).transform);
                }
                    
            }
        }
    }

    public void BDisplay() {
        int x, y;



        DisplayTiles(solved);
    }

    void countElements()
    {
        int x, y;

        for (x = 0; x < 9; x++) elements[x] = 0;

        for (x = 0; x < 9; x++)
            for (y = 0; y < 9; y++)
            {
                if (problemCpy[x, y] > 0) elements[problemCpy[x, y] - 1] += 1;
            }
    }

    bool ending()
    {
        int x;

        for (x = 0; x < 9; x++)
        {
            if (elements[x] != 9) return false;
        }

        return true;
    }

    //Check if the position is valid for a value
    bool valid(int x, int y, int value)
    {
        int iterator1, iterator2;

        //Line Check
        for (iterator1 = 0; iterator1 < 9; iterator1++)
        {
            if (problemCpy[x, iterator1] == value) return false;
            if (problemCpy[iterator1, y] == value) return false;
        }

        int lowX, lowY;
        lowX = low3(x);
        lowY = low3(y);

        //Square check
        for (iterator1 = lowX; iterator1 < lowX + 3; iterator1++)
            for (iterator2 = lowY; iterator2 < lowY + 3; iterator2++)
            {
                if (problemCpy[iterator1,iterator2] == value) return false;
            }

        return true;
    }

    //Return the smaller multiple of 3
    int low3(int x)
    {
        while (x % 3 != 0) x--; return x;
    }

    //Create a height map based on the empty value slots that we are looking for
    void setWeights(int value)
    {
        int x, y;
        int iterator;

        for (x = 0; x < 9; x++)
            for (y = 0; y < 9; y++)
                weights[x, y] = 0;

                for (x = 0; x < 9; x++)
            for (y = 0; y < 9; y++)
            {
                if (problemCpy[x, y] == value)
                {
                    weights[x, y] = 10;

                    for (iterator = x + 1; iterator < 9; iterator++)
                    {
                        if (weights[iterator, y] < weights[iterator - 1, y] - 1)
                            weights[iterator, y] = weights[iterator - 1, y] - 1;
                    }
                    for (iterator = x - 1; iterator >= 0; iterator--)
                    {
                        if (weights[iterator, y] < weights[iterator + 1, y] - 1)
                            weights[iterator, y] = weights[iterator + 1, y] - 1;
                    }

                    for (iterator = y + 1; iterator < 9; iterator++)
                    {
                        if (weights[x, iterator] < weights[x, iterator - 1] - 1)
                            weights[x, iterator] = weights[x, iterator - 1] - 1;
                    }
                    for (iterator = y - 1; iterator >= 0; iterator--)
                    {
                        if (weights[x, iterator] < weights[x, iterator + 1] - 1)
                            weights[x, iterator] = weights[x, iterator + 1] - 1;
                    }

                    int lowX, lowY; int i, j;
                    lowX = low3(x);
                    lowY = low3(y);
                    Vector2 origin = new Vector2(x, y);

                    for (i = lowX; i < lowX + 3; i++)
                        for (j = lowY; j < lowY + 3; j++)
                        {
                            Vector2 target = new Vector2(i, j);
                            int newWeight = 10 - (int)(origin - target).magnitude;
                            if (newWeight > weights[i, j]) weights[i, j] = newWeight;
                        }
                }
                else if (problemCpy[x, y] != 0)
                {
                    if (1 > weights[x, y]) weights[x, y] = 1;
                }
            }
    }
    Vector2 GetTargetPosition(int[,] weights)
    {
        int rows = weights.GetLength(0);
        int cols = weights.GetLength(1);

        // Initialize distances to infinity and visited array to false
        int[,] dist = new int[rows, cols];
        bool[,] visited = new bool[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                dist[i, j] = int.MaxValue;
                visited[i, j] = false;
            }
        }

        // Set distance to starting point to 0
        dist[4, 4] = 0;

        SortedDictionary<int, Vector2> pq = new SortedDictionary<int, Vector2>();

        // Helper method to add or update key-value pairs in the priority queue
        void AddOrUpdate(int key, Vector2 value)
        {
            if (pq.ContainsKey(key))
            {
                pq[key] = value;
            }
            else
            {
                pq.Add(key, value);
            }
        }

        AddOrUpdate(0, new Vector2(4, 4));

        while (pq.Count > 0)
        {
            // Get the vertex with the minimum distance
            KeyValuePair<int, Vector2> curr = pq.First();
            pq.Remove(curr.Key);
            int x = (int)curr.Value.x;
            int y = (int)curr.Value.y;

            // If we've reached the target weight, return the position
            if (weights[x, y] == 0)
            {
                return curr.Value;
            }

            // If we've already visited this vertex, skip it
            if (visited[x, y])
            {
                continue;
            }

            visited[x, y] = true;

            // Check neighbors and update distances
            if (x > 0 && !visited[x - 1, y])
            {
                int alt = dist[x, y] + weights[x - 1, y];
                if (alt < dist[x - 1, y])
                {
                    dist[x - 1, y] = alt;
                    AddOrUpdate(alt, new Vector2(x - 1, y));
                }
            }
            if (x < rows - 1 && !visited[x + 1, y])
            {
                int alt = dist[x, y] + weights[x + 1, y];
                if (alt < dist[x + 1, y])
                {
                    dist[x + 1, y] = alt;
                    AddOrUpdate(alt, new Vector2(x + 1, y));
                }
            }
            if (y > 0 && !visited[x, y - 1])
            {
                int alt = dist[x, y] + weights[x, y - 1];
                if (alt < dist[x, y - 1])
                {
                    dist[x, y - 1] = alt;
                    AddOrUpdate(alt, new Vector2(x, y - 1));
                }
            }
            if (y < cols - 1 && !visited[x, y + 1])
            {
                int alt = dist[x, y] + weights[x, y + 1];
                if (alt < dist[x, y + 1])
                {
                    dist[x, y + 1] = alt;
                    AddOrUpdate(alt, new Vector2(x, y + 1));
                }
            }
        }

        return new Vector2(-1, -1);
    }

    public void solve()
    {
        countElements();
        if (!ending())
        {
            int x = 0;
            for (x = 0; x < 9; x++)
            {
                setWeights(x);
                Vector2 target = GetTargetPosition(weights);

                if (target.x != -1 && target.y != -1)
                {
                    problemCpy[(int)target.x, (int)target.y] = x;
                    solve();
                    problemCpy[(int)target.x, (int)target.y] = 0;
                }
            }
        }
        else solved = problemCpy;
    }
}
