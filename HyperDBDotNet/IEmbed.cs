using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;


namespace HyperDBDotNet {
    public interface IEmbed {

        public Vector<Double> GetVector(String Query);

        }
}
