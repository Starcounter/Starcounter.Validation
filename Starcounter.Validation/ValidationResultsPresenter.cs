using System.Collections.Generic;

namespace Starcounter.Validation
{
    /// <summary>
    /// Used by <see cref="IValidator"/> to present validation results.
    /// </summary>
    /// <param name="propertyName">The name of C# property for which the results should be presented.</param>
    /// <param name="errors">The human readable error messages. Empty if validation has passed.</param>
    public delegate void ValidationResultsPresenter(string propertyName, IEnumerable<string> errors);
}