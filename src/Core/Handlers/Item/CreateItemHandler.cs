﻿using Core.Data;
using Core.Errors;
using Core.Requests.Item;
using FluentResults;
using FluentValidation;
using MediatR;

namespace Core.Handlers.Item;

public class CreateItemHandler : IRequestHandler<CreateItem, Result<Models.Item>>
{
    private readonly IAppDbContext _context;
    private readonly IValidator<CreateItem> _validator;

    public CreateItemHandler(IAppDbContext context, IValidator<CreateItem> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<Result<Models.Item>> Handle(CreateItem request, CancellationToken cancellationToken)
    {
        var result = new Result<Models.Item>();
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return result.WithErrors(validationResult.Errors.Select(e => new ValidationError(e.ErrorMessage)));
        }

        var newItem = new Models.Item
        {
            Name = request.Name
        };

        await _context.Items.AddAsync(newItem, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return result.WithValue(newItem);
    }
}