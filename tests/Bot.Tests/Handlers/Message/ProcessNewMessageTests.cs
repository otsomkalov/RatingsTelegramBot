﻿using System.Threading;
using System.Threading.Tasks;
using Bot.Constants;
using Bot.Handlers.Message;
using Bot.Requests.InlineKeyboardMarkup;
using Bot.Requests.Message;
using Bot.Resources;
using Core.Models;
using Core.Requests.Item;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Telegram.Bot;
using Xunit;

namespace Bot.Tests.Handlers.Message;

public class ProcessNewMessageTests
{
    private readonly IStringLocalizer<Messages> _localizer;
    private readonly IMediator _mediator;
    private readonly ITelegramBotClient _telegramBotClient;

    public ProcessNewMessageTests()
    {
        _mediator = Substitute.For<IMediator>();
        _localizer = Substitute.For<IStringLocalizer<Messages>>();
        _telegramBotClient = Substitute.For<ITelegramBotClient>();
    }

    [Fact]
    public async Task MessageFromBot_Works()
    {
        // Arrange

        var handler = new ProcessMessageHandler(_localizer, _mediator, _telegramBotClient);

        var message = new Telegram.Bot.Types.Message
        {
            From = new()
            {
                IsBot = true
            },
            Text = "test"
        };

        var request = new ProcessMessage(message);

        // Act

        await handler.Handle(request, CancellationToken.None);

        // Assert
    }

    [Fact]
    public async Task MessageEmptyText_Works()
    {
        // Arrange

        var handler = new ProcessMessageHandler(_localizer, _mediator, _telegramBotClient);

        var message = new Telegram.Bot.Types.Message
        {
            From = new(),
            Text = string.Empty
        };

        var request = new ProcessMessage(message);

        // Act

        await handler.Handle(request, CancellationToken.None);

        // Assert
    }

    [Theory]
    [InlineData(Commands.Start)]
    [InlineData(Commands.NewCategory)]
    [InlineData(Commands.NewItem)]
    [InlineData(Commands.NewManufacturer)]
    [InlineData(Commands.NewPlace)]
    public async Task MessageTextCommands_ExecutesCommands(string command)
    {
        // Arrange

        var handler = new ProcessMessageHandler(_localizer, _mediator, _telegramBotClient);

        var message = new Telegram.Bot.Types.Message
        {
            From = new(),
            Text = command
        };

        var request = new ProcessMessage(message);

        // Act

        await handler.Handle(request, CancellationToken.None);

        // Assert

        await _mediator.Received().Send(Arg.Any<IRequest>());
    }

    [Fact]
    public async Task ReplyToMessageNewItemCommand_ExecutesCommand()
    {
        // Arrange

        const string command = "command";

        _localizer[nameof(Messages.NewItemCommand)].Returns(new LocalizedString(nameof(Messages.NewItemCommand), command));

        _mediator.Send(Arg.Any<CreateItem>())
            .Returns(Result.Ok(new Item
            {
                Id = 1
            }));

        var handler = new ProcessMessageHandler(_localizer, _mediator, _telegramBotClient);

        var message = new Telegram.Bot.Types.Message
        {
            From = new(),
            Text = "test",
            ReplyToMessage = new()
            {
                Text = command
            }
        };

        var request = new ProcessMessage(message);

        // Act

        await handler.Handle(request, CancellationToken.None);

        // Assert

        await _mediator.Received().Send(Arg.Any<GetInlineKeyboardMarkup>());
    }

    [Theory]
    [InlineData(nameof(Messages.NewManufacturerCommand))]
    [InlineData(nameof(Messages.NewCategoryCommand))]
    [InlineData(nameof(Messages.NewPlaceCommand))]
    public async Task ReplyToMessageCommands_ExecutesCommands(string messageCommand)
    {
        // Arrange

        const string command = "command";

        _localizer[messageCommand].Returns(new LocalizedString(messageCommand, command));

        var handler = new ProcessMessageHandler(_localizer, _mediator, _telegramBotClient);

        var message = new Telegram.Bot.Types.Message
        {
            From = new(),
            Text = "test",
            ReplyToMessage = new()
            {
                Text = command
            }
        };

        var request = new ProcessMessage(message);

        // Act

        await handler.Handle(request, CancellationToken.None);

        // Assert

        await _mediator.Received().Send(Arg.Any<IRequest<Result>>());
    }
}