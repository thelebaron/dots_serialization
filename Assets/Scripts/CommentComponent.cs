using System.Collections;
using UnityEngine;

/// <summary>
/// Simple component that allows users to write comments to place on MonoBehaviours. Mainly for testmaps.
/// </summary>
public class CommentComponent : MonoBehaviour
{
    [TextArea(3, 10)]
    [SerializeField]
    [Tooltip("Enter comment text here")]
    private string text;

    /// <summary>
    /// Gets or sets the body of the comment, mainly used for display of information on test maps.
    /// </summary>
    /// <value>The body.</value>
    public string Text
    {
        get => this.text;
        set => this.text = value;
    }
}