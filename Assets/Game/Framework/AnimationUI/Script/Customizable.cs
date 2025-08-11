using UnityEngine;

namespace AnimationUI
{
    public class Customizable
    {
        public static void SetActiveAllInput(bool isActivating)
        {
            // Please modify this line to use your own Singleton class.
            Debug.Log("Set Active All Input");
        }

        public static void PlaySound(AudioClip _SFXFile)
        {
            // Please modify this line to use your own Singleton class.
            Debug.Log("SFX by file");
        }

        public static void PlaySound(int _index)
        {
            // Please modify this line to use your own Singleton class.
            Debug.Log("SFX by index");
        }
    }
}