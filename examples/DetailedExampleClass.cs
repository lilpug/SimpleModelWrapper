using SimpleModelWrapper.Core;
using SimpleModelWrapper.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;

namespace Examples.Models
{
    //A random interface for some processing
    public interface IRandomFunctionality
    {
        object DoSomething(object details);
    }

    //A repo example interface
    public interface IRepository
    {
        object GetDetails();
        void SaveDetails(object details);
        void SaveRequestTime(Stopwatch timer);
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
    public class DetailedExampleClass : BaseModel
    {
        private ILogger Logger { get; set; }
        private IRepository Repo { get; set; }
        private IRandomFunctionality RandomFunctionality { get; set; }

        protected object RandomResults { get; set; }

        public ModelResult Process(IRequest request, ILogger logger, IRepository repo, IRandomFunctionality randomFunctionality)
        {
            Logger = logger;
            Repo = repo;
            RandomFunctionality = randomFunctionality;
            return base.Process(request);
        }

        protected override void CoreProcessing()
        {
            //do some generic processing for this model in here!
            var details = Repo.GetDetails();
            RandomFunctionality.DoSomething(details);

            //Adds our outcome to the modelresult
            ModelResult.Response = new { Message = "Success", Results = RandomResults };
            ModelResult.Status = HttpStatusCode.OK;
        }

        protected override void ValidationProcessingHook()
        {
            //Checks the ModelState validation if the modelstate was supplied
            base.ValidationProcessingHook();

            //Lets say we do some extra validation checks by checking a cache or database.
            //That can be done here!
            //ValidationErrors.Add("something failed validation", new List<string>() { "invalid type", "invalid cast" });
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

        //Adds our extra logic to do at the end of the processing regardless on if it went well or not
        protected override void FinalExtrasHook(bool ErrorOccurred, Stopwatch modelTime)
        {
            Repo.SaveRequestTime(modelTime);
            Repo.SaveDetails(RandomResults);
        }
    }
	
	//The API example
	[ApiController]
    public class RandomController : ControllerBase
    {
        private readonly ILogger _Logger;
		private readonly IRepository _Repo;
        private readonly IRandomFunctionality _RandomFunctionality;
        public RandomController(ILogger logger, IRepository repo, IRandomFunctionality randomFunctionality)
        {
            _Logger = logger;
			_Repo = repo;
            _RandomFunctionality = randomFunctionality;
        }

        [HttpGet]
        public ActionResult Example(Request request)
        {
            DetailedExampleClass model = new DetailedExampleClass();
            ModelResult result = model.Process(request, _Logger, _Repo, _RandomFunctionality);
            return StatusCode((int)result.Status, result.Response);
        }
    }
}