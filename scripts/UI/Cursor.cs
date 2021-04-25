using Game.State;
using Godot;
using GodotUtilities;

namespace Game.UI
{
    public class Cursor : CanvasLayer
    {
        private const string ANIM_DEFAULT = "default";

        [Node("Node2D/Sprite")]
        private Sprite sprite;
        [Node("Node2D/Control")]
        private Control control;
        [Node("Node2D/Control/PanelContainer")]
        private PanelContainer panelContainer;
        [Node("Node2D/Control/PanelContainer/Label")]
        private Label label;
        [Node("Node2D/Control/PanelContainer/AnimationPlayer")]
        private AnimationPlayer animationPlayer;

        private Node tooltipOwner;

        public override void _EnterTree()
        {
            this.WireNodes();
            GameState.CreateEffect<BoardActions.ShowTooltip>(this, nameof(ShowTooltipEffect));
            GameState.CreateEffect<BoardActions.ClearTooltip>(this, nameof(ClearTooltipEffect));
        }

        public override void _Ready()
        {
            UpdateTooltip(null);
        }

        public override void _Process(float delta)
        {
            sprite.GlobalPosition = sprite.GetGlobalMousePosition();
            panelContainer.RectSize = Vector2.Zero;
            panelContainer.RectPivotOffset = panelContainer.RectSize / 2f;
            panelContainer.RectPosition = new Vector2(-panelContainer.RectPivotOffset.x, panelContainer.RectSize.y);
            control.RectGlobalPosition = sprite.GlobalPosition;
        }

        private void UpdateTooltip(string text)
        {
            if (animationPlayer.IsPlaying())
            {
                animationPlayer.Seek(0f, true);
                animationPlayer.Stop();
            }

            label.Text = text;
            if (!string.IsNullOrEmpty(text))
            {
                animationPlayer.Play(ANIM_DEFAULT);
            }
            else
            {
                panelContainer.Visible = false;
            }
        }

        private void ShowTooltipEffect(BoardActions.ShowTooltip showTooltip)
        {
            tooltipOwner = showTooltip.Owner;
            UpdateTooltip(showTooltip.Text);
        }

        private void ClearTooltipEffect(BoardActions.ClearTooltip clearTooltip)
        {
            if (tooltipOwner == clearTooltip.Owner || clearTooltip.Force)
            {
                UpdateTooltip(null);
            }
        }
    }
}
