using System.Collections.Generic;
using System.Linq;

namespace Forbbiden.Client.Model
{
    public class ValidationResults
    {
        public bool IsValid => !Errors.Any();
        public List<ValidationErrorCodes> Errors { get; } = new List<ValidationErrorCodes>();

        public ValidationResults()
        {
        }
    }
}
