using Microsoft.AspNetCore.Mvc.ModelBinding;
using SimpleModelWrapper.Core;

namespace SimpleModelWrapper.Interfaces
{
    public interface IModel
    {
        ModelResult Process(IRequest request);
        ModelResult Process(IRequest request, ModelStateDictionary modelState);
    }
}