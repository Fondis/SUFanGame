﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SUGame.Interactions
{
    public class SupportRunner : MonoBehaviour
    {
        /* TODO:
         * Remove hardcoded values
         * Check if it's inefficient to do a Resources.Load<>() for every single line or if caching is built in
         */

        public GameObject canvas;

        private ScriptLine[] dialog;

        private GameObject newCanvas;
        // Typically in Unity referencing other objects is done like this,
        // serialize the private fields and set them through the inspector.
        // This way you can move or rename the objects and the references will still be maintained.
        [SerializeField]
        private Text spokenText;
        [SerializeField]
        private Image leftPortrait;
        [SerializeField]
        private Image rightPortrait;

        // Speaker nameplates are disabled when not talking
        [SerializeField]
        GameObject nameplateLeft;
        [SerializeField]
        GameObject nameplateRight;
        // Retrieve our text components through GetComponent in awake.
        Text nameplateLeftText;
        Text nameplateRightText;
        //private GameObject nameplate;

        private bool destroyOnEnd = true; //Dialog destroyed after script is done by default
        [SerializeField]
        private bool preservedCanvas = true; //True if the canvas wasn't destroyed so we don't load twice


        public bool DestroyOnEnd
        {
            set { destroyOnEnd = value; }
        }

        public ScriptLine[] Dialog
        {
            set { dialog = value; }
        }

        // Alternative to storing all the relevant references in the caller - have the 
        // canvas object keep track of what it needs to can use a simple interface
        // to interact with it as needed.
        /// <summary>
        /// Perform the current dialogue on the given canvas.
        /// </summary>
        public IEnumerator DoDialogueWithCanvas(SupportCanvas canvas)
        {

            if (dialog == null)
            {
                throw new UnityException("There's no dialog set!");
            }

            foreach (ScriptLine curDialog in dialog)
            {
                int side = curDialog.CurrentSpeaker == curDialog.LeftSpeaker ? 0 : 1;
                canvas.SetDialogue(side, curDialog.CurrentSpeaker, curDialog.Line);
                var leftSprite = Resources.Load<Sprite>
                    ("Characters/Test/" + curDialog.LeftSpeaker + "/" + curDialog.LeftExpr);
                var rightSprite = Resources.Load<Sprite>
                    ("Characters/Test/" + curDialog.RightSpeaker + "/" + curDialog.RightExpr);
                canvas.SetPortraits(leftSprite, rightSprite);
                // Prevent double key presses
                yield return new WaitForSeconds(.2f);
                // Now wait for the next key
                yield return new WaitWhile(() => !Input.GetButtonDown("Submit"));
            }
            dialog = null;
        }

        public IEnumerator DoDialog()
        {
            nameplateLeftText = nameplateLeft.GetComponentInChildren<Text>();
            nameplateRightText = nameplateRight.GetComponentInChildren<Text>();
            if (dialog == null)
            {
                throw new UnityException("There's no dialog set!");
            }

            if (!preservedCanvas)
            {
                //Make the gui!
                newCanvas = Instantiate(canvas) as GameObject;

                //Convenience references, this is super, super dependent on the prefab structure.
                //Reference SupportCanvas prefab for the child tree structure.
                spokenText = newCanvas.transform.GetChild(2).GetChild(1).gameObject.GetComponent<Text>();
                //nameplate = newCanvas.transform.GetChild(2).GetChild(0).gameObject;
                leftPortrait = newCanvas.transform.GetChild(1).gameObject.GetComponent<Image>();
                rightPortrait = newCanvas.transform.GetChild(0).gameObject.GetComponent<Image>();
            }

            foreach (ScriptLine curDialog in dialog)
            {

                //Update the nameplate if speaker changed sides
                //These numbers need to reference the prefab's settings, currently hardcoded
                //anchor is middle top of the textbox
                
                //offsetMax The offset of the upper right corner of the rectangle relative to the upper right anchor.
                //offsetMin The offset of the lower left corner of the rectangle relative to the lower left anchor.
                //RectTransform nameplateBox = nameplate.GetComponent<RectTransform>();
                
                if (curDialog.CurrentSpeaker == curDialog.RightSpeaker)
                {  //RIGHT SIDE
                    nameplateLeft.SetActive(false);
                    nameplateRight.SetActive( true );
                    nameplateRightText.text = curDialog.CurrentSpeaker;
                    //nameplateBox.offsetMin = new Vector2(-140f + 265f, -15f);
                    //nameplateBox.offsetMax = new Vector2(265, 15f);
                }
                else
                { //LEFT SIDE
                    nameplateRight.SetActive( false );
                    nameplateLeft.SetActive(true);
                    nameplateLeftText.text = curDialog.CurrentSpeaker;

                    // nameplateBox.offsetMin = new Vector2(-265f, -15f);
                    //nameplateBox.offsetMax = new Vector2(140f - 265f, 15);
                }

                //Change the text and the name of the speaker
                //Text is actually a child object of nameplate, will have to fetch it
                //nameplate.transform.GetChild(0).gameObject.GetComponent<Text>().text = curDialog.CurrentSpeaker;
                spokenText.text = curDialog.Line;

                //Swap the replacee's face with the image corresponding to newExpression
                //Check if this is the proper way to load sprites
                rightPortrait.sprite = Resources.Load<Sprite>
                    ("Characters/Test/" + curDialog.RightSpeaker + "/" + curDialog.RightExpr);
                leftPortrait.sprite = Resources.Load<Sprite>
                    ("Characters/Test/" + curDialog.LeftSpeaker + "/" + curDialog.LeftExpr);

                // Prevent double key presses
                yield return new WaitForSeconds(.2f);
                // Now wait for the next key, modify this to submit button?
                yield return new WaitWhile(() => !Input.GetButtonDown("Submit"));
            }

            if (destroyOnEnd)
            {
                //Clear the GUI from the screen
                Destroy(newCanvas);
                preservedCanvas = false;
            }
            else
            {
                //Leave last line preserved on screen, it will be updated when the next dialog loads
                destroyOnEnd = true; //reset to default so we don't leave false by accident
                preservedCanvas = true;
            }

            dialog = null; //reset the dialog

        }
    }
}
