using System.Collections.Generic;
using System.Linq;

namespace Starcounter.Validation.Tests
{
    public class ValidationResultsPresenters
    {
        public static void NullValidationResultsPresenter(string name, IEnumerable<string> errors)
        {
            errors.ToList(); // force enumeration
        }
    }
}