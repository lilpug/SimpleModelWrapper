using Microsoft.AspNetCore.Mvc.ModelBinding;
using SimpleModelWrapper.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace SimpleModelWrapper.Core
{
    public abstract partial class BaseModel : IModel
    {
        /// <summary>
        /// A generic base function for setting up the request object
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual ModelResult Process(IRequest request)
        {
            Request = request;
            ModelState = null;

            return CoreWrapper();
        }

        /// <summary>
        /// A generic base function for setting up the request and model state objects
        /// </summary>
        /// <param name="request"></param>
        /// <param name="modelState"></param>
        /// <returns></returns>
        public virtual ModelResult Process(IRequest request, ModelStateDictionary modelState)
        {
            Request = request;
            ModelState = modelState;

            return CoreWrapper();
        }

        /// <summary>
        /// A function which wraps the major CoreProcessing in various hooks and checks
        /// </summary>
        /// <returns></returns>
        protected virtual ModelResult CoreWrapper()
        {
            Stopwatch StopWatch = new Stopwatch();
            bool ErrorOccurred = false;

            try
            {
                //Maps the properties
                MapRequestProperties();

                //Runs any of the validation which has been added
                ValidationProcessingHook();

                //Checks if we have any validation failures and if so then fires off the validation exception
                if (ValidationErrors.Count > 0)
                {
                    throw new ValidationException("Validation Failure");
                }

                //Runs the supplied main function wrapped in our error and timer checking
                CoreProcessing();
            }
            catch (ValidationException validationException)
            {
                ErrorOccurred = true;
                CaughtValidationExceptionHook(validationException);
            }
            catch (Exception exception)
            {
                ErrorOccurred = true;
                CaughtErrorExceptionHook(exception);
            }
            finally
            {
                //We should have a status set at this point, if not assume its internal error as it should be set by now!
                if (!ModelResult.Status.HasValue)
                {
                    ModelResult.Status = HttpStatusCode.InternalServerError;
                }
                StopWatch.Stop();
                FinalExtrasHook(ErrorOccurred, StopWatch);
            }

            return ModelResult;
        }

        /// <summary>
        /// A function which maps the requests public properties to our models instance
        /// </summary>
        protected virtual void MapRequestProperties()
        {
            var type = this.GetType();
            var properties = type.GetProperties()?.ToList();
            var requestType = Request?.GetType();
            if (properties != null && requestType != null)
            {
                //Loops over the request properties and checks if we have any matchs
                foreach (var requestProp in requestType.GetProperties())
                {
                    var propertyExists = properties.Where(p => p.Name == requestProp.Name);
                    if (propertyExists != null && propertyExists.FirstOrDefault() != null)
                    {
                        //Converts it into a physical object
                        var prop = propertyExists.First();

                        //sets the value of the property to the same as the request object as they are the same name
                        prop.SetValue(this, Convert.ChangeType(requestProp.GetValue(Request), prop.PropertyType));
                    }
                }
            }
        }
    }
}