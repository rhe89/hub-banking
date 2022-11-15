using System.Collections.Generic;

namespace Banking.Web.WebApp.Models.Form;

public interface IValidatableModel
{
    IDictionary<string, string> ValidationErrors { get; }
    void Validate(out bool isValid);
}