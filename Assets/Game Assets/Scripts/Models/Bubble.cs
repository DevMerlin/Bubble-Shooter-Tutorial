using System;
using UnityEngine;

public class Bubble 
{
    private Vector2Int position;

    private int type = 0;

    /// <summary>
    /// Removal Callback - Triggers when this bubble gets removed.
    /// </summary>
    public Action<Bubble> removalCallback;

    public Vector2Int Position
    {
        get { return position; }
        set
        {
            position = value;
        }
    }

    public int Type { get => type; set => type = value; }

    /// <summary>
    /// Bubble Constructor, takes a Position and Type
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="Type"></param>
    public Bubble(Vector2Int pos, int Type)
    {
        this.Position = pos;
        this.Type = Type;
    }

    public void TriggerRemoval()
    {
        removalCallback.Invoke(this);
    }

    /// <summary>
    /// Register the Removal Callback for this bubble
    /// </summary>
    /// <param name="removalCallback"></param>
    public void RegisterRemovalCallback(Action<Bubble> removalCallback)
    {
        this.removalCallback = removalCallback;
    }
}