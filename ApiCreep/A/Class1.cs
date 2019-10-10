using B;
using System;

namespace A
{
    public class AClass
    {
        private BClass _bclass;

        public AClass(BClass bclass)
        {
            _bclass = bclass ?? throw new ArgumentNullException(nameof(bclass));
        }

        public override string ToString()
        {
            return "AClass - " + _bclass.ToString();
        }
    }
}
