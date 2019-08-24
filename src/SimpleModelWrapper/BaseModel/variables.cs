using Microsoft.AspNetCore.Mvc.ModelBinding;
using SimpleModelWrapper.Interfaces;
using System.Collections.Generic;

namespace SimpleModelWrapper.Core
{
    public abstract partial class BaseModel : IModel
    {
        //Generic constructor parameters        
        protected IRequest Request { get; set; }
        protected ModelStateDictionary ModelState { get; set; }

        //Internal usage
        protected ModelResult ModelResult { get; set; }
        protected Dictionary<string, List<string>> ValidationErrors { get; set; }
        protected List<Error> Errors { get; set; }

        /// <summary>
        /// A generic constructor for creating the ModelResult, ValidationErrors and Errors object
        /// </summary>
        public BaseModel()
        {
            ModelResult = new ModelResult();
            ValidationErrors = new Dictionary<string, List<string>>();
            Errors = new List<Error>();
        }
    }
}