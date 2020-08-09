using UnityEngine;

namespace DOTS.Serialization
{
// Draws 2 buttons, one with an image, and other with a text
// And print a message when they got clicked.


    public class TemporaryMenu : MonoBehaviour
    {
        void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 50, 50), "Click"))
                Debug.Log("Clicked the button with an image");

            if (GUI.Button(new Rect(10, 70, 50, 30), "Click"))
                Debug.Log("Clicked the button with text");
        }
    }
}