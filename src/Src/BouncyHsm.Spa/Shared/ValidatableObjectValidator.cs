using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Spa.Shared;

public sealed class ValidatableObjectValidator : ComponentBase, IDisposable
{
    private ValidationMessageStore validationMessageStore = default!;

    [CascadingParameter]
    private EditContext EditContext
    {
        get;
        set;
    } = default!;


    public override async Task SetParametersAsync(ParameterView parameters)
    {
        EditContext previousEditContext = EditContext;

        await base.SetParametersAsync(parameters);

        if (EditContext != previousEditContext)
        {
            EditContextChanged();
        }
    }

    public void Dispose()
    {
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
        this.EditContext.OnValidationRequested -= this.ValidationRequested;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
    }

    private void EditContextChanged()
    {
        this.validationMessageStore = new ValidationMessageStore(EditContext);
        HookUpEditContextEvents();
    }
    private void HookUpEditContextEvents()
    {

#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
        this.EditContext.OnValidationRequested += this.ValidationRequested;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
        // EditContext.OnFieldChanged += FieldChanged; //TODO: maybe use
    }

    private void ValidationRequested(object sender, ValidationRequestedEventArgs args)
    {
        if (this.EditContext.Model is IValidatableObject validatedObject)
        {
            this.validationMessageStore.Clear();

            IEnumerable<ValidationResult> validationResults = validatedObject.Validate(new ValidationContext(this.EditContext.Model));
            foreach (ValidationResult vr in validationResults)
            {
                foreach (string fieldName in vr.MemberNames)
                {
                    FieldIdentifier fi = new FieldIdentifier(this.EditContext.Model, fieldName);
                    this.validationMessageStore.Add(in fi, vr.ErrorMessage ?? string.Empty);
                }
            }
            this.EditContext.NotifyValidationStateChanged();
        }
    }
}