using SimpleModelWrapper.Core;
using SimpleModelWrapper.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Examples.Models.Simple
{
    //A random interface for some processing
    public interface IRandomFunctionality
    {
        void DoSomething();
    }

    //A logger example interface
    public interface ILogger
    {
        void LogError(string errorMessage, string exceptionDetails);
        void LogValidation(string validationMessage, Dictionary<string, List<string>> errors);
    }

	//A generic request example
	public Request : IRequest
	{
		public string RandomProperty {get;set;}
	}
	
	//The model example
    public class SimpleExampleClass : BaseModel
    {
        private ILogger Logger { get; set; }        
        private IRandomFunctionality RandomFunctionality { get; set; }

        public ModelResult Process(IRequest request, ILogger logger, IRandomFunctionality randomFunctionality)
        {
            Logger = logger;            
            RandomFunctionality = randomFunctionality;
            return base.Process(request);
        }

        protected override void CoreProcessing()
        {
            //do some generic processing for this model in here!            
            RandomFunctionality.DoSomething();

            //Adds our outcome to the modelresult
            ModelResult.Response = new { Message = "Success" };
            ModelResult.Status = HttpStatusCode.OK;
        }

        //Adds our personal logger hooks into the error and validation error process
        protected override void CaughtErrorExceptionHook(Exception exception)
        {
            Logger.LogError("Internal Error", exception.Message);

            //Adds our outcome to the modelresult
            ModelResult.Response = new { Message = "Internal Failure", Errors };
            ModelResult.Status = HttpStatusCode.InternalServerError;
        }
        protected override void CaughtValidationExceptionHook(ValidationException validationException)
        {
            Logger.LogValidation("Validation Errors", ValidationErrors);

            //Adds our outcome to the modelresult
            ModelResult.Response = new { Message = "Validation Failure", Errors = ValidationErrors };
            ModelResult.Status = HttpStatusCode.BadRequest;
        }
    }

	//The API example
	[ApiController]
    public class RandomController : ControllerBase
    {
        private readonly ILogger _Logger;
        private readonly IRandomFunctionality _RandomFunctionality;
        public RandomController(ILogger logger, IRandomFunctionality randomFunctionality)
        {
            _Logger = logger;
            _RandomFunctionality = randomFunctionality;
        }

        [HttpGet]
        public ActionResult Example(Request request)
        {
            SimpleExampleClass model = new SimpleExampleClass();
            ModelResult result = model.Process(request, _Logger, _RandomFunctionality);
            return StatusCode((int)result.Status, result.Response);
        }
    }
}