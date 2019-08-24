using SimpleModelWrapper.Core;
using SimpleModelWrapper.Interfaces;
using NUnit.Framework;
using System.Net;

namespace SimpleModelWrapper.UnitTests.CoreTests
{
    public class ValidMappingModel : BaseModel
    {
        public string RandomProperty { get; set; }
        protected override void CoreProcessing()
        {
            if (RandomProperty == "TestValue")
            {
                ModelResult.Response = true;
                ModelResult.Status = HttpStatusCode.OK;
            }
            else
            {
                ModelResult.Response = false;
                ModelResult.Status = HttpStatusCode.BadRequest;
            }
        }
    }

    public class InvalidMappingModel : BaseModel
    {
        private string RandomProperty { get; set; }
        protected override void CoreProcessing()
        {
            if (RandomProperty == "TestValue")
            {
                ModelResult.Response = true;
                ModelResult.Status = HttpStatusCode.OK;
            }
            else
            {
                ModelResult.Response = false;
                ModelResult.Status = HttpStatusCode.BadRequest;
            }
        }
    }

    public class Request : IRequest
    {   
        public string RandomProperty { get; set; }

        public Request()
        {
            RandomProperty = "TestValue";
        }
    }
    
    public class CoreTests
    {
        private IRequest Request { get; set; }

        [SetUp]
        public void Setup()
        {
            Request = new Request();            
        }

        [Test]
        public void ValidMapRequestProperties()
        {
            ValidMappingModel model = new ValidMappingModel();
            var results = model.Process(Request);

            Assert.IsTrue(results != null && results.Response != null & results.Status != null);
            Assert.IsTrue(results.Response is bool r && r && results.Status == HttpStatusCode.OK);
            Assert.IsTrue(model.RandomProperty == "TestValue");
        }

        [Test]
        public void InvalidMapRequestProperties()
        {
            InvalidMappingModel model = new InvalidMappingModel();
            var results = model.Process(Request);

            Assert.IsTrue(results != null && results.Response != null & results.Status != null);
            Assert.IsTrue(results.Response is bool r && !r && results.Status == HttpStatusCode.BadRequest);            
        }
    }
}
