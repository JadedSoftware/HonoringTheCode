using Core.Unit.AI.Goal;

namespace Core.AI
{
    public class ActionPlanEstimation
    {
        public AiActionPlan actionPlan;
        public float totalDamage;
        public UnitCommon unitCommon;
        public int actionPoints;
        public float health;
        public float sheilds;

        public ActionPlanEstimation(UnitCommon unitCommon, int actionPoints, float health, float sheilds)
        {
            this.unitCommon = unitCommon;
            this.actionPoints = actionPoints;
            this.health = health;
            this.sheilds = sheilds;
        }
    }
}