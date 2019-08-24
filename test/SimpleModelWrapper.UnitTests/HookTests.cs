using Microsoft.AspNetCore.Mvc.ModelBinding;
using SimpleModelWrapper.Core;
using SimpleModelWrapper.Interfaces;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;

namespace SimpleModelWrapper.UnitTests.HookTests
{
    public class SuccessfulModel : BaseModel
    {
        protected override void CoreProcessing()
        {
            ModelResult.Response = true;
            ModelResult.Status = HttpStatusCode.OK;
        }
    }

    public class ValidationFailureModel : BaseModel
    {
        //should never reach this
        protected override void CoreProcessing()
        {
            ModelResult.Response = true;
            ModelResult.Status = HttpStatusCode.OK;
        }

        protected override void ValidationProcessingHook()
        {
            ValidationErrors.Add("Testing", new List<string>() { "cause", "Validation", "Failure" });
        }

        protected override void CaughtValidationExceptionHook(ValidationException validationException)
        {
            ModelResult.Response = false;
            ModelResult.Status = HttpStatusCode.BadRequest;
        }
    }

    public class ErrorFailureModel : BaseModel
    {   
        protected override void CoreProcessing()
        {
            throw new Exception("Fire an error");
        }

        protected override void CaughtErrorExceptionHook(Exception exception)
        {
            ModelResult.Response = false;
            ModelResult.Status = HttpStatusCode.InternalServerError;
        }
    }

    public class Request : IRequest
    {   
    }
    
    public class HookTests
    {   
        private IRequest Request { get; set; }

        [SetUp]
        public void Setup()
        {
            Request = new Request();            
        }

        [Test]
        public void CoreProcessing()
        {
            SuccessfulModel model = new SuccessfulModel();
            var results = model.Process(Request);
            Assert.IsTrue(results != null && results.Response != null && results.Status != null);
            Assert.IsTrue(results.Response is bool r && r && results.Status == HttpStatusCode.OK);
        }

        [Test]
        public void ValidationHook()
        {
            int validationCalledCount = 0;
            Mock<SuccessfulModel> mockModel = new Mock<SuccessfulModel>() { CallBase = true };
            mockModel.Protected().Setup("ValidationProcessingHook").Callback(() =>
            {
                validationCalledCount++;
            });

            SuccessfulModel model = mockModel.Object;
            var result = model.Process(Request);

            Assert.IsTrue(result != null && result.Response != null && result.Status != null);
            Assert.IsTrue(result.Response is bool r && r && result.Status == HttpStatusCode.OK);
            Assert.IsTrue(validationCalledCount == 1);
        }

        [Test]
        public void FinalExtrasHook()
        {
            int finalExtrasCount = 0;
            Mock<SuccessfulModel> mockModel = new Mock<SuccessfulModel>() { CallBase = true };
            mockModel.Protected().Setup("FinalExtrasHook", ItExpr.IsAny<bool>(), ItExpr.IsAny<Stopwatch>()).Callback(() =>
            {
                finalExtrasCount++;
            });

            SuccessfulModel model = mockModel.Object;
            var result = model.Process(Request);

            Assert.IsTrue(result != null && result.Response != null && result.Status != null);
            Assert.IsTrue(result.Response is bool r && r && result.Status == HttpStatusCode.OK);
            Assert.IsTrue(finalExtrasCount == 1);
        }
        
        [Test]
        public void CaughtValidationExceptionHook()
        {   
            ValidationFailureModel model = new ValidationFailureModel();
            var result = model.Process(Request);

            Assert.IsTrue(result != null && result.Response != null && result.Status != null);
            Assert.IsTrue(result.Response is bool r && !r && result.Status == HttpStatusCode.BadRequest);            
        }

        [Test]
        public void CaughtModelStateValidationExceptionHook()
        {
            ModelStateDictionary modelState = new ModelStateDictionary();
            modelState.AddModelError("RandomProperty", "has invalid characters.");

            ValidationFailureModel model = new ValidationFailureModel();
            var result = model.Process(Request, modelState);

            Assert.IsTrue(result != null && result.Response != null && result.Status != null);
            Assert.IsTrue(result.Response is bool r && !r && result.Status == HttpStatusCode.BadRequest);
        }

        [Test]
        public void CaughtErrorExceptionHook()
        {   
            ErrorFailureModel model = new ErrorFailureModel();
            var result = model.Process(Request);

            Assert.IsTrue(result != null && result.Response != null && result.Status != null);
            Assert.IsTrue(result.Response is bool r && !r && result.Status == HttpStatusCode.InternalServerError);
        }
    }
}
