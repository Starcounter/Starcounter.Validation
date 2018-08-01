using System.Collections.Generic;
using System.Linq;

namespace Starcounter.Validation.Tests
{
    public class ErrorPresenters
    {
        public static void NullErrorPresenter(string name, IEnumerable<string> errors)
        {
            errors.ToList(); // force enumeration
        }
    }
}