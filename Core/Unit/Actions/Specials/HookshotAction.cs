using Core.GameManagement;
using Core.GameManagement.Interfaces;

namespace Core.Unit.Specials
{
    public class HookshotAction : ActionContainer
    {
        public UnitCommon selectedUnit;
        public IHookshotable target;
        public SpecialHookshot hookshotData;
        
        public HookshotAction(UnitCommon selectedUnit, IHookshotable target, SpecialHookshot hookshotData)
        {
            this.selectedUnit = selectedUnit;
            this.target = target;
            this.hookshotData = hookshotData;
        }
    }
}