using UnityEngine;
using Unity.VisualScripting;
using Remedy.StateMachines.ForGameObjects;

namespace Remedy.StateMachines.VS
{
    [UnitCategory("State Machines")]
    [UnitTitle("Set Object's State")]
    public class SetState : Unit
    {
        public ControlInput In;
        public ControlOutput Out;
        public ValueInput GameObject;
        public ValueInput Name;

        protected override void Definition()
        {
            In = ControlInput("", (flow) =>
            {
                var gameObject = flow.GetValue<GameObject>(GameObject);
                gameObject.SetState(flow.GetValue<string>(Name));
                return Out;
            });
            Out = ControlOutput("");
            GameObject = ValueInput<GameObject>("Game Object", default);
            Name = ValueInput<string>("Name", "");
        }
    }
}