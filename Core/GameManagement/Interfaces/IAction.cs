namespace Core.GameManagement.Interfaces
{

    public interface IAction
    {
        public int actionPointCost { get; }
        public ActionTypes actionType { get; }
        public void OnExecute();
    }
}