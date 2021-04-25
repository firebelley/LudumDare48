using Godot;
using GodotUtilities;

namespace Game.UI
{
    public class TutorialSlideshow : CanvasLayer
    {
        private const int TUTORIAL_STEPS = 5;
        private int currentTutorialStep;

        [Node("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/ButtonContainer/BackButton")]
        private Button backButton;
        [Node("MarginContainer/PanelContainer/MarginContainer/VBoxContainer/ButtonContainer/NextButton")]
        private Button nextButton;
        [Node("MarginContainer/PanelContainer/MarginContainer/VBoxContainer")]
        private VBoxContainer screenContainer;

        public override void _EnterTree()
        {
            this.WireNodes();
        }

        public override void _Ready()
        {
            nextButton.Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array { 1 });
            backButton.Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array { -1 });
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            screenContainer.GetNodesOfType<VBoxContainer>().ForEach(x => x.Visible = false);
            screenContainer.GetNode<VBoxContainer>($"{currentTutorialStep + 1}").Visible = true;
            backButton.Visible = currentTutorialStep > 0;
            nextButton.Text = currentTutorialStep == TUTORIAL_STEPS - 1 ? "Ok!" : "Next";
        }

        private void OnButtonPressed(int change)
        {
            currentTutorialStep += change;
            if (currentTutorialStep == TUTORIAL_STEPS)
            {
                QueueFree();
            }
            else
            {
                UpdateDisplay();
            }
        }
    }
}
