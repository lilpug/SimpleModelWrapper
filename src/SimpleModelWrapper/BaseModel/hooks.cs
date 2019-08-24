using SimpleModelWrapper.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;

namespace SimpleModelWrapper.Core
{
    public abstract partial class BaseModel : IModel
    {
        /// <summary>
        /// A function for checking for any validation errors before continueing the process
        /// Note: If the ModelState is set, this will also check the errors inside of it.
        /// </summary>
        protected virtual void ValidationProcessingHook()
        {
            if (ModelState != null && !ModelState.IsValid)
            {
                var modelstateGroupFilter = from ms in ModelState
                                            where ms.Value.Errors.Any()

                                            //Stores the field name
                                            let fieldKey = ms.Key

                                            //Gets all the current error messages for this field into string format
                                            let errors = (from error in ms.Value.Errors select error.ErrorMessage)

                                            //Builds a new keyvalue pair with the key and list of error values
                                            select new KeyValuePair<string, List<string>>(fieldKey, errors.ToList());
                if (modelstateGroupFilter != null && !modelstateGroupFilter.FirstOrDefault().Equals(default(KeyValuePair<string, List<string>>)))
                {
                    ValidationErrors = modelstateGroupFilter.ToDictionary(t => t.Key, t => t.Value);
                }
            }
        }

        /// <summary>
        /// A function which gets fired when a validation exception is thrown after failing anything in the ValidationProcessing function
        /// </summary>
        /// <param name="validationException"></param>
        protected virtual void CaughtValidationExceptionHook(ValidationException validationException) { }

        /// <summary>
        /// A function which gets fired if any exceptions are thrown inside of the CoreProcessing function
        /// </summary>
        /// <param name="exception"></param>
        protected virtual void CaughtErrorExceptionHook(Exception exception) { }

        /// <summary>
        /// A function which runs at the end of the CoreProcessing function inside a final block
        /// </summary>
        /// <param name="ErrorOccurred"></param>
        protected virtual void FinalExtrasHook(bool ErrorOccurred, Stopwatch modelTime) { }

        //This is the core method which will get overriden to input the logic for that particular models process
        protected abstract void CoreProcessing();
    }
}