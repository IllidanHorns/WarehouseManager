using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehouseManager.Services.Exceptions
{
    public class ModelValidationException : Exception
    {
        public List<ValidationFailure> Errors { get; }

        public ModelValidationException(List<ValidationFailure> errors)
            : base("Произошла ошибка валидации")
        {
            Errors = errors;
        }
    }
}
