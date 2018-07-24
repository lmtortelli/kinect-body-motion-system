using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightBuzz.Vituvius.Exergames.Kimos.WPF.Domain
{
    class Entidade
    {
        public JointType start { set; get; }
        public JointType pontoInteresse { set; get; }
        public JointType end { set; get; }

        public Entidade(JointType start, JointType center, JointType end)
        {
            this.start = start;
            this.pontoInteresse = center;
            this.end = end;
        }
    }
    
}
