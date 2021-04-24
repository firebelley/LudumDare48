using GodotUtilities.StateManagement;

namespace Game.State
{
    public class BoardReducer : Reducer<BoardState>
    {
        public override void Reduce(BoardState state, BaseAction action)
        {
            switch (action)
            {
                case BoardActions.BuildingSelected _:
                    state.BuildingSelected = true;
                    break;
                case BoardActions.BuildingDeselected _:
                    state.BuildingSelected = false;
                    break;
                case BoardActions.ResourcesGained resourcesGained:
                    state.ResourceCount += resourcesGained.Count;
                    break;
                case BoardActions.ResourcesSpent resourcesSpent:
                    state.ResourceCount -= resourcesSpent.Count;
                    break;
            }
        }
    }
}
