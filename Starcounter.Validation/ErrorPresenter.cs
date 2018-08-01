using System.Collections.Generic;

namespace Starcounter.Validation
{
    /// <summary>
    /// Used by <see cref="IValidator"/> to present validation errors.
    /// </summary>
    /// <param name="propertyName">The name of C# property that has a validation error.</param>
    /// <param name="errors">The human readable error messages</param>
    public delegate void ErrorPresenter(string propertyName, IEnumerable<string> errors);
}