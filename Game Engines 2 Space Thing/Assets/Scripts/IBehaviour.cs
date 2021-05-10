using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Interfaces
{
    interface IBehaviour
    {
        void OnIdle();
        void OnTargeting();
        void OnInCombat();
        void OnFlock();
        void OnReturning();
        void OnDamaged();
    }
}
