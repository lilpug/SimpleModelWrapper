# SimpleModelWrapper
SimpleModelWrapper is an ASP.NET Core Model wrapper that takes all the hard work away and allows you to focus more on the processing functionality then setting up the correct model structure.

[![The MIT License](https://img.shields.io/badge/license-MIT-orange.svg?style=flat-square&maxAge=3600)](https://raw.githubusercontent.com/lilpug/SimpleModelWrapper/master/LICENSE)
[![NuGet](https://img.shields.io/nuget/v/SimpleModelWrapper.svg?maxAge=3600)](https://www.nuget.org/packages/SimpleModelWrapper/)


## General Information

The SimpleModelWrapper is a generic wrapper which takes care of firing validation, error try catches and returning various responses with a HttpStatusCode. When a SimpleModelWrapper object is created and the main processing function is executed, it returns a ModelResult object after calling all the various hooks which have been supplied for that particular instance.

by default, when creating a BaseModel object, if a ModelStateDictionary is supplied along with the request, it will also validate it and place any errors into the protected ValidationErrors Dictionary variable. This will then allow you to return it and the HttpStatusCode in the Validation hook function however you would prefer, alternatively this can be overridden by simply not calling the base function in the overriding Validation hook.

A BaseModel implementation has both validation and generic error variables which can be used to store and process any error or validation details which flag up in the supplied hooking functions.

The SimpleModelWrapper also houses functionality to map any public properties from the request object supplied to the current implementation of the inheriting BaseModel. For example, if the request object had a public property of Name and you placed the same public property of Name into the inheriting implementation of the BaseModel, then the Name property would automatically be set to the same as the supplied request object.

## General Structure

### Optional Hooks
Their are 4 hooking methods which can be used but are optional and don't need to be supplied.
```c#
        /// <summary>
        /// A function for checking for any validation errors before continueing the process
        /// Note: If the ModelState is supplied, this will also check the errors inside of it.
        /// </summary>
        protected virtual void ValidationProcessingHook();
        
        /// <summary>
        /// A function which gets fired when a validation exception is thrown after failing anything in the ValidationProcessing function
        /// </summary>
        /// <param name="validationException"></param>
        protected virtual void CaughtValidationExceptionHook(ValidationException validationException);
        
        /// <summary>
        /// A function which gets fired if any exceptions are thrown inside of the CoreProcessing function
        /// </summary>
        /// <param name="exception"></param>
        protected virtual void CaughtErrorExceptionHook(Exception exception);
        
        /// <summary>
        /// A function which runs at the end of the CoreProcessing function inside a final block
        /// </summary>
        /// <param name="ErrorOccurred"></param>
        protected virtual void FinalExtrasHook(bool ErrorOccurred, Stopwatch modelTime);
```

### Mandatory hook
The Mandatory hook is were the main processing functionality should be placed. This function will only be executed if the validation hook is successful, otherwise it will simply return what was specified in the validation hooking process.
```c#
        protected abstract void CoreProcessing();
```

## Getting Started
* Download SimpleModelWrapper from NuGet.

## How To Use

To create a SimpleModelWrapper model, simply inherit the BaseModel abstract class into your class.
```c#
	public class YourClass : BaseModel
	{
	}
```

you can then override any of the hooks through the process to build your own functionality.
```c#
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
```

Once your model is ready simply call the process function in your controller with or without the ModelState object and then return the response and status code supplied in the ModelResult object.
```c#
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
```

## Examples

For further examples, see the example folder which have a basic and a more detailed example of how to fully utilise this library.

## Copyright and License
Copyright &copy; 2019 David Whitehead

This project is licensed under the MIT License.
